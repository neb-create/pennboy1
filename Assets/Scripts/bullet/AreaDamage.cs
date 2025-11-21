using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEngine;

public class AreaDamage : MonoBehaviour
{
    /*[Header("Preset (leave blank if you're hardcoding the objects)")]
    [SerializeField] AreaDamageAttributePreset preset;*/
    //commented out bc i presume we will be hardcoding attributes like size

    [Header("General Attributes")]
    [SerializeField] private float damage;
    [SerializeField] bool lifetimeInBeats = true;
    private float lifetimeBeats = 0;
    [SerializeField] private float lifetime;
    private float elapsedTime;
    private float damageCooldown = 1f;
    private float lastDamageInstanceTime = -1;
    private bool canDealDamage = false;
    [SerializeField] float beatsBeforeCanDoDamage = 3f; //just assume lifetime beats is true
    private float timeBeforeCanDealDamage;
    private Collider col;
    private MeshRenderer mr;

    void OnEnable()
    {
        elapsedTime = 0;
        canDealDamage = false;
        col = GetComponent<Collider>();
        mr = GetComponent<MeshRenderer>();

        col.enabled = false;
        mr.enabled = false;

        timeBeforeCanDealDamage = beatsBeforeCanDoDamage * (60.0f / GameManager.instance.bpm);

        
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        
    }
    void Update()
    {
        elapsedTime += Time.deltaTime;
        if (!canDealDamage)
        {
            if (elapsedTime >= timeBeforeCanDealDamage)
            {
                canDealDamage = true;
                col.enabled = true;
                mr.enabled = true;
            }
        } else {
            if (lifetimeBeats == 0) lifetimeBeats = lifetime * (60.0f / GameManager.instance.bpm);
        
            float effectiveLifetime = lifetime;
            if (lifetimeInBeats) effectiveLifetime = lifetimeBeats;
            if (elapsedTime - timeBeforeCanDealDamage > effectiveLifetime)
            {
                gameObject.SetActive(false);
                elapsedTime = 0;
                canDealDamage = false;
            }
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
