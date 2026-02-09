using Unity.Netcode;
using UnityEngine;

public class NPCInteractable : NetworkBehaviour, IInteractable
{
    [SerializeField] private string interactText;

    // Track if this victim has been rescued to prevent double-rescue
    private bool isRescued = false;
    
    public void Interact(Transform interactorTransform)
    {
        // Prevent multiple interactions
        if (isRescued) return;

        Debug.Log($"[NPCInteractable] {gameObject.name} interaction requested by {interactorTransform.name}");
        
        // Request server to handle the rescue
        if (IsServer)
        {
            // Server can handle directly
            HandleRescue();
        }
        else
        {
            // Client requests server to handle rescue
            RequestRescueServerRpc();
        }
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    private void RequestRescueServerRpc(RpcParams rpcParams = default)
    {
        // Prevent double-rescue from race conditions
        if (isRescued) return;

        ulong senderClientId = rpcParams.Receive.SenderClientId;
        Debug.Log($"[NPCInteractable] Server received rescue request from client {senderClientId} for {gameObject.name}");
        
        HandleRescue();
    }

    private void HandleRescue()
    {
        if (isRescued) return;
        isRescued = true;

        Debug.Log($"[NPCInteractable] {gameObject.name} has been rescued!");

        // Notify all clients about the rescue (for points/UI updates)
        NotifyRescueClientRpc();

        // Increment saved victims count in GameManager (server-side)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.IncrementSavedVictims();
        }
        else
        {
            Debug.LogWarning("[NPCInteractable] GameManager.Instance not found!");
        }

        // Despawn the networked object (this removes it on all clients)
        NetworkObject netObj = GetComponent<NetworkObject>();
        if (netObj != null && netObj.IsSpawned)
        {
            netObj.Despawn(true); // true = destroy GameObject
        }
        else
        {
            // Fallback for non-networked scenarios
            Destroy(gameObject);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void NotifyRescueClientRpc()
    {
        Debug.Log($"[NPCInteractable] Victim rescued notification received on client");

        // Add points on all clients (PointManager handles local display)
        if (PointManager.Instance != null)
        {
            PointManager.Instance.AddPoints("Rescued Victim", 20);
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
