using Unity.VisualScripting;
using UnityEngine;

public class NPCInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactText;
    public void Interact(Transform interactorTransform)
    {
        Debug.Log("NPC " + gameObject.name + " has been interacted with.");
        gameObject.SetActive(false);
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
