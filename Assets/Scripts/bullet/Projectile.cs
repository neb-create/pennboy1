using Unity.VisualScripting;
using UnityEditor.Presets;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Scripting.APIUpdating;

public class Projectile : MonoBehaviour
{
    [Header("Preset (USE THIS!! VARIABLES BELOW EXPOSED FOR DEBUGGING)")]
    [SerializeField] BulletAttributePreset preset;
    [Header("Homing Attributes")]
    [SerializeField] bool homing;
    private bool lockedOn = false;
    [SerializeField] float delayBeforeLockOn = 0.5f;
    [SerializeField] float homingRotateSpeedRads;
    [SerializeField] float lockedOnSpeedMultiplier = 2f;
    private float lockedOnSpeed;
    private Vector3 targetDir;

    [Header("General Attributes")]
    [SerializeField] private float damage;
    private Vector2 dir;
    private float speed;
    private Rigidbody2D rb;
    private Renderer rend;
    private float elapsedTime;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rend = GetComponent<Renderer>();
    }
   public void InitAttributes() //will be called by object pooling thing
    {
        if (preset != null)
        {
            homing = preset.homing;
            delayBeforeLockOn = preset.delayBeforeLockOn;
            homingRotateSpeedRads = preset.homingRotateSpeedRads;
            lockedOnSpeedMultiplier = preset.lockedOnSpeedMultiplier;
            damage = preset.damage;
            //TODO you may want a way to change the damage but keep everything else the same?
        }
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        rb.linearVelocity = dir * speed;

        if (homing && elapsedTime > delayBeforeLockOn)
        {
            if (!lockedOn)
            {
                Vector3 playerPos = BulletHellManager.instance.player.transform.position;
                targetDir = (playerPos - this.transform.position).normalized;
                speed = lockedOnSpeed;
                lockedOn = true;
            }
            else
            { 
                Vector3 newDir = Vector3.RotateTowards(dir, targetDir, homingRotateSpeedRads, 0.01f);
                dir = (Vector2)newDir;
            }
        }

        elapsedTime += Time.deltaTime;
    }
    void Update()
    {
        if (!rend.isVisible)
        {
            BulletHellManager.instance.masterProjectilePool.Release(this.gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Health h = collision.gameObject.GetComponent<Health>();
            h.TakeDamage(damage);
            PlayHitVFX();
            Destroy(this.gameObject);
        }
    }
    void PlayHitVFX()
    {
        //make sure this impl doesn't depend on the projectile existing
    }
    public void SetVelocity(Vector2 v)
    {
        dir = v.normalized;
        speed = v.magnitude;
        lockedOnSpeed = speed * lockedOnSpeedMultiplier;
    }
}
