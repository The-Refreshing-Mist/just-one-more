using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float mouseSensitivity = 0.15f;

    [SerializeField] private float maxLookUp = 50f;
    [SerializeField] private float maxLookDown = -35f;
    [SerializeField] private float maxLookLeft = -70f;
    [SerializeField] private float maxLookRight = 70f;

    private float pitch = 0f;
    private float yaw = 0f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        float mouseX = mouseDelta.x * mouseSensitivity;
        float mouseY = mouseDelta.y * mouseSensitivity;

        yaw += mouseX;
        pitch -= mouseY;

        pitch = Mathf.Clamp(pitch, maxLookDown, maxLookUp);
        yaw = Mathf.Clamp(yaw, maxLookLeft, maxLookRight);

        transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
        cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}