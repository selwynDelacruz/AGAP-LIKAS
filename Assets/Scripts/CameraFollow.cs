using UnityEngine;

public class BoatCameraFollow : MonoBehaviour
{
    public Transform boat; // Assign the boat object here
    public Vector3 offset = new Vector3(0, 5, -10); // Adjust height & distance
    public float smoothSpeed = 5f;

    public float mouseSensitivity = 2f;
    public float minYAngle = -20f; // Minimum up/down angle
    public float maxYAngle = 60f; // Maximum up/down angle

    private float currentYaw = 0f;
    private float currentPitch = 20f; // Up/down angle

    void LateUpdate()
    {
        if (boat == null) return;

        // Mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        currentYaw += mouseX; // Left/right angle
        currentPitch -= mouseY; // Up/down angle
        currentPitch = Mathf.Clamp(currentPitch, minYAngle, maxYAngle);

        // Calculate rotation
        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0);

        // Apply offset with rotation around the boat
        Vector3 desiredPosition = boat.position + rotation * offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // Always look at the boat
        transform.LookAt(boat.position + Vector3.up * 2f); // Slightly above boat center
    }

    void Update()
    {
        // Always lock/hide cursor for local camera control
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}

