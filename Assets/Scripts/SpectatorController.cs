using UnityEngine;
using PlayerInputControl;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Controls spectator mode for instructors. Disables player movement
/// and allows camera rotation around the active player using right-click.
/// </summary>
public class SpectatorController : MonoBehaviour
{
    [Header("Spectator Settings")]
    [Tooltip("Camera sensitivity for spectator mode")]
    public float SpectatorCameraSensitivity = 1.0f;

    [Tooltip("Smooth camera rotation")]
    public float CameraRotationSmoothTime = 0.12f;

    [Header("Camera References")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera")]
    public GameObject CinemachineCameraTarget;

    [Tooltip("How far in degrees can you move the camera up")]
    public float TopClamp = 70.0f;

    [Tooltip("How far in degrees can you move the camera down")]
    public float BottomClamp = -30.0f;

    // Private variables
    private bool isInstructor = false;
    private PlayerInputs playerInputs;
    private float cinemachineTargetYaw;
    private float cinemachineTargetPitch;
    private bool isRightClickHeld = false;

#if ENABLE_INPUT_SYSTEM
    private PlayerInput playerInput;
#endif

    private void Awake()
    {
        // Check if the user is an instructor
        string userType = PlayerPrefs.GetString("Type_Of_User", "");
        isInstructor = userType.Equals("instructor", System.StringComparison.OrdinalIgnoreCase);

        Debug.Log($"[SpectatorController] User Type: {userType}, Is Instructor: {isInstructor}");
    }

    private void Start()
    {
        if (isInstructor)
        {
            EnableSpectatorMode();
        }
    }

    private void EnableSpectatorMode()
    {
        Debug.Log("[SpectatorController] Enabling Spectator Mode for Instructor");

        // Get PlayerInputs component (used by both BoatController and ThirdPersonController)
        playerInputs = GetComponent<PlayerInputs>();
        if (playerInputs != null)
        {
            // Disable movement and look input
            playerInputs.cursorInputForLook = false;
            playerInputs.cursorLocked = false;
        }

#if ENABLE_INPUT_SYSTEM
        // Get PlayerInput component
        playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            // Disable the player input actions to prevent control
            playerInput.DeactivateInput();
            Debug.Log("[SpectatorController] Player input deactivated");
        }
#endif

        // Disable movement controllers while keeping camera target active
        DisablePlayerControllers();

        // Set cursor to visible and unlocked for instructors
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Initialize camera rotation to current camera target rotation
        if (CinemachineCameraTarget != null)
        {
            cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
            cinemachineTargetPitch = CinemachineCameraTarget.transform.rotation.eulerAngles.x;
        }
    }

    private void DisablePlayerControllers()
    {
        // Disable BoatController if present (for flood scenario)
        BoatController boatController = GetComponent<BoatController>();
        if (boatController != null)
        {
            boatController.enabled = false;
            Debug.Log("[SpectatorController] BoatController disabled");
        }

        // Disable ThirdPersonController if present (for earthquake scenario)
        StarterAssets.AgapThirdPersonController thirdPersonController = GetComponent<StarterAssets.AgapThirdPersonController>();
        if (thirdPersonController != null)
        {
            thirdPersonController.enabled = false;
            Debug.Log("[SpectatorController] ThirdPersonController disabled");
        }

        // Disable CharacterController to prevent physics-based movement
        CharacterController characterController = GetComponent<CharacterController>();
        if (characterController != null)
        {
            characterController.enabled = false;
            Debug.Log("[SpectatorController] CharacterController disabled");
        }

        // Disable Rigidbody physics if present (for boat)
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            Debug.Log("[SpectatorController] Rigidbody set to kinematic");
        }
    }

    private void Update()
    {
        if (!isInstructor || CinemachineCameraTarget == null)
            return;

        HandleSpectatorCamera();
    }

    private void LateUpdate()
    {
        if (!isInstructor || CinemachineCameraTarget == null)
            return;

        // Apply camera rotation
        CameraRotation();
    }

    private void HandleSpectatorCamera()
    {
        // Check for right mouse button hold
        if (Input.GetMouseButtonDown(1))
        {
            isRightClickHeld = true;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            isRightClickHeld = false;
        }

        // Only rotate camera when right-click is held
        if (isRightClickHeld)
        {
            // Get mouse delta movement
            float mouseX = Input.GetAxis("Mouse X") * SpectatorCameraSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * SpectatorCameraSensitivity;

            // Adjust yaw and pitch
            cinemachineTargetYaw += mouseX;
            cinemachineTargetPitch -= mouseY; // Inverted for natural camera movement

            // Clamp pitch to prevent camera flipping
            cinemachineTargetPitch = ClampAngle(cinemachineTargetPitch, BottomClamp, TopClamp);
        }
    }

    private void CameraRotation()
    {
        // Apply rotation to Cinemachine camera target
        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(
            cinemachineTargetPitch,
            cinemachineTargetYaw,
            0.0f
        );
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    private void OnEnable()
    {
        // Re-apply spectator settings when object is re-enabled
        if (isInstructor)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void OnDisable()
    {
        // Clean up when disabled
        isRightClickHeld = false;
    }
}
