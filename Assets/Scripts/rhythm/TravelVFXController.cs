using UnityEngine;

public class TravelVFXController : MonoBehaviour
{
    ParticleSystem ps;
    float baseRate;
    float timer;
    public float bpm = 120f;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        baseRate = ps.emission.rateOverTime.constant;
    }

    void Update()
    {
        float beatInterval = 60f / bpm;
        timer += Time.deltaTime;
        if (timer >= beatInterval)
        {
            timer -= beatInterval;
            Pulse();
        }
    }

    void Pulse()
    {
        
        var emission = ps.emission;
        emission.rateOverTime = baseRate * 2f;

        transform.localScale = Vector3.one * 1.3f;
        Invoke(nameof(ResetVFX), 0.1f);
    }

    void ResetVFX()
    {
        var emission = ps.emission;
        emission.rateOverTime = baseRate;
        transform.localScale = Vector3.one;
    }
}
