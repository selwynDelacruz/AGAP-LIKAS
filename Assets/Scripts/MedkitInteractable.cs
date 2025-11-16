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
            bool success = ApplyMedkit();

            // Only proceed if medkit was successfully applied
            if (success)
            {
                hasHealed = true;
                if (PointManager.Instance != null)
                {
                    PointManager.Instance.AddPoints("Healed Victim", 10);
                }
                Debug.Log("Now you can rescue the victim!");
            }
            return;
        }

        // STAGE 2: Rescue Victim (only after healing)
        if (hasHealed && !hasRescued)
        {
            RescueVictim();
            if (PointManager.Instance != null)
            {
                PointManager.Instance.AddPoints("Rescued Victim", 20);
            }
            return;
        }

        // Already rescued
        Debug.Log("Victim has already been rescued.");
    }

    private bool ApplyMedkit()
    {
        // Check if GameManager exists
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance not found!");
            return false;
        }

        // Check if current medkits is 0
        if (GameManager.Instance.CurrentMedkits == 0)
        {
            Debug.Log("You don't have medkit!");
            GameManager.Instance.TriggerBlinkEffect();
            return false;
        }

        // Use a medkit
        bool success = GameManager.Instance.UseMedkit();
        
        if (success)
        {
            Debug.Log("Used 1 medkit! Victim is now healed.");
            Debug.Log("Victim healed! Remaining medkits: " + GameManager.Instance.CurrentMedkits);
            return true;
        }

        return false;
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