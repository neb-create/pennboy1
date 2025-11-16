using UnityEngine;
using System.Collections.Generic;
using System;
using TMPro;
using System.Diagnostics;


public class SlideNote : Note3D
{
    [SerializeField] public float startTime;
    [SerializeField] public float endTime;
    public int maxDist = 2;               // ORIGINAL meaning: max horizontal index jump allowed
    public KeyCode startKey;
    public KeyCode endKey;
    public int numKeys;
    public List<(KeyCode, float)> progress = new List<(KeyCode, float)>();
    const float PERFECT_THRESHOLD = 0.4F;

    const float approachTime = 1.5f;
    const float tolerance = 0.4f;

    public bool done;

    public GameObject obj;
    public GameObject other;              // optional world-space end marker
    private SpriteRenderer spriteRenderer;

    private Vector3 velocity = Vector3.zero;

    // movement tuning
    [SerializeField] private float baseMoveSpeed = 10.0f; // fallback
    [SerializeField] private float maxSpeed = 30f;       // capped predictive speed
    [SerializeField] private float minSpeed = 3f;        // minimum speed when presses are slow
    [SerializeField] private float smoothFactor = 12f;   // how quickly position LERPs to target

    private Vector3 startPos;
    private Vector3 endPos;
    private Vector3 targetPos;
    [SerializeField] private float slideLength = 2f;    // world-units if 'other' not provided

    // press timing memory (used to compute tempo)
    private Queue<float> recentIntervals = new Queue<float>();
    [SerializeField] private int intervalMemory = 4;    // how many intervals to average

    [SerializeField] private ParticleSystem moveParticles;
    private ParticleSystem.EmissionModule particleEmission;

    [SerializeField] private TMP_Text letterText;
    [SerializeField] private Material noteTextMaterial;

    [SerializeField] private float fadeOffset = 0.5f; // extra lead time before approach
    [SerializeField] private GameObject ring;        // reference to ring GameObject
    private SpriteRenderer ringRenderer;
    private float ringStartScale = 1.0f;
    private float ringEndScale = 0.3f;  // full size when it hits the circle

    public float accuracy;

    private bool velocityLocked = false;
    [SerializeField] private float destroyDelay = 2f; // seconds before deletion

    private int dirSign;
    private bool reverse = false;

    // keep your key mapping (trimmed here for brevity). Add keys you need.
    public static Dictionary<KeyCode, (int, int)> keys = new Dictionary<KeyCode, (int, int)>()
    {
        [KeyCode.BackQuote] = (0, 1),
        [KeyCode.Alpha1] = (0, 2),
        [KeyCode.Alpha2] = (0, 3),
        [KeyCode.Alpha3] = (0, 4),
        [KeyCode.Alpha4] = (0, 5),
        [KeyCode.Alpha5] = (0, 6),
        [KeyCode.Alpha6] = (0, 7),
        [KeyCode.Alpha7] = (0, 8),
        [KeyCode.Alpha8] = (0, 9),
        [KeyCode.Alpha9] = (0, 10),
        [KeyCode.Alpha0] = (0, 11),
        [KeyCode.Minus] = (0, 12),
        [KeyCode.Equals] = (0, 13),
        [KeyCode.Q] = (1, 2),
        [KeyCode.W] = (1, 3),
        [KeyCode.E] = (1, 4),
        [KeyCode.R] = (1, 5),
        [KeyCode.T] = (1, 6),
        [KeyCode.Y] = (1, 7),
        [KeyCode.U] = (1, 8),
        [KeyCode.I] = (1, 9),
        [KeyCode.O] = (1, 10),
        [KeyCode.P] = (1, 11),
        [KeyCode.LeftBracket] = (1, 12),
        [KeyCode.RightBracket] = (1, 13),
        [KeyCode.Backslash] = (1, 14),
        [KeyCode.A] = (2, 2),
        [KeyCode.S] = (2, 3),
        [KeyCode.D] = (2, 4),
        [KeyCode.F] = (2, 5),
        [KeyCode.G] = (2, 6),
        [KeyCode.H] = (2, 7),
        [KeyCode.J] = (2, 8),
        [KeyCode.K] = (2, 9),
        [KeyCode.L] = (2, 10),
        [KeyCode.Semicolon] = (2, 11),
        [KeyCode.Quote] = (2, 12),
        [KeyCode.Z] = (3, 2),
        [KeyCode.X] = (3, 3),
        [KeyCode.C] = (3, 4),
        [KeyCode.V] = (3, 5),
        [KeyCode.B] = (3, 6),
        [KeyCode.N] = (3, 7),
        [KeyCode.M] = (3, 8),
        [KeyCode.Comma] = (3, 9),
        [KeyCode.Period] = (3, 10),
        [KeyCode.Slash] = (3, 11)
    };
    void OnDestroy()
    {
        UnityEngine.Debug.Log($"DESTROYED: {name}", this);
        UnityEngine.Debug.Log(System.Environment.StackTrace);
    }

    public enum SlideResult { InProgress, Passed, Failed }

    public SlideResult GetResult()
    {
        if (done) return SlideResult.Passed;

        // fail if past endTime + tolerance and not done
        if (!done && Time.time > endTime + tolerance)
            return SlideResult.Failed;

        return SlideResult.InProgress;
    }

    // returns true if input was accepted and added
    bool addInput(KeyCode hitKey, float time)
    {
        // guard: if the key isn't in the dictionary, ignore it
        if (!keys.ContainsKey(hitKey)) return false;

        (int, int) newKeyPos = keys[hitKey];
        (int newVertical, int newHorizontal) = newKeyPos;

        // first key: must equal startKey to start the slide
        if (progress.Count == 0)
        {
            if (startKey == hitKey)
            {
                progress.Add((hitKey, time));
                RecordPressInterval(time);
                RecomputeTargetPosition();
                return true;
            }
            return false;
        }

        // validate adjacency / same row etc (preserves original purpose of maxDist)
        (KeyCode, float) prevInput = progress[progress.Count - 1];
        (int prevVertical, int prevHorizontal) = keys[prevInput.Item1];

        int direction = keys[endKey].Item2 - keys[startKey].Item2;
        if (prevVertical != newVertical || Math.Abs(newHorizontal - prevHorizontal) > maxDist) return false;

        // ensure direction is consistent with slide direction
        if ((direction > 0 && newHorizontal - prevHorizontal > 0) ||
            (direction < 0 && newHorizontal - prevHorizontal < 0))
        {
            progress.Add((hitKey, time));
            RecordPressInterval(time);
            RecomputeTargetPosition();

            if (evaluateProgress())
            {
                done = true;
                velocityLocked = true;
                velocity = (targetPos - transform.position) * smoothFactor * (baseMoveSpeed / (maxSpeed + 0.01f));

                // Destroy object after delay
                UnityEngine.Debug.Log("Destroy obj due to success");
                Destroy(gameObject, destroyDelay);
                //if (spriteRenderer != null) spriteRenderer.color = new Color(0f, 1f, 0f, 1f);  // green
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Record the interval between this press and the last press.
    /// Keep only up to intervalMemory intervals.
    /// </summary>
    void RecordPressInterval(float now)
    {
        // If there's a previous press, compute interval and enqueue
        if (progress.Count >= 2)
        {
            // last press time is progress[progress.Count - 2].Item2 (because we already added current in addInput)
            float lastTime = progress[progress.Count - 2].Item2;
            float interval = Mathf.Max(0.0001f, now - lastTime); // avoid divide-by-zero

            recentIntervals.Enqueue(interval);
            if (recentIntervals.Count > intervalMemory) recentIntervals.Dequeue();
        }
        // Note: For the very first key press, there's no interval to record.
    }

    /// <summary>
    /// Recompute the world-space target position from progress.
    /// Uses (progress.Count - 1) / (numKeys - 1) so:
    ///   - first key results in fraction 0 -> startPos
    ///   - final key results in fraction 1 -> endPos
    /// </summary>
    void RecomputeTargetPosition()
    {
        if (numKeys <= 1)
        {
            targetPos = endPos;
            return;
        }

        // number of steps that have happened: progress.Count - 1 (0-based)
        float fraction = Mathf.Clamp01((float)(progress.Count - 1) / (float)(numKeys - 1));
        targetPos = Vector3.Lerp(startPos, endPos, fraction);

        // predict speed from recent intervals and adjust moveSpeed if desired
        float predictedSpeed = PredictSpeedFromIntervals();
        // blend predicted speed and baseMoveSpeed so it doesn't jump wildly
        baseMoveSpeed = Mathf.Clamp(Mathf.Lerp(baseMoveSpeed, predictedSpeed, 0.6f), minSpeed, maxSpeed);
    }

    /// <summary>
    /// Predict movement speed (units/sec) from averaged recent intervals.
    /// We map shorter average interval -> higher normalized tempo -> higher speed.
    /// </summary>
    float PredictSpeedFromIntervals()
    {
        if (recentIntervals.Count == 0) return baseMoveSpeed;

        float sum = 0f;
        foreach (var iv in recentIntervals) sum += iv;
        float avgInterval = sum / recentIntervals.Count; // seconds per step

        // map avgInterval to a 0..1 value where small intervals -> 1, large intervals -> 0
        // choose sensible bounds for interval (0.05s very fast, 0.8s slow)
        float minInterval = 0.05f;
        float maxInterval = 0.8f;
        float t = Mathf.Clamp01((maxInterval - avgInterval) / (maxInterval - minInterval)); // 0..1

        // then map to speed range
        return Mathf.Lerp(minSpeed, maxSpeed, t);
    }

    bool evaluateProgress()
    {
        float percentageDone = (0.0F + progress.Count) / numKeys;
        if (percentageDone >= PERFECT_THRESHOLD)
        {
            UnityEngine.Debug.Log("PERFECT");
            return true;
        }
        return false;
    }

    void Start()
    {
        UnityEngine.Debug.Log("Start called for slidenote " + startKey + " " + endKey + " " + startTime + " " + endTime);
        obj = this.gameObject;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

        startPos = transform.position;


        // attempt to infer direction from startKey/endKey horizontal indexes if possible
        dirSign = 1;
        if (keys.ContainsKey(startKey) && keys.ContainsKey(endKey))
        {
            dirSign = Math.Sign(keys[endKey].Item2 - keys[startKey].Item2);
            if (dirSign == 0) dirSign = 1;
        }

        // determine world-space endPos
        endPos = startPos + new Vector3(slideLength * dirSign, 0f, 0f);

        // flip sprite visually if sliding left
        
        if (dirSign < 0 && obj != null)
        {
            if (spriteRenderer != null)
                spriteRenderer.flipX = true;

            moveParticles.transform.rotation = Quaternion.Euler(0f, 90, 0f);
            // Flip particle system visually

            /*if (moveParticles != null)
            {
                Vector3 psScale = moveParticles.transform.localScale;
                psScale.x *= -1;
                moveParticles.transform.localScale = psScale;

                // Optional: flip particle velocity along X if using Velocity over Lifetime
                var velModule = moveParticles.velocityOverLifetime;
                velModule.enabled = true;
                velModule.x = new ParticleSystem.MinMaxCurve(-Mathf.Abs(velModule.x.constant));
            }*/
        }


        if (moveParticles != null)
        {
            particleEmission = moveParticles.emission;
            particleEmission.enabled = false; // start disabled
        }

        if (letterText != null)
        {
            letterText.text = startKey.ToString(); // show the key letter
        }

        if (ring != null)
        {
            ringRenderer = ring.GetComponent<SpriteRenderer>();
            ring.transform.localScale = Vector3.one * ringStartScale;
            ringRenderer.color = new Color(1f, 1f, 1f, 0f); // initially invisible
        }
        spriteRenderer.color = new Color(1f, 1f, 1f, 0f);
        letterText.color = new Color(1f, 1f, 1f, 0f);
        targetPos = startPos;

        if (keys.ContainsKey(startKey) && keys.ContainsKey(endKey))
        {
            // Get their horizontal positions
            int startIndex = keys[startKey].Item2;
            int endIndex = keys[endKey].Item2;

            // Include both ends of the range (+1)
            numKeys = Mathf.Abs(endIndex - startIndex) + 1;
        }
        else
        {
            // fallback if something went wrong
            numKeys = 1;
        }
    }

    void Update()
    {
        if (Time.time > endTime)
        {
            if ((Time.time - endTime) / 0.5f > 1.0f)
                Destroy(gameObject);
            else
            {
                float t = (Time.time - endTime) / 0.5f;
                Color c = spriteRenderer.color;
                c.a = Mathf.Clamp01(1.0f - t);
                spriteRenderer.color = c;

                Color textColor = letterText.color;
                textColor.a = Mathf.Clamp01(1.0f - t);
                letterText.color = textColor;
            }
        }

        if (Time.time >= startTime - tolerance)
        {
            // use GetKeyDown so each press is only added once
            foreach (var entry in keys)
            {
                if (Input.GetKeyDown(entry.Key))
                {
                    addInput(entry.Key, Time.time);
                }
            }
        }

        float noteFadeInStart = startTime - approachTime - fadeOffset; // note fade starts
        float noteFadeInEnd = startTime - approachTime;                // note fully visible
        float ringAnimStart = startTime - approachTime;               // ring starts enclosing
        float ringAnimEnd = startTime;                                // ring reaches final scale

        // Handle note fade-in
        if (spriteRenderer != null && Time.time >= noteFadeInStart && Time.time <= noteFadeInEnd)
        {
            float t = (Time.time - noteFadeInStart) / (noteFadeInEnd - noteFadeInStart);
            Color c = spriteRenderer.color;
            c.a = Mathf.Clamp01(t);
            spriteRenderer.color = c;

            if (letterText != null)
            {
                Color textColor = letterText.color;
                textColor.a = Mathf.Clamp01(t);
                letterText.color = textColor;
            }
        }

        // Handle ring enclosing animation
        if (ringRenderer != null && Time.time >= ringAnimStart && Time.time <= ringAnimEnd)
        {
            float t = (Time.time - ringAnimStart) / (ringAnimEnd - ringAnimStart);
            ring.transform.localScale = Vector3.one * Mathf.Lerp(ringStartScale, ringEndScale, t);

            Color rc = ringRenderer.color;
            rc.a = Mathf.Lerp(0.0f, 1f, t);
            ringRenderer.color = rc;
        }
        if (moveParticles != null)
        {
            particleEmission.enabled = progress.Count > 0; // threshold to avoid tiny jitters
        }

        if (!velocityLocked)
        {
            // normal smooth lerp toward target
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * smoothFactor * (baseMoveSpeed / (maxSpeed + 0.01f)));
        }
        else
        {

            if (Math.Abs(transform.position.x - startPos.x) > slideLength * 1.5f)
                reverse = true;
            if (reverse)
            {
                particleEmission.enabled = false;
                transform.position += -dirSign  * (new Vector3(1.0f,0.0f,0.0f)) * Time.deltaTime * 400.0f;
            }
                
            else
                transform.position += dirSign * (new Vector3(1.0f, 0.0f, 0.0f)) * Time.deltaTime * 200.0f;
        }
    }

    public override GameManager.NoteType GetNoteType()
    {
        return GameManager.NoteType.SLIDE;
    }
}
