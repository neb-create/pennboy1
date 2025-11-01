using UnityEngine;

public class Note_VFXRingAnimation : MonoBehaviour
{
    [SerializeField] Gradient colorCycle;
    float baseRate;
    float timer;
    public float bpm = 120f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float beatInterval = 60f / bpm;
        timer += Time.deltaTime;
        if (timer >= beatInterval)
        {
            timer -= beatInterval;
            PulseScale();
        }

        float t_color = timer / beatInterval;


    }
    void PulseScale()
    {
        transform.localScale = Vector3.one * 1.3f;
        Invoke(nameof(ResetScale), 0.1f);
    }

    void ResetScale()
    {
        transform.localScale = Vector3.one;
    }
}
