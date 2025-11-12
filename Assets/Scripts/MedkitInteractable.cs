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
        // Check if MedkitManager exists
        if (MedkitManager.Instance == null)
        {
            Debug.LogError("MedkitManager.Instance not found!");
            return;
        }

        // Check if current medkits is 0
        if (MedkitManager.Instance.CurrentMedkits == 0)
        {
            Debug.Log("You don't have medkit!");
            return;
        }

        // Use a medkit
        bool success = MedkitManager.Instance.UseMedkit();
        
        if (success)
        {
            hasHealed = true;
            Debug.Log("Used 1 medkit! Victim is now healed.");
            Debug.Log("Victim healed! Remaining medkits: " + MedkitManager.Instance.CurrentMedkits);
            Debug.Log("Now you can rescue the victim!");
        }
    }

    private void RescueVictim()
    {
        hasRescued = true;
        Debug.Log("Victim " + gameObject.name + " has been rescued!");
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