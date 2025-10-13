using UnityEngine;

public class VisualDisk : MonoBehaviour
{
    private Transform cameraTransform;

    void Start()
    {
        // Get a reference to the main camera's transform
        cameraTransform = Camera.main.transform;
    }


    void Update()
    {
        float scaleAmount = 0.2f;
        float speed = 2.0f;
        float pulse = (Mathf.Sin(Time.time * speed) + 1f) / 2f; // Sinwave in range 0..1
        transform.localScale = new Vector3(1.0f, 1.0f, 1.0f) * (1f + pulse * scaleAmount);
    }

    void LateUpdate()
    {
        // Make the sprite always face the camera
        transform.LookAt(transform.position + cameraTransform.forward);
    }
}