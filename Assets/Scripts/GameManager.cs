using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Pool;
using System;

public class GameManager : MonoBehaviour
{

    // Score System
    [Header("Game Score")]
    public int score = 0;
    public int current_combo = 0;
    public int max_combo = 0;
    public int perfect_count = 0;
    public int great_count = 0;
    public int bad_count = 0;
    public int miss_count = 0;
    const int SCORE_PERFECT = 100;
    const int SCORE_GREAT = 50;
    const int SCORE_BAD = 0;
    const int PERFECT = 1;
    const int GREAT = 2;
    const int BAD = 3;
    const int MISS = 0;
    const float JUDGEMENT_PERFECT_WINDOW = 0.08f;
    const float JUDGEMENT_GREAT_WINDOW = 0.24f;
    const float JUDGEMENT_BAD_WINDOW = 0.32f;

    [Header("Game Settings")]
    // Note Spawning System
    public int bpm = 174;

    // Note Moving System
    List<GameObject> ActiveNotes = new List<GameObject>();
    Dictionary<KeyCode, GameObject> keyToObjMap = new Dictionary<KeyCode, GameObject>();
    public float note_speed = 9f;
    public float note_height_origin = 22f;
    float note_z_spawn = 12.0f;
    float note_z_despawn = -6.0f;
    float note_prespawn_time;
    public float time_current = -2.0f;
    float time_offset = 0.0f;
    float time_nextnote = 5.0f;

    // Input state var - 0 (space) 1 2 (left) 3 4 (right)
    //bool[] InputState = new bool[5];
    const int KEY_MIDDLE = 0;
    const int KEY_LEFT_1 = 1;
    const int KEY_LEFT_2 = 2;
    const int KEY_RIGHT_1 = 3;
    const int KEY_RIGHT_2 = 4;

    [Header("Note Prefab")]
    public GameObject NotePrefabTap;
    public GameObject NotePrefabHold;
    public GameObject NotePrefabSlide;
    public GameObject NotePrefabSwap;
    public Material NoteMaterialSpace;
    public GameObject NoteTriggerPrefab;
    public GameObject NoteTriggerPlayerPrefab;

    //VFX Prefabs
    [Header("VFX Prefabs")]
    public GameObject hitVFXPrefab;       
    public GameObject travelVFXPrefab;
    public GameObject slideVFXPrefab;

    [Header("Config")]
    // Player Object
    public GameObject player;
    public GameObject gameCamera;

    public static GameManager instance;

    // Note Types
    public enum NoteType { TAP, HOLD, SLIDE, SWAP };

    public enum GameState { RHYTHM, BULLET };
    public GameState gamestate = GameState.RHYTHM;
    private bool swapToBulletFlag = false;
    private bool swapToRhythmFlag = false;

    // Player Key 
    KeyCode playerKey = KeyCode.Space;
    // List of Keys used in gameplay
    List<KeyCode> keyList = new List<KeyCode>() { KeyCode.W, KeyCode.E, KeyCode.Space, KeyCode.I, KeyCode.O };

    // Beatmap Loading
    private List<Beatmap.NoteInfo> noteInfos;
    private int currNote;
    public bool randomSpawnMode = true;
    public bool spawnSwapNotes = true;

    [Header("UI/VFX")]
    // UI Text
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private TextMeshProUGUI scoreTypeText;
    [SerializeField] private GameObject comboVfx;
    private Color combo_inc_color;
    private Color combo_init_color;

    [Header("Bullet Hell")]
    public GameObject PlaceholderDefaultEmitter;
    public ObjectPool<GameObject> masterProjectilePool;
    public int defaultPoolSize = 50;
    public int maxPoolSize = 100;
    [SerializeField] GameObject bulletPrefab;
    [Header("int is number beats after prev one spawned or bullet hell began, whichever comes later")]
    public List<Pair> emitters = new List<Pair>();
    private int emitterIndex = 0;
    private float timePerBeat = 0.5f;
    private float timeLastEmitterSetActive;
    private float bulletHellStartTime;


    void swapGameState()
    {

        swapToRhythmFlag = false;
        swapToBulletFlag = false;

        // Clean Up player
        player.transform.position = Vector3.zero;
        player.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        player.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        player.GetComponent<Rigidbody>().Sleep();   
        // Clean Up rhythm game stuff
        keyToObjMap.Clear();
        for (int i = player.transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(player.transform.GetChild(i).gameObject);
        }
        for (int i = ActiveNotes.Count - 1; i >= 0; i--)
        {
            DestroyImmediate(ActiveNotes[i]);
        }
        ActiveNotes.Clear();

        if (gamestate == GameState.RHYTHM)
            gamestate = GameState.BULLET;
        else
            gamestate = GameState.RHYTHM;
    }

    void swapToRhythmGame()
    {
        //clear bullet hell stuff
        if (masterProjectilePool != null)
        {
            masterProjectilePool.Clear();
            Debug.Log("clear pool");
        }
        ClearBulletHell();
        
        // Swap: 
        gamestate = GameState.BULLET;
        swapGameState();

        // TODO: cool special effects
        gameCamera.GetComponent<GameCamera>().TransitionToRhythmGame();


        int i = -keyList.Count/2 - 1;
        foreach (KeyCode k in keyList)
        {
            i += 1;
            // Skip Player Key
            if (playerKey == k) continue;
            // Create Note Triggers
            GameObject newObj = Instantiate(NoteTriggerPrefab, player.transform);

            newObj.transform.localPosition += new Vector3( 1.32f * i, 0.12f * Mathf.Abs(i), 0f );

            newObj.GetComponent<NoteTrigger>().setKeyCode(k);

            keyToObjMap[k] = newObj;

        }
        
        // find the player note trigger object
        GameObject pObj = Instantiate(NoteTriggerPlayerPrefab, player.transform);
        pObj.GetComponent<NoteTrigger>().setKeyCode(KeyCode.Space);
        keyToObjMap[KeyCode.Space] = pObj;

    }

    void SpawnNote(KeyCode key, float time, NoteType type, float length = 0.0f) {

        GameObject NoteToSpawn;
        if (length <= 0.0f)
        {
            length = 60f/bpm/1f;
        }
        float time_end = time + length;
        switch (type)
        {
            case NoteType.HOLD:
                NoteToSpawn = NotePrefabHold;
                break;
            case NoteType.SWAP:
                NoteToSpawn = NotePrefabSwap;
                key = KeyCode.Space;
                break;
            case NoteType.SLIDE:
                NoteToSpawn = NotePrefabSlide;
                break;
            default:
                NoteToSpawn = NotePrefabTap;
                break;
        }
        
        GameObject new_note = Instantiate(NoteToSpawn, new Vector3(0f,0f,0f), Quaternion.identity);
        new_note.GetComponent<Note3D>().key = key;
        new_note.GetComponent<Note3D>().targetObject = keyToObjMap[key];

        new_note.GetComponent<Note3D>().time = time;
        if (type == NoteType.HOLD)
            new_note.GetComponent<HoldNote3D>().time_end = time_end;

        // Set Color
        if (key == KeyCode.Space)
        {
            Renderer[] childRenderers = new_note.GetComponentsInChildren<Renderer>();
            foreach (Renderer rend in childRenderers)
            {
                rend.material = NoteMaterialSpace;
            }
        }
        
        // Attach moving glow
        if (travelVFXPrefab != null)
        {
            GameObject trail = Instantiate(travelVFXPrefab, new_note.transform.position, Quaternion.identity, new_note.transform.GetChild(0));
        }

        ActiveNotes.Add(new_note);

    }
    void DestroyNote(GameObject note) {

        ActiveNotes.Remove(note);
        Destroy(note);
    }

    


    // Handle Input Scection - 
    // This part of the code handles the player action of pressing any of the keys.
    // Needs to update to adapt to being able to use any key..
    void HandleInput()
    {
        foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(key))
                if (keyToObjMap.ContainsKey(key))
                    HandleKeyDown(key);

            if (Input.GetKeyUp(key))
                if (keyToObjMap.ContainsKey(key))
                    HandleKeyUp(key);
        }
    }
    public (GameObject, float) GetClosestNote(float t, KeyCode k)
    {
        GameObject hit_note = null;
        float closest_note_distance = float.MaxValue;
        foreach (GameObject note in ActiveNotes) {

            float note_distance = note.GetComponent<Note3D>().GetDistance(t, k);
            if (note_distance < closest_note_distance) {
                hit_note = note;
                closest_note_distance = note_distance;
            }
        }

        return (hit_note, closest_note_distance);
    }
    public (GameObject, float) GetActiveHoldNote(float t, KeyCode k)
    {
        foreach (GameObject note in ActiveNotes) {

            // check if key matches
            if (note.GetComponent<Note3D>().key != k) continue;
            // check if it is a hold note
            if (!(note.GetComponent<Note3D>() is HoldNote3D)) continue;
            // check if it is on hold
            if (!note.GetComponent<HoldNote3D>().on_hold) continue;

            // else, return note
            return (note, note.GetComponent<HoldNote3D>().GetDistance(t, k));
        }

        return (null, float.MaxValue);
    }
    void HandleKeyDown(KeyCode keyId) {
        
        // Find the closest note in lane
        (GameObject hit_note, float hit_note_distance) = GetClosestNote(time_current, keyId);

        // check if the hit_note is valid
        if (hit_note != null)
        {
            if (hit_note_distance <= JUDGEMENT_BAD_WINDOW)
            {
                
                ScoreNote(hit_note, hit_note_distance);
                // If hold note: don't destroy, else destory
                if (!(hit_note.GetComponent<Note3D>() is HoldNote3D))
                {
                    DestroyNote(hit_note);
                } else
                {
                    HoldNote3D hold_note = hit_note.GetComponent<HoldNote3D>();
                    hold_note.on_hold = true;
                }
            }
        }

        //update visual disk 
        keyToObjMap[keyId].GetComponent<NoteTrigger>().Disk.GetComponent<VisualDisk>().SwitchToPressed();

    }
    void HandleKeyUp(KeyCode keyId) {

        (GameObject hit_note, float hit_note_distance) = GetActiveHoldNote(time_current, keyId);

        if (hit_note != null)
        {
            
            ScoreNote(hit_note, hit_note_distance);
            hit_note.GetComponent<Note3D>().Trigger();
            DestroyNote(hit_note);

        }

        keyToObjMap[keyId].GetComponent<NoteTrigger>().Disk.GetComponent<VisualDisk>().SwitchToUnpressed();


    }

    void ScoreSwapNote(GameObject note) {
        
        swapToBulletFlag = true;

    }

    void ScoreNote(GameObject note, float accuracy) {

        scoreTypeText.color = Color.white;

        if (accuracy <= JUDGEMENT_PERFECT_WINDOW) {
            current_combo += 1;
            max_combo = Mathf.Max(max_combo, current_combo);
            score += SCORE_PERFECT;
            perfect_count += 1;

            // =Spawn VFX on Hit
            if (hitVFXPrefab != null)
            {
                Vector3 hitPos = note.transform.position;
                GameObject vfx = Instantiate(hitVFXPrefab, hitPos, Quaternion.identity);
                Destroy(vfx, 1f); 
            }
            scoreTypeText.text = "PERFECT!";
            StartCoroutine(ScoreTypeTextAnimation());
        }
        else if (accuracy <= JUDGEMENT_GREAT_WINDOW) {
            current_combo += 1;
            max_combo = Mathf.Max(max_combo, current_combo);
            score += SCORE_GREAT;
            great_count += 1;

            // =Spawn VFX on Hit
            if (hitVFXPrefab != null)
            {
                Vector3 hitPos = note.transform.position;
                GameObject vfx = Instantiate(hitVFXPrefab, hitPos, Quaternion.identity);
                Destroy(vfx, 1f); // 
            }
            scoreTypeText.text = "GREAT!";
            StartCoroutine(ScoreTypeTextAnimation());

        }
        else if (accuracy <= JUDGEMENT_BAD_WINDOW) {
            current_combo = 0;
            score += SCORE_BAD;
            bad_count += 1;

            // =Spawn VFX on Hit
            if (hitVFXPrefab != null)
            {
                Vector3 hitPos = note.transform.position;
                GameObject vfx = Instantiate(hitVFXPrefab, hitPos, Quaternion.identity);
                Destroy(vfx, 1f); // 
            }
            scoreTypeText.text = "BAD";
            scoreTypeText.color = Color.red;
            StartCoroutine(ScoreTypeTextAnimation());

        }
        else
        {
            scoreTypeText.text = "MISS";
            scoreTypeText.color = Color.red; 
            StartCoroutine(ScoreTypeTextAnimation());

            current_combo = 0;
            miss_count += 1;
        }

        // if is swap note: trigger swap
        if (note.GetComponent<Note3D>() is SwapNote3D) {
            ScoreSwapNote(note);
        }
        
    }




    KeyCode LaneIDToKey(int laneID)
    {
        switch (laneID)
        {
            case 1:
                return KeyCode.W;
            case 2:
                return KeyCode.E;
            case 3:
                return KeyCode.I;
            case 4:
                return KeyCode.O;
            default:
                return KeyCode.Space;
        }
    }
    void HandleNotes() {

        // Setup
        List<GameObject> ToRemoveNotes = new List<GameObject>();

        if (currNote == noteInfos.Count)
        {
            Debug.Log("Finished Song");
            currNote++;
        }

        // Spawn Notes
        time_current += Time.deltaTime;
        if (randomSpawnMode) {

            // Random Spawn Mode
            while (time_current >= (time_nextnote - note_prespawn_time) ) {

                time_nextnote += 60.0f/bpm;

                int note_lane = UnityEngine.Random.Range(1, 25);

                if (note_lane <= 4 && note_lane >= 1) {

                    SpawnNote(LaneIDToKey(note_lane), time_nextnote, NoteType.TAP);

                } else if (note_lane <= 15 && note_lane >= 12) {

                    int hold_lane = note_lane - 11;
                    SpawnNote(LaneIDToKey(hold_lane), time_nextnote, NoteType.HOLD);
                    time_nextnote += 60.0f/bpm;

                }  else if (note_lane == 16 || note_lane == 17) {

                    int note_lane1 = UnityEngine.Random.Range(1, 5);
                    int note_lane1_limiter = note_lane1;
                    if (note_lane1_limiter == 3 || note_lane1_limiter == 1) note_lane1_limiter += 1;
                    int note_lane2 = UnityEngine.Random.Range(1, 3);
                    note_lane2 = (note_lane1_limiter + note_lane2 - 1) % 4 + 1;

                    SpawnNote(LaneIDToKey(note_lane1), time_nextnote, NoteType.HOLD);
                    time_nextnote += 60.0f/bpm;
                    SpawnNote(LaneIDToKey(note_lane2), time_nextnote, NoteType.TAP);

                } else if (note_lane == 23) {

                    int note_lane1 = UnityEngine.Random.Range(1, 5);
                    int note_lane1_limiter = note_lane1;
                    if (note_lane1_limiter == 3 || note_lane1_limiter == 1) note_lane1_limiter += 1;
                    int note_lane2 = UnityEngine.Random.Range(1, 3);
                    note_lane2 = (note_lane1_limiter + note_lane2 - 1) % 4 + 1;
                    int note_lane3 = UnityEngine.Random.Range(1, 3);
                    note_lane3 = (note_lane1_limiter + note_lane3 - 1) % 4 + 1;

                    SpawnNote(LaneIDToKey(note_lane1), time_nextnote, NoteType.HOLD);
                    SpawnNote(LaneIDToKey(note_lane2), time_nextnote + 30.0f/bpm, NoteType.TAP);
                    SpawnNote(LaneIDToKey(note_lane3), time_nextnote + 60.0f/bpm, NoteType.TAP);
                    time_nextnote += 60.0f/bpm;

                } else if (note_lane == 5 || note_lane == 8 || note_lane == 10 || note_lane == 21 || note_lane == 24) {

                    int note_lane1 = UnityEngine.Random.Range(1, 5);
                    int note_lane2 = UnityEngine.Random.Range(1, 4);
                    note_lane2 = (note_lane1 + note_lane2 - 1) % 4 + 1;

                    SpawnNote(LaneIDToKey(note_lane1), time_nextnote, NoteType.TAP);
                    SpawnNote(LaneIDToKey(note_lane2), time_nextnote, NoteType.TAP);

                } else if (note_lane == 6 || note_lane == 9 || note_lane == 11 || note_lane == 22) {
                    int note_lane1 = UnityEngine.Random.Range(1, 5);
                    int note_lane2 = UnityEngine.Random.Range(1, 5);

                    SpawnNote(LaneIDToKey(note_lane1), time_nextnote, NoteType.TAP);
                    SpawnNote(LaneIDToKey(note_lane2), time_nextnote + 30.0f/bpm, NoteType.TAP);
                } else if (note_lane == 7) {
                    SpawnNote(LaneIDToKey(0), time_nextnote, NoteType.TAP);
                } else if (note_lane == 18) {
                    SpawnNote(LaneIDToKey(0), time_nextnote, NoteType.HOLD);
                    time_nextnote += 60.0f/bpm;
                } else if (note_lane == 19) {

                    if (spawnSwapNotes) {
                        SpawnNote(LaneIDToKey(0), time_nextnote, NoteType.SWAP);
                        time_nextnote += 480.0f/bpm;
                    } else
                    {
                        int note_lane1 = UnityEngine.Random.Range(1, 5);
                        int note_lane2 = UnityEngine.Random.Range(1, 4);
                        note_lane2 = (note_lane1 + note_lane2 - 1) % 4 + 1;

                        SpawnNote(LaneIDToKey(note_lane1), time_nextnote, NoteType.HOLD);
                        SpawnNote(LaneIDToKey(note_lane2), time_nextnote, NoteType.HOLD);
                        time_nextnote += 60.0f/bpm;
                    }

                }

            }

        } else {

            // Load Beatmap Mode
            while (currNote < noteInfos.Count && time_current >= (noteInfos[currNote].start_time - note_prespawn_time) ) {
            
                switch (noteInfos[currNote].note_type)
                {
                    case Beatmap.NoteInfo.BASIC_NOTE:
                        //Debug.Log("BASIC_NOTE_PLAYED");
                        SpawnNote(LaneIDToKey(int.Parse(noteInfos[currNote].extra_info[0])), noteInfos[currNote].start_time, NoteType.TAP);
                        break;
                    case Beatmap.NoteInfo.HOLD_NOTE:
                        //Debug.Log("HOLD_NOTE_PLAYED");
                        break;
                    case Beatmap.NoteInfo.SPACE_NOTE:
                        //Debug.Log("SPACE_NOTE_PLAYED");
                        SpawnNote(KeyCode.Space, noteInfos[currNote].start_time, NoteType.TAP);
                        break;
                    case Beatmap.NoteInfo.SLIDE_NOTE:
                        //Debug.Log("SLIDE_NOTE_PLAYED");
                        break;
                }
                currNote++;
            }
        }

        //Update Notes
        foreach (GameObject note in ActiveNotes) {
            
            note.GetComponent<Note3D>().UpdatePosition(time_current, note_speed);

            // handle miss & despawn feature
            if (note.GetComponent<Note3D>().GetDespawnTime() + JUDGEMENT_BAD_WINDOW < time_current) {
                if (!note.GetComponent<Note3D>().triggered) {
                    ScoreNote(note, 100.0f);
                    note.GetComponent<Note3D>().Trigger();
                }
                if (note.transform.position.z < note_z_despawn) {
                    ToRemoveNotes.Add(note);
                }
            }

        }

        // Cleanup 
        foreach (GameObject note in ToRemoveNotes) {

            DestroyNote(note);

        }

    }

    System.Collections.IEnumerator ComboAnimation()
    {
        float timer = 0f;
        float time_to_finish = 0.2f;
        float start_size = comboText.fontSize;
        Color start_color = comboText.color;
        while (timer < time_to_finish)
        {
            timer += Time.deltaTime;
            // Lerp between the initial scale and the target scale
            comboText.fontSize = Mathf.Lerp(start_size, 72, timer / time_to_finish);
            comboText.color = Color.Lerp(start_color, combo_inc_color, timer / time_to_finish);
            yield return null; // Wait for the next frame
        }

        StartCoroutine(ComboAnimationDec());
    }

    System.Collections.IEnumerator ComboAnimationDec()
    {
        float timer = 0f;
        float time_to_finish = 0.2f;
        while (timer < time_to_finish)
        {
            timer += Time.deltaTime;
            // Lerp between the initial scale and the target scale
            comboText.fontSize = Mathf.Lerp(72, 64, timer / time_to_finish);
            comboText.color = Color.Lerp(combo_inc_color, combo_init_color, timer / time_to_finish);
            yield return null; // Wait for the next frame
        }
        comboText.fontSize = 64; // Ensure the final scale is exactly the target scale
        comboText.color = combo_init_color;
    }

    System.Collections.IEnumerator ScoreTypeTextAnimation()
    {
        float timer = 0f;
        float time_to_finish = 0.3f;
        while (timer < time_to_finish)
        {
            timer += Time.deltaTime;
            // Lerp between the initial scale and the target scale
            scoreTypeText.fontSize = Mathf.Lerp(36, 48, timer / time_to_finish);
            yield return null; // Wait for the next frame
        }
        scoreTypeText.fontSize = 36; // Ensure the final scale is exactly the target scale
        scoreTypeText.text = "";
    }

    // Update is called once per frame
    void UpdateRhythm()
    {

        // Handle Notes
        HandleNotes();
        // Handle Player Input
        HandleInput();
        
        
        if (time_current > 0.0f && !GetComponent<AudioSource>().isPlaying)
        {
            GetComponent<AudioSource>().Play();
            time_current = time_offset;
        }

        scoreText.text = "SCORE: " + score;
        
        if (int.Parse(comboText.text) < current_combo)
        {
            StartCoroutine(ComboAnimation());
        }
        comboText.text = current_combo + "";

        if (current_combo > 49)
        {
            comboVfx.SetActive(true);
        } else
        {
            comboVfx.SetActive(false);
        }

        if (swapToBulletFlag)
        {
            swapToBulletHell();
            swapToBulletFlag = false;
        }

    }

    // ============================================== //
    // =========== Bullet Hell Section ============== //
    // ============================================== //

    void swapToBulletHell()
    {
        // Swap: 
        gamestate = GameState.RHYTHM;
        swapGameState();

        // player
        player.GetComponent<Rigidbody>().WakeUp();

        // Todo... bullet hell setup..
        gameCamera.GetComponent<GameCamera>().TransitionToBulletHell();

        masterProjectilePool = new ObjectPool<GameObject>(
            CreatePooledBullet,
            OnGetBullet,
            OnReleaseBullet,
            OnDestroyBullet,
            collectionCheck: false,
            defaultCapacity: defaultPoolSize,
            maxSize: maxPoolSize
        );
        bulletHellStartTime = Time.time; //TODO: i am just setting this to the time the bullet hell manager is activated

    }
    void ClearBulletHell()
    {
        foreach(BulletEmitter ae in GetComponentsInChildren<BulletEmitter>())
        {
            ae.gameObject.SetActive(false);
        }
    }
    
    void UpdateBullet()
    {
        //emitters will be set active on a certain beat 
        //ex: 240 bpm, 3 min song = 720 beats and each emitter is released on one of those beats
        //will continuously set projectile emitters to active
        if (emitterIndex >= emitters.Count)
        {
            OnBulletHellComplete();

        } else {
            
            Pair p = emitters[emitterIndex];

            if (p.emitter == null)
            {
                p.emitter = Instantiate(PlaceholderDefaultEmitter);
                p.emitter.SetActive(false);
                timeLastEmitterSetActive = Time.time;
            }

            if (Time.time >= Math.Max(timeLastEmitterSetActive, bulletHellStartTime) + p.beatsAfterPrevToActivate * timePerBeat)
            {
                p.emitter.SetActive(true);
                Debug.Log("set active: " + emitterIndex);
                emitterIndex++;
                timeLastEmitterSetActive = Time.time;
                if (emitterIndex == emitters.Count)
                {
                    OnBulletHellComplete();
                }
            }
        }

        if (swapToRhythmFlag)
        {
            swapToRhythmGame();
            swapToRhythmFlag = false;
        }
    }

    void OnBulletHellComplete()
    {
        emitterIndex = 0;
        swapToRhythmFlag = true;
        //gameObject.SetActive(false);
        //TODO something (placeholder^^)
    }

    public GameObject CreatePooledBullet()
    {
        GameObject proj = Instantiate(bulletPrefab);
        proj.SetActive(false);
        return proj;
    }

    public void OnGetBullet(GameObject bullet) {
        bullet.SetActive(true);
    }
    public void OnReleaseBullet(GameObject bullet) {
        bullet.SetActive(false);
    }
    public void OnDestroyBullet(GameObject bullet) {
        Destroy(bullet);
    }

    void Update()
    {
        if (gamestate == GameState.RHYTHM)
        {
            UpdateRhythm();
        }
        else
        {
            UpdateBullet();
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //initialize bullet hell variables
        timeLastEmitterSetActive = Time.time;
        emitterIndex = 0;

        // Rhythm game setup
        time_offset = 60f / (bpm * 2f) * 8f;
        instance = this;
        note_prespawn_time = (note_z_spawn - note_z_despawn) / note_speed;
        noteInfos = Beatmap.LoadBeatmap("Beatmap");
        currNote = 0;

        string combo_inc_hex_color = "#B2ACFF";
        ColorUtility.TryParseHtmlString(combo_inc_hex_color, out combo_inc_color);

        string combo_init_hex_color = "rgba(255, 255, 255, 1)";
        ColorUtility.TryParseHtmlString(combo_init_hex_color, out combo_init_color);

        swapToRhythmGame();

    }
}