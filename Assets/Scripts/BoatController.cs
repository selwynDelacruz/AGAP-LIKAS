using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
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

    Rigidbody rb;
    Quaternion motorStartRot;

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
}
