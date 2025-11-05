using UnityEngine;
using TMPro;
using System.Collections;

public class MedkitInteractable : MonoBehaviour, IInteractable
{
    [Header("Interaction Settings")]
    [SerializeField] private string healInteractText = "Hold E to use medkit on victim";
    [SerializeField] private string rescueInteractText = "Hold E to rescue the victim";
    [SerializeField] private TextMeshProUGUI updateText;

    [Header("State Flags")]
    private bool hasHealed = false;
    private bool hasRescued = false;
    private float messageDisplayTime = 3f;

    public void Interact(Transform interactorTransform)
    {
        // STAGE 1: Apply Medkit
        if (!hasHealed)
        {
            ApplyMedkit();
            return;
        }

        // STAGE 2: Rescue Victim (only after healing)
        if (hasHealed && !hasRescued)
        {
            RescueVictim();
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
            StartCoroutine(ShowTemporaryMessage("You don't have medkit!"));
            return;
        }

        // Use a medkit
        bool success = MedkitManager.Instance.UseMedkit();
        
        if (success)
        {
            hasHealed = true;
            Debug.Log("Used 1 medkit! Victim is now healed.");
            
            // Show success message temporarily
            string message = "Victim healed! Remaining medkits: " + MedkitManager.Instance.CurrentMedkits;
            StartCoroutine(ShowTemporaryMessage(message));
            
            Debug.Log("Now you can rescue the victim!");
        }
    }

    private void RescueVictim()
    {
        hasRescued = true;
        Debug.Log("Victim " + gameObject.name + " has been rescued!");
        
        // Hide immediately (no message)
        gameObject.SetActive(false);
    }

    private IEnumerator ShowTemporaryMessage(string message)
    {
        if (updateText != null)
        {
            updateText.text = message;
            yield return new WaitForSeconds(messageDisplayTime);
            updateText.text = "";
        }
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