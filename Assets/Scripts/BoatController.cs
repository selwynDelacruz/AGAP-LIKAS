using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RescueBoatController : MonoBehaviour
{
    public float speed = 10f;
    public float turnSpeed = 50f;
    public float buoyancy = 5f;
    public Rigidbody rb;

    void Start()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        rb.linearDamping = 2f;
        // Reduce sliding
        rb.angularDamping = 3f; // Prevent excessive rotation
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void FixedUpdate()
    {
        HandleMovement();
        ApplyBuoyancy();
        StabilizeBoat();
    }

    void HandleMovement()
    {
        float moveInput = Input.GetAxis("Vertical");
        float turnInput = Input.GetAxis("Horizontal");

        if (moveInput != 0)
        {
            Vector3 moveDirection = transform.forward * moveInput * speed;
            rb.AddForce(moveDirection, ForceMode.Force);
        }
        else
        {
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, 2f * Time.deltaTime); // Gradually slow down
        }

        if (turnInput != 0)
        {
            Vector3 turnRotation = Vector3.up * turnInput * turnSpeed * Time.deltaTime;
            rb.MoveRotation(rb.rotation * Quaternion.Euler(turnRotation));
        }
    }

    void ApplyBuoyancy()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 2f))
        {
            if (hit.collider.CompareTag("Water"))
            {
                rb.AddForce(Vector3.up * buoyancy, ForceMode.Acceleration);
            }
        }
    }

    void StabilizeBoat()
    {
        Vector3 flatForward = transform.forward;
        flatForward.y = 0; // Keep rotation level
        Quaternion targetRotation = Quaternion.LookRotation(flatForward, Vector3.up);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.deltaTime * 3f));
    }
}
