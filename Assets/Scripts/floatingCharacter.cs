using UnityEngine;

public class floatingCharacter : MonoBehaviour
{
    [Tooltip("Desired floating height above water")]
    public float floatHeight = 1.0f; // Desired floating height above water
    [Tooltip("How much the movement smooths out")]
    public float bounceDamping = 0.05f; // How much the movement smooths out
    [Tooltip("Height above the character to start raycast")]
    public float waterOffset = 5f; // Height above the character to start raycast
    [Tooltip("Strength of the floating force")]
    public float floatForce = 10f; // Strength of the floating force

    private CharacterController controller;
    private StarterAssets.ThirdPersonController thirdPersonController;
    private bool isFloating = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GetComponent<CharacterController>();
        thirdPersonController = GetComponent<StarterAssets.ThirdPersonController>();
    }

    // Update is called once per frame
    void Update()
    {
        float waterY = GetWaterHeight(transform.position);
        float objectY = transform.position.y;
        float displacement = waterY + floatHeight - objectY;

        if (displacement > 0)
        {
            // Apply floating effect
            Vector3 floatMovement = new Vector3(0, displacement * floatForce * Time.deltaTime, 0);
            controller.Move(floatMovement);

            // Reduce oscillation
            thirdPersonController.Gravity = Mathf.Lerp(thirdPersonController.Gravity, 0f, bounceDamping);

            isFloating = true;
        }
        else
        {
            // Restore normal gravity if not floating
            thirdPersonController.Gravity = -9.81f;
            isFloating = false;
        }
    }

    float GetWaterHeight(Vector3 position)
    {
        RaycastHit hit;
        Vector3 rayStart = new Vector3(position.x, position.y + waterOffset, position.z);

        if (Physics.Raycast(rayStart, Vector3.down, out hit, waterOffset * 2f))
        {
            if (hit.collider.CompareTag("Water")) // Ensure water has the "Water" tag
            {
                return hit.point.y;
            }
        }
        return 0f; // Default water height if no water is hit
    }
}
