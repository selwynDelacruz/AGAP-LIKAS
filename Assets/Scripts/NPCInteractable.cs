using Unity.VisualScripting;
using UnityEngine;

public class NPCInteractable : MonoBehaviour
{
    [SerializeField] private string interactText;
    public void Interact()
    {
        Debug.Log("NPC " + gameObject.name + " has been interacted with.");
    }

    public string GetInteractText()
    {
        return interactText;
    }
}
