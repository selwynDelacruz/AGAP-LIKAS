using UnityEngine;
using Unity.Netcode;

public class RubbleInteractable : NetworkBehaviour, IInteractable
{
    [SerializeField] private string interactText = "Clear the rubble";

    // Track if already cleared to prevent double-clear
    private bool isCleared = false;

    public void Interact(Transform interactorTransform)
    {
        if (isCleared) return;

        Debug.Log($"[RubbleInteractable] {gameObject.name} interaction requested");

        if (IsServer)
        {
            HandleClearServer();
        }
        else
        {
            RequestClearServerRpc();
        }
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    private void RequestClearServerRpc(RpcParams rpcParams = default)
    {
        if (isCleared) return;

        ulong senderClientId = rpcParams.Receive.SenderClientId;
        Debug.Log($"[RubbleInteractable] Server received clear request from client {senderClientId}");

        HandleClearServer();
    }

    private void HandleClearServer()
    {
        if (isCleared) return;
        isCleared = true;

        Debug.Log($"[RubbleInteractable] {gameObject.name} cleared on server!");

        // Notify all clients about the clear (for points)
        NotifyClearClientRpc();

        // Despawn the networked object
        NetworkObject netObj = GetComponent<NetworkObject>();
        if (netObj != null && netObj.IsSpawned)
        {
            netObj.Despawn(true); // true = destroy GameObject
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void NotifyClearClientRpc()
    {
        Debug.Log("[RubbleInteractable] Clear notification received on client");

        // Add points on all clients
        if (PointManager.Instance != null)
        {
            PointManager.Instance.AddPoints("Cleared Rubble", 10);
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
