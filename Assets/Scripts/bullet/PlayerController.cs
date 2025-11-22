using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("is one indexed not zero")]
    [SerializeField] CharacterDataSO[] fishTypes;
    [SerializeField] TextMeshPro expression;
    private Rigidbody rb;
    private Vector2 dir;
    [SerializeField] float speed = 10f;

    [Header("Dash Attributes")]
    [SerializeField] float dashSpeed = 30f;
    [SerializeField] float dashDuration = 0.1f; //in seconds
    [SerializeField] float dashCooldown = 0.5f;
    private float timeWhenDashStart;
    private bool isDashing = false;
    private Rect bounds;
    public bool IsDashing()
    {
        return isDashing;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        timeWhenDashStart = Time.time - 1f;

        if(PlayerPrefs.GetInt("Fish Type") != 0)
        {
            Debug.Log("changing");
            expression.text = fishTypes[PlayerPrefs.GetInt("Fish Type")].characterName;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Don't do anything if in rhythm game state
        if (GameManager.instance.gamestate == GameManager.GameState.RHYTHM) return;

        if(bounds == null)
        {
            bounds = calcBulletHellBounds(); //hopefully only need to calc once
        }

        // Else
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
        // Don't do anything if in rhythm game state
        if (GameManager.instance.gamestate == GameManager.GameState.RHYTHM) return;
        
        // Else
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
    Rect calcBulletHellBounds()
    {
        Camera cam = Camera.main;

        Ray bottomLeft = cam.ViewportPointToRay(new Vector3(0, 0, 0));
        float t = -bottomLeft.origin.z/bottomLeft.direction.z;
        Vector3 point = bottomLeft.origin + bottomLeft.direction*t;

        float xMin = point.x;
        float xMax = point.x;
        float yMin = point.y;
        float yMax = point.y;

        Ray topLeft = cam.ViewportPointToRay(new Vector3(0, 1, 0));
        t = -topLeft.origin.z/topLeft.direction.z;
        point = topLeft.origin + topLeft.direction*t;
        xMin = Mathf.Min(xMin, point.x);
        xMax = Mathf.Max(xMax, point.x);
        yMin = Mathf.Min(yMin, point.y);
        yMax = Mathf.Max(yMax, point.y);

        Ray topRight = cam.ViewportPointToRay(new Vector3(1, 1, 0));
        t = -topRight.origin.z/topRight.direction.z;
        point = topRight.origin + topRight.direction*t;
        xMin = Mathf.Min(xMin, point.x);
        xMax = Mathf.Max(xMax, point.x);
        yMin = Mathf.Min(yMin, point.y);
        yMax = Mathf.Max(yMax, point.y);

        Ray bottomRight = cam.ViewportPointToRay(new Vector3(1, 0, 0));
        t = -bottomRight.origin.z/bottomRight.direction.z;
        point = bottomRight.origin + bottomRight.direction*t;
        xMin = Mathf.Min(xMin, point.x);
        xMax = Mathf.Max(xMax, point.x);
        yMin = Mathf.Min(yMin, point.y);
        yMax = Mathf.Max(yMax, point.y);

        return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
    }
}
