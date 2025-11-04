using UnityEngine;
using TMPro;
using System.Runtime.CompilerServices;

public class MedkitInteractable : MonoBehaviour, IInteractable
{
    [Header("Interaction Settings")]
    [SerializeField] private string interactText = "Use medkit to the victim";
    [SerializeField] private TextMeshProUGUI updateText;

    [Header("NPC Interaction")]
    [SerializeField] private bool enableNPCInteractionAfterInteraction = true;
    
    private NPCInteractable npcInteractable;
    private bool hasInteracted = false; // Flag to prevent multiple interactions

    void Start()
    {
        // Get the NPCInteractable component if it exists on the same GameObject
        npcInteractable = GetComponent<NPCInteractable>();
        
        // Disable NPCInteractable if it exists
        if (npcInteractable != null)
        {
            //npcInteractable.enabled = false;
        }
    }

    public void Interact(Transform interactorTransform)
    {
        // Prevent multiple interactions
        if (hasInteracted)
        {
            Debug.Log("Already interacted with this object.");
            return;
        }

        // Check if MedkitManager exists
        if (MedkitManager.Instance == null)
        {
            Debug.LogError("MedkitManager.Instance not found!");
            return;
        }

        // Check if current medkits is 0
        if (MedkitManager.Instance.CurrentMedkits == 0)
        {
            if (updateText != null)
            {
                updateText.text = "You don't have medkit!";
            }
            Debug.Log("You don't have medkit!");
            return;
        }

        // Mark as interacted IMMEDIATELY to prevent double-interaction
        hasInteracted = true;

        // Use a medkit (decrement)
        bool success = MedkitManager.Instance.UseMedkit();
        
        if (success)
        {
            Debug.Log("Used 1 medkit!");
            
            // Update the text to show success
            if (updateText != null)
            {
                updateText.text = "Used medkit! Remaining: " + MedkitManager.Instance.CurrentMedkits;
            }
            
            // Enable NPCInteractable if it exists and if enabled in settings
            if (npcInteractable != null && enableNPCInteractionAfterInteraction)
            {
                npcInteractable.enabled = true;
                Debug.Log("NPCInteractable has been enabled.");
            }
            // Disable this script LAST
            this.enabled = false;
            Debug.Log("MedkitInteractable has been disabled.");
        }
        else
        {
            // If UseMedkit failed, reset the flag
            hasInteracted = false;
        }
    }

    public string GetInteractText()
    {
        return interactText;
    }

    public Transform GetTransform()
    {
        return transform;
    }
}