using UnityEngine;

public class EditorCamera : MonoBehaviour
{
    public float lookSpeed = 2f;
    public float moveSpeed = 10f;
    public float zoomSpeed = 10f;

    private Vector3 pivotPoint;

    void Update()
    {
        HandleMouseLook();
        HandlePanning();
        HandleZoom();
    }

    void HandleMouseLook()
    {
        if (Input.GetMouseButton(1)) // Right mouse button
        {
            float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
            float mouseY = Input.GetAxis("Mouse Y") * lookSpeed;

            transform.Rotate(Vector3.up, mouseX, Space.World);
            transform.Rotate(Vector3.right, -mouseY, Space.Self);
        }
    }

    void HandlePanning()
    {
        if (Input.GetMouseButton(2)) // Middle mouse button
        {
            float moveX = -Input.GetAxis("Mouse X") * moveSpeed * Time.deltaTime;
            float moveY = -Input.GetAxis("Mouse Y") * moveSpeed * Time.deltaTime;

            transform.position += transform.right * moveX;
            transform.position += transform.up * moveY;
        }
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            transform.position += transform.forward * scroll * zoomSpeed;
        }
    }
}