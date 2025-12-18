using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Cinemachine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Spectator controller for instructor users.
/// Allows camera orbit around active trainee players without controlling them.
/// Attach this to an empty GameObject prefab for the instructor.
/// </summary>
public class SpectatorController : MonoBehaviour
{
    [Header("Spectator Settings")]
    [Tooltip("Camera sensitivity for spectator mode")]
    public float cameraSensitivity = 2.0f;

    [Tooltip("Distance from the target player")]
    public float spectatorDistance = 5.0f;

    [Tooltip("Height offset above the target player")]
    public float spectatorHeight = 2.0f;

    [Header("Camera Clamps")]
    [Tooltip("How far in degrees can you move the camera up")]
    public float topClamp = 70.0f;

    [Tooltip("How far in degrees can you move the camera down")]
    public float bottomClamp = -30.0f;

    [Header("Player Switching")]
    [Tooltip("Key to switch to next trainee (default: Space)")]
    public KeyCode switchPlayerKey = KeyCode.Space;

    [Header("References")]
    [Tooltip("Reference to the main camera in the scene")]
    public Camera mainCamera;

    [Tooltip("Optional: Reference to Cinemachine Virtual Camera")]
    public CinemachineCamera virtualCamera;

    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;
    private bool _isInstructor = false;
    private bool _isRightClickHeld = false;
    private bool _lastSpacebarState = false;
    private bool _hasInitialized = false;
    private float _initializationTimer = 0f;
    private const float INITIALIZATION_DELAY = 0.5f;

    // List of all trainee players
    private List<GameObject> _traineePlayers = new List<GameObject>();
    private int _currentPlayerIndex = 0;
    private GameObject _currentTargetPlayer = null;

    // Camera target object that follows the selected trainee
    private GameObject _spectatorCameraTarget;

    private void Start()
    {
        // Check if the user is an instructor
        _isInstructor = PlayerPrefs.GetString("Type_Of_User", "") == "instructor";

        if (!_isInstructor)
        {
            // Disable this script if not an instructor
            enabled = false;
            return;
        }

        // Unlock and show cursor for instructors
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("[SpectatorController] Spectator mode enabled for instructor");

        // Create spectator camera target
        _spectatorCameraTarget = new GameObject("SpectatorCameraTarget");
        _spectatorCameraTarget.transform.position = Vector3.zero;

        // Find main camera if not assigned
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        // Find Cinemachine virtual camera if not assigned
        if (virtualCamera == null)
        {
            virtualCamera = Object.FindFirstObjectByType<CinemachineCamera>();
        }

        // Initialize camera rotation
        _cinemachineTargetYaw = 0f;
        _cinemachineTargetPitch = 20f;

        // Reset initialization timer - we'll find trainees in Update using unscaled time
        _hasInitialized = false;
        _initializationTimer = 0f;
    }

    private void Update()
    {
        if (!_isInstructor) return;

        // Handle delayed initialization using unscaled time (works even when Time.timeScale = 0)
        if (!_hasInitialized)
        {
            _initializationTimer += Time.unscaledDeltaTime;
            if (_initializationTimer >= INITIALIZATION_DELAY)
            {
                FindAllTraineePlayers();
                _hasInitialized = true;
            }
        }

        // Refresh trainee list periodically (in case players join/leave)
        // Only refresh when game is running (Time.timeScale > 0)
        if (_hasInitialized && Time.timeScale > 0 && Time.frameCount % 60 == 0)
        {
            RefreshTraineeList();
            
            // If we have no trainees, try to find them again
            if (_traineePlayers.Count == 0)
            {
                FindAllTraineePlayers();
            }
        }

        // Check for spacebar press to switch players
        bool spacebarPressed = Input.GetKey(switchPlayerKey);
        if (spacebarPressed && !_lastSpacebarState) // On key down
        {
            SwitchToNextTrainee();
        }
        _lastSpacebarState = spacebarPressed;

        // Check for right-click input
#if ENABLE_INPUT_SYSTEM
        _isRightClickHeld = Mouse.current != null && Mouse.current.rightButton.isPressed;
#else
        _isRightClickHeld = Input.GetMouseButton(1);
#endif

        // Ensure cursor remains visible and unlocked
        if (Cursor.lockState != CursorLockMode.None)
            Cursor.lockState = CursorLockMode.None;
        if (!Cursor.visible)
            Cursor.visible = true;

        // Update spectator camera target position
        UpdateSpectatorCameraTarget();
    }

    private void LateUpdate()
    {
        if (!_isInstructor) return;

        // Only rotate camera when right-click is held
        if (_isRightClickHeld)
        {
            RotateSpectatorCamera();
        }
    }

    /// <summary>
    /// Find all trainee players in the scene (excluding instructor players)
    /// </summary>
    private void FindAllTraineePlayers()
    {
        _traineePlayers.Clear();

        // Method 1: Find by tag "Player"
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
        
        foreach (GameObject player in allPlayers)
        {
            // Check if this player is a trainee (not the instructor's spectator object)
            NetworkObject netObj = player.GetComponent<NetworkObject>();
            if (netObj != null && player != gameObject)
            {
                _traineePlayers.Add(player);
            }
        }

        // Method 2: If no players found by tag, find by NetworkObject
        if (_traineePlayers.Count == 0)
        {
            NetworkObject[] allNetworkObjects = Object.FindObjectsByType<NetworkObject>(FindObjectsSortMode.None);
            foreach (NetworkObject netObj in allNetworkObjects)
            {
                // Look for player characters (must have CharacterController or BoatController)
                if (netObj.gameObject.GetComponent<CharacterController>() != null ||
                    netObj.gameObject.GetComponent<BoatController>() != null)
                {
                    if (netObj.gameObject != gameObject)
                    {
                        _traineePlayers.Add(netObj.gameObject);
                    }
                }
            }
        }

        Debug.Log($"[SpectatorController] Found {_traineePlayers.Count} trainee players");

        // Select the first trainee by default
        if (_traineePlayers.Count > 0)
        {
            _currentPlayerIndex = 0;
            _currentTargetPlayer = _traineePlayers[_currentPlayerIndex];
            
            // Update Cinemachine to follow the new target immediately
            if (virtualCamera != null && _currentTargetPlayer != null)
            {
                virtualCamera.Follow = _spectatorCameraTarget.transform;
                virtualCamera.LookAt = _currentTargetPlayer.transform;
            }
            
            Debug.Log($"[SpectatorController] Now spectating: {_currentTargetPlayer.name}");
        }
        else
        {
            Debug.LogWarning("[SpectatorController] No trainee players found in scene!");
        }
    }

    /// <summary>
    /// Refresh the list of trainee players (removes destroyed players)
    /// </summary>
    private void RefreshTraineeList()
    {
        _traineePlayers.RemoveAll(player => player == null);
        
        // Validate current target
        if (_currentTargetPlayer == null && _traineePlayers.Count > 0)
        {
            _currentPlayerIndex = 0;
            _currentTargetPlayer = _traineePlayers[_currentPlayerIndex];
            
            // Update camera to follow new target
            if (virtualCamera != null && _currentTargetPlayer != null)
            {
                virtualCamera.Follow = _spectatorCameraTarget.transform;
                virtualCamera.LookAt = _currentTargetPlayer.transform;
            }
        }
    }

    /// <summary>
    /// Switch to the next trainee in the list
    /// </summary>
    private void SwitchToNextTrainee()
    {
        if (_traineePlayers.Count == 0)
        {
            // Try to find trainees again if none exist
            FindAllTraineePlayers();
            
            if (_traineePlayers.Count == 0)
            {
                Debug.LogWarning("[SpectatorController] No trainees available to switch to!");
                return;
            }
        }

        // Increment index and wrap around
        _currentPlayerIndex = (_currentPlayerIndex + 1) % _traineePlayers.Count;
        _currentTargetPlayer = _traineePlayers[_currentPlayerIndex];

        // Reset camera angles when switching players
        _cinemachineTargetYaw = _currentTargetPlayer.transform.eulerAngles.y;
        _cinemachineTargetPitch = 20f;
        
        // Update Cinemachine to follow the new target
        if (virtualCamera != null && _currentTargetPlayer != null)
        {
            virtualCamera.Follow = _spectatorCameraTarget.transform;
            virtualCamera.LookAt = _currentTargetPlayer.transform;
        }

        Debug.Log($"[SpectatorController] Switched to trainee {_currentPlayerIndex + 1}/{_traineePlayers.Count}: {_currentTargetPlayer.name}");
    }

    /// <summary>
    /// Update the spectator camera target to follow the current trainee
    /// </summary>
    private void UpdateSpectatorCameraTarget()
    {
        if (_currentTargetPlayer == null || _spectatorCameraTarget == null) return;

        // Position the camera target behind and above the current player
        Vector3 offset = Quaternion.Euler(_cinemachineTargetPitch, _cinemachineTargetYaw, 0f) * new Vector3(0f, spectatorHeight, -spectatorDistance);
        Vector3 targetPosition = _currentTargetPlayer.transform.position + offset;

        _spectatorCameraTarget.transform.position = targetPosition;
        _spectatorCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch, _cinemachineTargetYaw, 0f);

        // Update Cinemachine camera to follow spectator target
        if (virtualCamera != null)
        {
            virtualCamera.Follow = _spectatorCameraTarget.transform;
            virtualCamera.LookAt = _currentTargetPlayer.transform;
        }
        // Fallback: Update main camera directly if no Cinemachine
        else if (mainCamera != null)
        {
            mainCamera.transform.position = _spectatorCameraTarget.transform.position;
            mainCamera.transform.LookAt(_currentTargetPlayer.transform.position + Vector3.up * spectatorHeight);
        }
    }

    private void RotateSpectatorCamera()
    {
        Vector2 mouseDelta = Vector2.zero;

#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            mouseDelta = Mouse.current.delta.ReadValue();
        }
#else
        mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
#endif

        if (mouseDelta.sqrMagnitude < 0.01f) return;

        // Apply camera sensitivity
        _cinemachineTargetYaw += mouseDelta.x * cameraSensitivity * 0.1f;
        _cinemachineTargetPitch -= mouseDelta.y * cameraSensitivity * 0.1f;

        // Clamp pitch rotation
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, bottomClamp, topClamp);
    }

    private static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360f) angle += 360f;
        if (angle > 360f) angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }

    private void OnDestroy()
    {
        // Clean up spectator camera target
        if (_spectatorCameraTarget != null)
        {
            Destroy(_spectatorCameraTarget);
        }
    }

    /// <summary>
    /// Check if current user is an instructor (spectator)
    /// </summary>
    public static bool IsInstructor()
    {
        return PlayerPrefs.GetString("Type_Of_User", "") == "instructor";
    }
    
    /// <summary>
    /// Force refresh the trainee list (call after game starts)
    /// </summary>
    public void ForceRefreshTrainees()
    {
        FindAllTraineePlayers();
    }

    // Debug visualization
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || !_isInstructor || _currentTargetPlayer == null) return;

        // Draw line from spectator to current target
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(_spectatorCameraTarget.transform.position, _currentTargetPlayer.transform.position);
        
        // Draw sphere at current target
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(_currentTargetPlayer.transform.position + Vector3.up * spectatorHeight, 0.5f);
    }
}
