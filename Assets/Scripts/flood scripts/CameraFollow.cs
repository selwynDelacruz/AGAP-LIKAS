using UnityEngine;
using Photon.Pun; // Add this

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

    private PhotonView boatPhotonView; // Add this

    void LateUpdate()
    {
        if (boat == null) return;

        // Only control camera if this is the local player's boat
        if (boatPhotonView != null && !boatPhotonView.IsMine)
            return;

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

    void OnEnable()
    {
        // Try to get the PhotonView from the boat
        if (boat != null)
        {
            boatPhotonView = boat.GetComponent<PhotonView>();
        }
    }

    void Update()
    {
        // In case the boat is assigned after Start/OnEnable
        if (boatPhotonView == null && boat != null)
        {
            boatPhotonView = boat.GetComponent<PhotonView>();
        }

        // Only lock/hide cursor for the local player controlling their own boat
        if (boatPhotonView != null && boatPhotonView.IsMine)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
