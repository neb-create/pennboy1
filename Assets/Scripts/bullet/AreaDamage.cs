using Unity.VisualScripting;
using UnityEngine;

public class AreaDamage : MonoBehaviour
{
    /*[Header("Preset (leave blank if you're hardcoding the objects)")]
    [SerializeField] AreaDamageAttributePreset preset;*/
    //commented out bc i presume we will be hardcoding attributes like size

    [Header("General Attributes")]
    [SerializeField] private float damage;
    [SerializeField] bool lifetimeInBeats;
    private float lifetimeBeats;
    [SerializeField] private float lifetime;
    private float elapsedTime;
    private float damageCooldown = 1f;
    private float lastDamageInstanceTime = -1;
    void Start()
    {
        lifetimeBeats = lifetime * (60f / GameManager.instance.bpm);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
    }
    void Update()
    {
        elapsedTime += Time.deltaTime;
        float effectiveLifetime = lifetime;
        if (lifetimeInBeats) effectiveLifetime = lifetimeBeats;
        if (elapsedTime > effectiveLifetime)
        {
            gameObject.SetActive(false);
            elapsedTime = 0;
        }
    }

    void OnTriggerStay(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player") && lastDamageInstanceTime + damageCooldown <= Time.time)
        {
            PlayerController pc = collision.gameObject.GetComponent<PlayerController>();
            if (pc.IsDashing()) return;
            Health h = collision.gameObject.GetComponent<Health>();
            h.TakeDamage(damage);
            PlayHitVFX();
            lastDamageInstanceTime = Time.time;
        }
    }
    void PlayHitVFX()
    {
        //make sure this impl doesn't depend on the projectile existing
    }
}
