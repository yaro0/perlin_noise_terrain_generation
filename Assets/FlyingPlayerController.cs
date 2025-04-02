using UnityEngine;

public class FlyingPlayerController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float lookSensitivity = 2f;

    private float yaw = 0f;
    private float pitch = 0f;

    void Start()
    {
        // Lock cursor to the center
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMovement();
        HandleRotation();
    }

    void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");  // A/D or Left/Right Arrow
        float moveZ = Input.GetAxis("Vertical");    // W/S or Up/Down Arrow
        float moveY = 0;

        if (Input.GetKey(KeyCode.Space)) moveY = 1;     // Ascend
        if (Input.GetKey(KeyCode.LeftShift)) moveY = -1; // Descend

        // Move in local space (relative to rotation)
        Vector3 moveDir = transform.right * moveX + transform.up * moveY + transform.forward * moveZ;
        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }

    void HandleRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -90f, 90f); // Prevent flipping over

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}