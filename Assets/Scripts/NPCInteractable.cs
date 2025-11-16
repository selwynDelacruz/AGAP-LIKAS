using Unity.VisualScripting;
using UnityEngine;

public class NPCInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactText;
    
    public void Interact(Transform interactorTransform)
    {
        Debug.Log("NPC " + gameObject.name + " has been interacted with.");
        
        // Add points for clearing rubble
        if (PointManager.Instance != null)
        {
            PointManager.Instance.AddPoints("Rescued Victim", 20);
        }
        
        // Increment saved victims count in GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.IncrementSavedVictims();
        }
        else
        {
            Debug.LogWarning("[NPCInteractable] GameManager.Instance not found!");
        }
        
        Destroy(gameObject);
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
