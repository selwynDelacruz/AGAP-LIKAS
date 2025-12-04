using PlayerInputControl;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody))]
#if ENABLE_INPUT_SYSTEM
[RequireComponent(typeof(PlayerInput))]
#endif
public class BoatController : MonoBehaviour
{
    [Header("References")]
    public Transform Motor;                      // visual motor location (force applied here for turning)
    public ParticleSystem MotorParticles;        // optional: drag a particle system in inspector

    [Header("Movement")]
    public float SteerPower = 500f;              // torque-like effect for turning
    public float Power = 10f;                    // responsiveness / acceleration multiplier
    public float MaxSpeed = 10f;                 // max horizontal speed (m/s)
    public float StopDeceleration = 2f;          // deceleration when no input (m/s^2)

    [Header("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    public GameObject CinemachineCameraTarget;
    
    [Tooltip("How far in degrees can you move the camera up")]
    public float TopClamp = 70.0f;

    [Tooltip("How far in degrees can you move the camera down")]
    public float BottomClamp = -30.0f;

    [Tooltip("Additional degrees to override the camera. Useful for fine tuning camera position when locked")]
    public float CameraAngleOverride = 0.0f;

    [Tooltip("For locking the camera position on all axis")]
    public bool LockCameraPosition = false;

    // Cinemachine
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;

    // Input
    private PlayerInputs _input;
#if ENABLE_INPUT_SYSTEM
    private PlayerInput _playerInput;
#endif

    // Rigidbody and motor
    private Rigidbody rb;
    private Quaternion motorStartRot;
    private bool isInitialized = false;

    private const float _threshold = 0.01f;

    private bool IsCurrentDeviceMouse
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            return _playerInput != null && _playerInput.currentControlScheme == "KeyboardMouse";
#else
            return false;
#endif
        }
    }

    // **CRITICAL: Use OnEnable instead of Start for objects that get enabled mid-scene**
    private void OnEnable()
    {
        if (!isInitialized)
        {
            Initialize();
        }
        else
        {
            // Re-enable input when boat is re-enabled
            ReactivateInput();
        }

        // Lock cursor when boat is enabled
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Debug.Log($"[BoatController] OnEnable - Cursor LOCKED");
    }

    private void Initialize()
    {
        Debug.Log("[BoatController] Initializing...");

        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("BoatController requires a Rigidbody.", this);
            enabled = false;
            return;
        }

        if (Motor == null)
        {
            Debug.LogError("BoatController: assign the Motor transform in the inspector.", this);
            enabled = false;
            return;
        }

        if (MotorParticles == null)
            MotorParticles = Motor.GetComponentInChildren<ParticleSystem>();

        motorStartRot = Motor.localRotation;

        // Initialize camera target yaw
        if (CinemachineCameraTarget != null)
        {
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
        }

        // **CRITICAL: Get and activate PlayerInput**
#if ENABLE_INPUT_SYSTEM
        _playerInput = GetComponent<PlayerInput>();
        if (_playerInput == null)
        {
            Debug.LogError("[BoatController] PlayerInput component missing!");
        }
        else
        {
            // Force activate the PlayerInput
            _playerInput.enabled = true;
            _playerInput.ActivateInput();
            Debug.Log($"[BoatController] PlayerInput activated - Enabled: {_playerInput.enabled}, Actions: {_playerInput.actions != null}");
        }
#endif

        _input = GetComponent<PlayerInputs>();
        if (_input == null)
        {
            Debug.LogError("[BoatController] PlayerInputs script missing!");
        }
        else if (_input is MonoBehaviour inputBehaviour)
        {
            inputBehaviour.enabled = true;
            // **FIX: Force enable cursor input for look**
            _input.cursorInputForLook = true;
            _input.cursorLocked = true;
            Debug.Log("[BoatController] PlayerInputs enabled with cursorInputForLook=true");
        }

        isInitialized = true;
        Debug.Log("[BoatController] Initialization complete");
    }

    private void ReactivateInput()
    {
#if ENABLE_INPUT_SYSTEM
        if (_playerInput != null)
        {
            _playerInput.enabled = true;
            _playerInput.ActivateInput();
            Debug.Log("[BoatController] Input reactivated");
        }
#endif

        if (_input != null && _input is MonoBehaviour inputBehaviour)
        {
            inputBehaviour.enabled = true;
            // **FIX: Re-enable cursor input for look when reactivating**
            _input.cursorInputForLook = true;
            _input.cursorLocked = true;
        }
    }

    // **Deactivate input when boat is disabled**
    private void OnDisable()
    {
#if ENABLE_INPUT_SYSTEM
        if (_playerInput != null)
        {
            _playerInput.DeactivateInput();
            Debug.Log("[BoatController] Input deactivated");
        }
#endif

        // Unlock cursor when leaving boat
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("[BoatController] OnDisable - Cursor UNLOCKED");
    }

    void Update()
    {
        // **TEST: Manual input check**
        if (Input.GetKeyDown(KeyCode.M))
        {
#if ENABLE_INPUT_SYSTEM
            Debug.Log($"[BoatController] Manual Check - PlayerInput: {(_playerInput != null ? "Found" : "NULL")}, " +
                     $"Enabled: {(_playerInput != null ? _playerInput.enabled.ToString() : "N/A")}, " +
                     $"Actions: {(_playerInput != null && _playerInput.actions != null ? "Active" : "NULL")}, " +
                     $"_input.look: {(_input != null ? _input.look.ToString() : "NULL")}");
#endif
        }
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        // --- read input ---
        int steer = 0;
        if (Input.GetKey(KeyCode.A)) steer = 1;
        else if (Input.GetKey(KeyCode.D)) steer = -1;

        int forwardInput = 0;
        if (Input.GetKey(KeyCode.W)) forwardInput = 1;
        else if (Input.GetKey(KeyCode.S)) forwardInput = -1;

        // --- steering ---
        if (steer != 0)
        {
            Vector3 steerForce = steer * transform.right * SteerPower;
            rb.AddForceAtPosition(steerForce, Motor.position, ForceMode.Acceleration);
        }

        // --- forward/backward movement ---
        Vector3 forwardXZ = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
        Vector3 currentVelXZ = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (forwardInput != 0)
        {
            Vector3 desiredVel = forwardXZ * (MaxSpeed * forwardInput);
            Vector3 velocityDelta = desiredVel - currentVelXZ;
            float maxAccel = Mathf.Max(0.001f, Power);
            Vector3 accel = Vector3.ClampMagnitude(velocityDelta / Time.fixedDeltaTime, maxAccel);
            rb.AddForce(accel, ForceMode.Acceleration);
        }
        else
        {
            float decel = StopDeceleration;
            if (decel > 0f)
            {
                Vector3 decelAccel = -currentVelXZ.normalized * decel;
                if (currentVelXZ.magnitude > 0.01f)
                    rb.AddForce(decelAccel, ForceMode.Acceleration);
            }
        }

        // --- motor transform and particles ---
        Motor.SetPositionAndRotation(Motor.position, transform.rotation * motorStartRot * Quaternion.Euler(0f, 30f * steer, 0f));

        if (MotorParticles != null)
        {
            bool wantOn = forwardInput != 0;
            if (wantOn && !MotorParticles.isPlaying) MotorParticles.Play();
            if (!wantOn && MotorParticles.isPlaying) MotorParticles.Stop();
        }
    }

    void LateUpdate()
    {
        // **DEBUG: Log status periodically**
        if (Time.frameCount % 120 == 0) // Every 2 seconds
        {
            if (_input != null)
            {
                Debug.Log($"[BoatController] Status - Look: {_input.look}, Cursor: {Cursor.lockState}");
            }
        }

        CameraRotation();
    }

    private void CameraRotation()
    {
        if (_input == null || CinemachineCameraTarget == null) return;

        // if there is an input and camera position is not fixed
        if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
        {
            float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

            _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
            _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
        }

        // clamp our rotations
        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        // Cinemachine will follow this target
        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
            _cinemachineTargetYaw, 0.0f);
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }
}
