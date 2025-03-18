using UnityEngine;

[RequireComponent (typeof(Rigidbody))]
public class FloatingObject : MonoBehaviour
{
    [Tooltip("Desired floating height")]
    public float floatHeight = 1.5f;  // Desired floating height
    [Tooltip("How much the object stabilizes")]
    public float bounceDamping = 0.05f;  // How much the object stabilizes
    [Tooltip("How high the ray starts above the object")]
    public float waterOffset = 5f;  // How high the ray starts above the object

    private Rigidbody rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float waterY = GetWaterHeight(transform.position);
        float objectY = transform.position.y;
        float displacement = waterY + floatHeight - objectY;

        if (displacement > 0)
        {
            rb.AddForce(Vector3.up * displacement * 10f, ForceMode.Acceleration);
            rb.linearVelocity *= (1f - bounceDamping);  // Reduce oscillation
        }
    }

    float GetWaterHeight(Vector3 position)
    {
        RaycastHit hit;
        Vector3 rayStart = new Vector3(position.x, position.y + waterOffset, position.z);

        if (Physics.Raycast(rayStart, Vector3.down, out hit, waterOffset * 2f))
        {
            if (hit.collider.CompareTag("Water"))  // Make sure water is tagged correctly
            {
                return hit.point.y;
            }
        }
        return 0f; // Default water height if nothing is hit
    }
}
