using UnityEngine;

public class FloatingObject : MonoBehaviour
{
    public float waterLevel = 5f; // Set this to match your water height
    public float buoyancyForce = 10f; // Adjust this for stronger floating
    public float waterDrag = 1f; // Slows down movement in water
    public float angularDrag = 1f; // Slows down rotation in water

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (transform.position.y < waterLevel) // If object is below water
        {
            float forceAmount = buoyancyForce * (waterLevel - transform.position.y);
            rb.AddForce(Vector3.up * forceAmount, ForceMode.Force); // Apply buoyancy

            rb.linearDamping = waterDrag; // Apply water drag
            rb.angularDamping = angularDrag; // Apply rotational drag
        }
        else
        {
            rb.linearDamping = 0; // Reset drag in air
            rb.angularDamping = 0;
        }
    }
}
