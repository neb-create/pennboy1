using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    private Vector2 dir;
    [SerializeField] float speed = 5f;

    [Header("Dash Attributes")]
    [SerializeField] float dashSpeed = 30f;
    [SerializeField] float dashDuration = 0.2f; //in seconds
    [SerializeField] float dashCooldown = 1f;
    private float timeWhenDashStart;
    private bool isDashing = false;
    public bool IsDashing()
    {
        return isDashing;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        timeWhenDashStart = Time.time - 1f;
    }

    // Update is called once per frame
    void Update()
    {
        dir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        if (!isDashing)
        {
            bool canDash = (Time.time - timeWhenDashStart) >= dashCooldown;
            if (canDash && (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.Space)))
                Dash();
        }
        else
        {
            if (timeWhenDashStart + dashDuration <= Time.time)
            {
                isDashing = false;
            }
        }
    }
    void FixedUpdate()
    {
        if(!isDashing) rb.linearVelocity = dir * speed;
    }
    void Dash()
    {
        /*
        dash mechanics:
        -you can't move while dashing
        -you are invincible while dashing i frames yay <- TODO NOT IMPLEMENTED YET!!
        */
        isDashing = true;
        timeWhenDashStart = Time.time;
        rb.linearVelocity = dir * dashSpeed; //TODO: it should drop off a little instead of just reverting to 5
    }
}
