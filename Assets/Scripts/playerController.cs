using UnityEngine;

public class playerController : MonoBehaviour
{
    Animator animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();

    }

    // Update is called once per frame
    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right
        float moveZ = Input.GetAxisRaw("Vertical");   // W/S or Up/Down

        Vector3 move = new Vector3(moveX, 0, moveZ);

        if (move.magnitude > 0.1f)
        {
            //// Move the character
            //transform.Translate(move.normalized * moveSpeed * Time.deltaTime, Space.World);

            // Set walking animation
            if (animator != null)
                animator.SetBool("isWalking", true);

            // Optional: Rotate character to face movement direction
            transform.forward = move.normalized;
        }
        else
        {
            if (animator != null)
                animator.SetBool("isWalking", false);
        }
    }
}
