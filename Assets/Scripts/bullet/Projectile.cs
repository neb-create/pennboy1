using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class Projectile : MonoBehaviour
{
    [SerializeField] bool homing;
    [SerializeField] private float damage;
    private Vector2 dir;
    private float speed;
    private Rigidbody2D rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        rb.linearVelocity = dir * speed;
        //TODO implement homing
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
    }
}
