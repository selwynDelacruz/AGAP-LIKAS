using UnityEngine;

public class RubbleInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactText = "Clear the rubble";
    public void Interact(Transform interactorTransform)
    {
        Debug.Log("Rubble " + gameObject.name + " has been interacted with.");
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
