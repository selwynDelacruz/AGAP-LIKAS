using UnityEngine;

public class characterBuoyancy : MonoBehaviour
{
    [Header("Water Settings")]
    [Tooltip("The Y-position of the water surface. The character will swim at this height unless diving.")]
    public float waterHeight = 0f;
    [Tooltip("Offset from water height to determine the character's floating level.")]
    public float waterOffset = 1.0f;
    [Tooltip("How fast the character floats up to the surface when idle.")]
    public float floatingPower = 5f;
    [Tooltip("How smoothly the character stabilizes at the water surface.")]
    public float floatDamping = 2f;
    [Tooltip("Multiplier for movement speed when swimming.")]
    public float swimSpeedMultiplier = 0.5f;

    private CharacterController controller;
    private StarterAssets.ThirdPersonController thirdPersonController;
    private bool isSwimming = false;
    private Vector3 velocity;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GetComponent<CharacterController>();
        thirdPersonController = GetComponent<StarterAssets.ThirdPersonController>();
    }

    // Update is called once per frame
    void Update()
    {
        float targetWaterLevel = waterHeight - waterOffset; // Adjust floating position with offset
        float difference = targetWaterLevel - transform.position.y;
        bool isUnderwater = difference > 0;

        if (isUnderwater)
        {
            if (!isSwimming)
            {
                isSwimming = true;
                thirdPersonController.MoveSpeed *= swimSpeedMultiplier; // Reduce movement speed for swimming
                thirdPersonController.Gravity = 0f; // Disable gravity in water
            }

            if (Input.GetKey(KeyCode.Space)) // Ascend (swim up)
            {
                velocity.y = floatingPower * Time.deltaTime;
            }
            else if (Input.GetKey(KeyCode.LeftControl)) // Descend (swim down)
            {
                velocity.y = -floatingPower * Time.deltaTime;
            }
            else
            {
                // If idle, gently float toward water surface
                float targetY = Mathf.Lerp(transform.position.y, targetWaterLevel, Time.deltaTime * floatDamping);
                velocity.y = (targetY - transform.position.y) / Time.deltaTime;
            }
        }
        else
        {
            if (isSwimming)
            {
                isSwimming = false;
                thirdPersonController.MoveSpeed /= swimSpeedMultiplier; // Restore normal speed
                thirdPersonController.Gravity = -9.81f; // Restore normal gravity
            }

            velocity.y += Physics.gravity.y * Time.deltaTime; // Apply normal gravity when above water
        }

        controller.Move(velocity * Time.deltaTime);
    }
}
