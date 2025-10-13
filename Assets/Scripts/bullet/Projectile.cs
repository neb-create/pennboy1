using Unity.VisualScripting;
using UnityEditor.Presets;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Scripting.APIUpdating;

public class Projectile : MonoBehaviour
{
    [Header("Preset (USE THIS!!")]
    [SerializeField] BulletAttributePreset preset;
    [Header("Homing Attributes")]
    private bool homing;
    private bool lockedOn = false;
    private float delayBeforeLockOn = 0.5f;
    private float homingRotateSpeedRads;
    private float lockedOnSpeedMultiplier = 2f;
    private float lockedOnSpeed;
    private Vector3 targetDir;

    [Header("General Attributes")]
    [SerializeField] private float damage;
    private Vector2 dir;
    private float speed;
    private Rigidbody rb;
    private Renderer rend;
    private float elapsedTime;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
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
        }
    }
    public void InitAttributes(BulletAttributePreset preset) //will be called by object pooling thing
    {
        if (preset != null)
        {
            homing = preset.homing;
            delayBeforeLockOn = preset.delayBeforeLockOn;
            homingRotateSpeedRads = preset.homingRotateSpeedRads;
            lockedOnSpeedMultiplier = preset.lockedOnSpeedMultiplier;
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
    }
    void Update()
    {
        if (!rend.isVisible)
        {
            BulletHellManager.instance.masterProjectilePool.Release(this.gameObject);
        }
        elapsedTime += Time.deltaTime;
    }

    void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController pc = collision.gameObject.GetComponent<PlayerController>();
            if (pc.IsDashing()) return;
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
