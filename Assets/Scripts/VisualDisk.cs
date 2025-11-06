using UnityEngine;

public class VisualDisk : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform;
    public GameObject pressVFXPrefab; // prefab to spawn when pressed

    [Header("Animation Settings")]
    float scaleAmount = 0.1f;
    float pulseSpeed = 2.0f;
    float colorChangeSpeed = 10f;
    float moveRadius = 0.12f;
    float moveSpeed = 0.8f;
    float pressed_scale = 0.94f;

    [Header("Colors")]
    public Color normalColor = new Color(0.82f, 0.88f, 1.0f);
    public Color pressedColor = new Color(0.55f, 0.65f, 0.9f);

    private Renderer rend;
    private Vector3 basePos;
    private Color currentColor;
    private bool isPressed = false;
    private float pressBlend = 0f;

    void Start()
    {
        if (Camera.main != null)
            cameraTransform = Camera.main.transform;

        rend = GetComponent<Renderer>();
        basePos = transform.position;
        currentColor = normalColor;
    }

    void Update()
    {
        // Smooth blend value (0 = unpressed, 1 = pressed)
        float targetBlend = isPressed ? 1f : 0f;
        pressBlend = Mathf.Lerp(pressBlend, targetBlend, Time.deltaTime * colorChangeSpeed);

        // --- Color ---
        Color targetColor = Color.Lerp(normalColor, pressedColor, pressBlend);
        rend.material.color = targetColor;

        // --- Scale + motion ---
        float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
        Vector3 unpressedScale = Vector3.one * (1f + pulse * scaleAmount);
        Vector3 pressedScale = Vector3.one * pressed_scale;
        transform.localScale = Vector3.Lerp(unpressedScale, pressedScale, pressBlend);

        // --- Position ---
        Vector3 offset = Vector3.zero;
        if (moveRadius > 0f)
        {
            offset = new Vector3(
                Mathf.Sin(Time.time * moveSpeed + basePos.x),
                Mathf.Cos(Time.time * moveSpeed + basePos.y),
                0f
            ) * moveRadius;
        }

        Vector3 unpressedPos = basePos + offset;
        Vector3 pressedPos = basePos;
        transform.position = Vector3.Lerp(unpressedPos, pressedPos, pressBlend);
    }


    void LateUpdate()
    {
        if (cameraTransform != null)
            transform.LookAt(transform.position + cameraTransform.forward);
    }

    // --- STATE FUNCTIONS ---

    public void SwitchToPressed()
    {
        if (isPressed) return;
        isPressed = true;

        // Stop movement
        transform.position = basePos;

        // Spawn VFX
        if (pressVFXPrefab != null) {
            GameObject vfx = Instantiate(pressVFXPrefab, transform.position, Quaternion.identity);
            Destroy(vfx, 1f); // 
        }
    }

    public void SwitchToUnpressed()
    {
        if (!isPressed) return;
        isPressed = false;

        // Restore scale motion naturally via Update()
    }
}
