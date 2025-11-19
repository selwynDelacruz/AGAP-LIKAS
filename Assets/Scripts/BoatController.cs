using PlayerInputControl;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody))]
#if ENABLE_INPUT_SYSTEM
[RequireComponent(typeof(PlayerInput))]
#endif
public class BoatController :  MonoBehaviour
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

    void Start()
    {
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

        // Get input component
        _input = GetComponent<PlayerInputs>();
        if (_input == null)
        {
            Debug.LogWarning("BoatController: PlayerInputs component not found. Camera rotation will not work.", this);
        }

#if ENABLE_INPUT_SYSTEM
        _playerInput = GetComponent<PlayerInput>();
#endif
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

        // --- steering (force applied at motor position to create turning moment) ---
        if (steer != 0)
        {
            // Use AddForceAtPosition to create a turning moment around center of mass.
            // Use ForceMode.Acceleration so effect is independent of mass during tuning.
            Vector3 steerForce = steer * transform.right * SteerPower;
            rb.AddForceAtPosition(steerForce, Motor.position, ForceMode.Acceleration);
        }

        // --- forward/backward movement (horizontal only) ---
        Vector3 forwardXZ = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
        Vector3 currentVelXZ = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (forwardInput != 0)
        {
            // desired horizontal velocity
            Vector3 desiredVel = forwardXZ * (MaxSpeed * forwardInput);

            // velocity delta we want to correct this FixedUpdate
            Vector3 velocityDelta = desiredVel - currentVelXZ;

            // apply acceleration proportional to Power (ForceMode.Acceleration uses mass-independent units)
            // clamp the applied acceleration so it is stable
            float maxAccel = Mathf.Max(0.001f, Power);
            Vector3 accel = Vector3.ClampMagnitude(velocityDelta / Time.fixedDeltaTime, maxAccel);

            rb.AddForce(accel, ForceMode.Acceleration);
        }
        else
        {
            // No input -> gently decelerate horizontal velocity so boat slows naturally
            float decel = StopDeceleration;
            if (decel > 0f)
            {
                Vector3 decelAccel = -currentVelXZ.normalized * decel;
                // Only apply deceleration if velocity exists and decel will not reverse direction in a single step
                if (currentVelXZ.magnitude > 0.01f)
                    rb.AddForce(decelAccel, ForceMode.Acceleration);
            }
        }

        // --- motor transform (visual) and particles ---
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
        CameraRotation();
    }

    private void CameraRotation()
    {
        // Skip if no input or camera target
        if (_input == null || CinemachineCameraTarget == null) return;

        // if there is an input and camera position is not fixed
        if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
        {
            // Don't multiply mouse input by Time.deltaTime;
            float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

            _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
            _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
        }

        // clamp our rotations so our values are limited 360 degrees
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
