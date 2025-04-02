using UnityEngine;

public class EditorCamera : MonoBehaviour
{
    [SerializeField] float lookSpeed = 2f;
    [SerializeField] float moveSpeed = 500f;
    [SerializeField] float zoomSpeed = 500f;

    private Vector3 pivotPoint;

    void Update()
    {
        HandleMouseLook();
        HandlePanning();
        HandleZoom();
    }

    void HandleMouseLook()
    {
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
            float mouseY = Input.GetAxis("Mouse Y") * lookSpeed;
            transform.Rotate(Vector3.up, mouseX, Space.World);
            transform.Rotate(Vector3.right, -mouseY, Space.Self);
        }
    }

    void HandlePanning()
    {
        if (Input.GetMouseButton(2))
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