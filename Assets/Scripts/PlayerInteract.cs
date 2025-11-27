using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    [Header("Player Interaction Settings")]
    [Tooltip("Time in seconds the key must be held to interact")]
    public float holdDuration = 5.0f;
    public float interactRange = 2.0f;

    private float holdTimer = 0f;
    private bool isHolding = false;
    private IInteractable currentInteractable = null;

    void Update()
    {
        if (Input.GetKey(KeyCode.E))
        {   
            if (!isHolding)
            {
                // Start holding
                isHolding = true;
                currentInteractable = GetInteractableObject();
                holdTimer = 0f;

                // Check if interacting with MedkitInteractable and validate medkit availability
                if (currentInteractable != null)
                {
                    // Check if the interactable is a MedkitInteractable
                    MonoBehaviour interactableMono = currentInteractable as MonoBehaviour;
                    if (interactableMono != null)
                    {
                        MedkitInteractable medkitInteractable = interactableMono.GetComponent<MedkitInteractable>();
                        
                        if (medkitInteractable != null)
                        {
                            // Check the interaction text to determine the current stage
                            string interactText = medkitInteractable.GetInteractText();
                            
                            // If the text indicates we need to use a medkit (Stage 1)
                            if (interactText.Contains("medkit") || interactText.Contains("Use"))
                            {
                                // Check if player has medkits
                                if (GameManager.Instance != null && GameManager.Instance.CurrentMedkits == 0)
                                {
                                    // No medkits available, do not allow interaction
                                    Debug.Log("You don't have medkit!");
                                    GameManager.Instance.TriggerBlinkEffect();
                                    
                                    // Reset holding state
                                    isHolding = false;
                                    currentInteractable = null;
                                    return;
                                }
                            }
                            // If the text indicates rescue (Stage 2), no medkit check needed
                            // Player can proceed with the rescue
                        }
                    }
                }
            }
            else if (currentInteractable != null)
            {
                // Continue holding
                holdTimer += Time.deltaTime;

                if (holdTimer >= holdDuration)
                {
                    // Hold complete - trigger interaction
                    currentInteractable.Interact(transform);

                    // Reset to prevent continuous interaction
                    isHolding = false;
                    holdTimer = 0f;
                    currentInteractable = null;
                }
            }
        }
        else if (Input.GetKeyUp(KeyCode.E))
        {
            // Key released - reset
            isHolding = false;
            holdTimer = 0f;
            currentInteractable = null;
        }
    }

    public IInteractable GetInteractableObject()
    {
        List<IInteractable> interactableList = new List<IInteractable>();
        Collider[] colliderArray = Physics.OverlapSphere(transform.position, interactRange);
        foreach (Collider collider in colliderArray)
        {
            // Get ALL IInteractable components on this GameObject
            IInteractable[] allInteractables = collider.GetComponents<IInteractable>();

            if (collider.TryGetComponent(out IInteractable interactable))
            {
                interactableList.Add(interactable);
            }
        }

        IInteractable closestNPCInteractable = null;
        foreach (IInteractable interactable in interactableList)
        {
            if (closestNPCInteractable == null)
            {
                closestNPCInteractable = interactable;
            }
            else
            {
                if (Vector3.Distance(transform.position, interactable.GetTransform().position) <
                    Vector3.Distance(transform.position, closestNPCInteractable.GetTransform().position))
                {
                    //closer
                    closestNPCInteractable = interactable;
                }
            }
        }
        return closestNPCInteractable;
    }

    /// <summary>
    /// Get the current hold progress (0 to 1)
    /// </summary>
    public float GetHoldProgress()
    {
        return holdDuration > 0 ? Mathf.Clamp01(holdTimer / holdDuration) : 0f;
    }

    /// <summary>
    /// Check if currently holding the interact key
    /// </summary>
    public bool IsHolding()
    {
        return isHolding;
    }
}
