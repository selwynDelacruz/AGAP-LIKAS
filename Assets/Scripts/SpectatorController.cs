using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// Spectator controller for instructor userType - follows active player with mouse camera rotation
/// </summary>
public class SpectatorController : MonoBehaviour
{
    [Header("Spectator Settings")]
    [Tooltip("Mouse sensitivity for camera rotation")]
    [SerializeField] private float mouseSensitivity = 2f;

    [Tooltip("Smooth camera rotation")]
    [SerializeField] private float rotationSmoothness = 10f;

    [Tooltip("Distance from player")]
    [SerializeField] private float followDistance = 5f;

    [Tooltip("Height offset from player")]
    [SerializeField] private float heightOffset = 2f;

    [Header("Player Tracking")]
    [Tooltip("Tag to find the active player")]
    [SerializeField] private string playerTag = "Player";

    [Header("Spectator Camera")]
    [Tooltip("The spectator camera transform")]
    [SerializeField] private Transform spectatorCamera;

    [Tooltip("Cinemachine virtual camera for spectator mode")]
    [SerializeField] private CinemachineCamera spectatorVirtualCamera;

    private Transform activePlayer;
    private float pitch = 20f; // Default pitch angle
    private float yaw = 0f;
    private bool isSpectatorMode = false;
    private bool isRotating = false;

    void Start()
    {
        // Check if user is instructor
        string userType = PlayerPrefs.GetString("Type_Of_User", "");
        isSpectatorMode = (userType == "instructor");

        if (isSpectatorMode)
        {
            Debug.Log("[SpectatorController] Instructor mode detected - Spectator controls enabled");
            EnableSpectatorMode();
        }
        else
        {
            Debug.Log("[SpectatorController] Trainee mode detected - Spectator controls disabled");
            this.enabled = false;
        }
    }

    private void EnableSpectatorMode()
    {
        // Find the active player in the scene
        FindActivePlayer();

        // Setup spectator camera if not assigned
        if (spectatorCamera == null)
        {
            // Check if there's already a main camera
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                spectatorCamera = mainCam.transform;
                Debug.Log("[SpectatorController] Using main camera for spectator mode");
            }
            else
            {
                // Create new camera
                GameObject camObj = new GameObject("SpectatorCamera");
                spectatorCamera = camObj.transform;
                Camera cam = camObj.AddComponent<Camera>();
                cam.tag = "MainCamera";
                Debug.Log("[SpectatorController] Created new spectator camera");
            }
        }

        // Always keep cursor visible and unlocked for instructor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Activate spectator virtual camera if assigned
        if (spectatorVirtualCamera != null)
        {
            spectatorVirtualCamera.Priority = 100; // Higher priority than player camera
        }

        Debug.Log("[SpectatorController] Spectator mode enabled - Right-click to rotate camera");
    }

    private void FindActivePlayer()
    {
        // Try to find player by tag
        GameObject playerObj = null;
        
        try
        {
            playerObj = GameObject.FindGameObjectWithTag(playerTag);
        }
        catch (UnityException)
        {
            // Tag doesn't exist, will try alternative method
            Debug.LogWarning($"[SpectatorController] Player tag '{playerTag}' not found");
        }
        
        if (playerObj != null)
        {
            activePlayer = playerObj.transform;
            Debug.Log($"[SpectatorController] Found active player: {playerObj.name}");
        }
        else
        {
            // Try alternative methods to find player
            // Look for ThirdPersonController
            StarterAssets.ThirdPersonController thirdPersonController = Object.FindAnyObjectByType<StarterAssets.ThirdPersonController>();
            if (thirdPersonController != null)
            {
                activePlayer = thirdPersonController.transform;
                Debug.Log($"[SpectatorController] Found player via ThirdPersonController: {activePlayer.name}");
            }
            else
            {
                Debug.LogWarning("[SpectatorController] Could not find active player! Spectator camera will not follow.");
            }
        }
    }

    void Update()
    {
        if (!isSpectatorMode || spectatorCamera == null)
            return;

        // If player not found, try to find it
        if (activePlayer == null)
        {
            FindActivePlayer();
            return;
        }

        HandleCameraRotation();
        FollowPlayer();
        
        // Ensure cursor stays visible
        if (!Cursor.visible)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void HandleCameraRotation()
    {
        // Only rotate when right mouse button is held
        if (Input.GetMouseButton(1)) // Right mouse button
        {
            if (!isRotating)
            {
                isRotating = true;
            }

            // Get mouse input
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            // Update rotation
            yaw += mouseX;
            pitch -= mouseY;

            // Clamp pitch to prevent over-rotation
            pitch = Mathf.Clamp(pitch, -89f, 89f);
        }
        else
        {
            isRotating = false;
        }
    }

    private void FollowPlayer()
    {
        if (activePlayer == null)
            return;

        // Calculate target position based on player position and camera rotation
        Vector3 direction = Quaternion.Euler(pitch, yaw, 0f) * Vector3.back;
        Vector3 targetPosition = activePlayer.position + Vector3.up * heightOffset + direction * followDistance;

        // Apply smooth movement
        spectatorCamera.position = Vector3.Lerp(spectatorCamera.position, targetPosition, Time.deltaTime * rotationSmoothness);

        // Always look at the player
        Vector3 lookAtPosition = activePlayer.position + Vector3.up * heightOffset;
        spectatorCamera.LookAt(lookAtPosition);
    }

    /// <summary>
    /// Public method to check if spectator mode is active
    /// </summary>
    public bool IsSpectatorMode()
    {
        return isSpectatorMode;
    }

    /// <summary>
    /// Public method to get spectator camera transform
    /// </summary>
    public Transform GetSpectatorCamera()
    {
        return spectatorCamera;
    }

    /// <summary>
    /// Public method to set the active player manually
    /// </summary>
    public void SetActivePlayer(Transform player)
    {
        activePlayer = player;
        Debug.Log($"[SpectatorController] Active player set to: {player.name}");
    }

    /// <summary>
    /// Public method to check if camera is being rotated
    /// </summary>
    public bool IsRotating()
    {
        return isRotating;
    }

    void OnGUI()
    {
        if (isSpectatorMode && activePlayer != null)
        {
            // Display spectator info
            GUI.Label(new Rect(10, 10, 300, 20), "SPECTATOR MODE - Following: " + activePlayer.name);
            GUI.Label(new Rect(10, 30, 300, 20), "Hold RIGHT-CLICK to rotate camera");
        }
    }
}
