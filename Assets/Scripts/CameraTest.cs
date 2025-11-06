using UnityEngine;

public class SimpleCameraArrowMove : MonoBehaviour
{
    public float sensitivity = 3f;
    public float moveSpeed = 5f;
    public float maxYAngle = 80f;
    private Vector2 currentRotation;

    void Update()
    {
        // Rotate with RMB
        if (Input.GetMouseButton(1))
        {
            currentRotation.x += Input.GetAxis("Mouse X") * sensitivity;
            currentRotation.y -= Input.GetAxis("Mouse Y") * sensitivity;
            currentRotation.y = Mathf.Clamp(currentRotation.y, -maxYAngle, maxYAngle);

            transform.rotation = Quaternion.Euler(currentRotation.y, currentRotation.x, 0f);
        }

        // Move with arrow keys only
        float moveX = 0f;
        float moveZ = 0f;

        if (Input.GetKey(KeyCode.UpArrow)) moveZ += 1f;
        if (Input.GetKey(KeyCode.DownArrow)) moveZ -= 1f;
        if (Input.GetKey(KeyCode.LeftArrow)) moveX -= 1f;
        if (Input.GetKey(KeyCode.RightArrow)) moveX += 1f;

        Vector3 move = new Vector3(moveX, 0f, moveZ).normalized;
        transform.Translate(move * moveSpeed * Time.deltaTime, Space.Self);
    }
}
