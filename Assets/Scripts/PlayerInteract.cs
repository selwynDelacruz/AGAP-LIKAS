using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.E))
        //{
        //    float interactRange = 2.0f;
        //    Collider[] colliderArray = Physics.OverlapSphere(transform.position, interactRange);
        //    foreach (Collider collider in colliderArray)
        //    {
        //        if (collider.gameObject.TryGetComponent(out NPCInteractable npcInteractable))
        //        {
        //            npcInteractable.Interact(transform);
        //        }
        //    }
        //}
        if (Input.GetKeyDown(KeyCode.E))
        {
            IInteractable interactable = GetInteractableObject();
            if (interactable != null)
            {
                interactable.Interact(transform);
            }
        }
    }

    public IInteractable GetInteractableObject()
    {
        List<IInteractable> interactableList = new List<IInteractable>();
        float interactRange = 2.0f;
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
}
