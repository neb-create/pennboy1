using UnityEngine;

public class Note_VFXRingAnimation : MonoBehaviour
{
    [SerializeField] Gradient colorCycle;
    private SpriteRenderer sr;
    float baseRate;
    float timer;
    public float bpm = 120f;
    private Vector3 originalScale;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        float beatInterval = 60f / bpm;
        timer += Time.deltaTime;

        transform.localScale = originalScale * (4f * Mathf.Sin(beatInterval * timer / (2*Mathf.PI)) + 1f);

        timer %= beatInterval;

        float t_color = timer / beatInterval;
        sr.color = colorCycle.Evaluate(t_color);
    }
    void PulseScale()
    {
        transform.localScale = originalScale * 1.3f;
        Invoke(nameof(ResetScale), 0.1f);
    }

    void ResetScale()
    {
        transform.localScale = originalScale;
    }
}
