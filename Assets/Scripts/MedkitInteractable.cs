using UnityEngine;

public class MedkitInteractable : MonoBehaviour, IInteractable
{
    [Header("Interaction Settings")]
    [SerializeField] private string healInteractText = "Use medkit on victim";
    [SerializeField] private string rescueInteractText = "Rescue the victim";

    [Header("State Flags")]
    private bool hasHealed = false;
    private bool hasRescued = false;

    public void Interact(Transform interactorTransform)
    {
        // STAGE 1: Apply Medkit
        if (!hasHealed)
        {
            ApplyMedkit();
            PointManager.Instance.AddPoints("Healed Victim", 10);
            hasHealed = true;
            return;
            
        }

        // STAGE 2: Rescue Victim (only after healing)
        if (hasHealed && !hasRescued)
        {
            RescueVictim();
            PointManager.Instance.AddPoints("Rescued Victim", 20);
            return;
        }

        // Already rescued
        Debug.Log("Victim has already been rescued.");
    }

    private void ApplyMedkit()
    {
        // Check if GameManager exists
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance not found!");
            return;
        }

        // Check if current medkits is 0
        if (GameManager.Instance.CurrentMedkits == 0)
        {
            Debug.Log("You don't have medkit!");
            GameManager.Instance.TriggerBlinkEffect();
            return;
        }

        // Use a medkit
        bool success = GameManager.Instance.UseMedkit();
        
        if (success)
        {
            hasHealed = true;
            Debug.Log("Used 1 medkit! Victim is now healed.");
            Debug.Log("Victim healed! Remaining medkits: " + GameManager.Instance.CurrentMedkits);
            Debug.Log("Now you can rescue the victim!");
        }
    }

    private void RescueVictim()
    {
        hasRescued = true;
        Debug.Log("Victim " + gameObject.name + " has been rescued!");
        
        // Increment saved victims count in GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.IncrementSavedVictims();
        }
        
        Destroy(gameObject);
    }

    public string GetInteractText()
    {
        // Show different text based on state
        if (!hasHealed)
        {
            return healInteractText;
        }
        else if (hasHealed && !hasRescued)
        {
            return rescueInteractText;
        }
        else
        {
            return ""; // Already rescued, no interaction text
        }
    }

    public Transform GetTransform()
    {
        return transform;
    }
}