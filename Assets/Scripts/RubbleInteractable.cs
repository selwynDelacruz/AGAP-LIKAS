using UnityEngine;
using Unity.Netcode;

public class RubbleInteractable : NetworkBehaviour, IInteractable
{
    [SerializeField] private string interactText = "Clear the rubble";

    // Networked cleared flag so all clients observe same state
    private NetworkVariable<bool> isClearedNet = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        isClearedNet.OnValueChanged += OnClearedChanged;
    }

    public override void OnNetworkDespawn()
    {
        isClearedNet.OnValueChanged -= OnClearedChanged;
        base.OnNetworkDespawn();
    }

    private void OnClearedChanged(bool previousValue, bool newValue)
    {
        if (newValue)
        {
            Debug.Log($"[RubbleInteractable] {gameObject.name} marked cleared (networked).");
            // Optional: play VFX / disable colliders / change model here for clients that still exist
        }
    }

    public void Interact(Transform interactorTransform)
    {
        // Prevent duplicate requests locally
        if (isClearedNet.Value) return;

        Debug.Log($"[RubbleInteractable] Interaction requested on '{gameObject.name}' by local {NetworkManager.Singleton.LocalClientId}");

        // If running on server/host perform immediately, otherwise request server
        if (IsServer)
        {
            HandleClearServer();
        }
        else
        {
            RequestClearServerRpc();
        }
    }

    // Client -> Server: request clear (requireOwnership = false so any client can request)
    [ServerRpc(RequireOwnership = false)]
    private void RequestClearServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong sender = rpcParams.Receive.SenderClientId;
        Debug.Log($"[RubbleInteractable] Server received clear request from client {sender} for '{gameObject.name}'");

        // Server handles authoritatively
        HandleClearServer();
    }

    // Server-side authoritative handling
    private void HandleClearServer()
    {
        if (isClearedNet.Value) return;

        // Mark cleared (networked)
        isClearedNet.Value = true;

        Debug.Log($"[RubbleInteractable] '{gameObject.name}' cleared on server");

        // Award points server-side if PointManager is server-authoritative
        if (PointManager.Instance != null)
        {
            PointManager.Instance.AddPoints("Cleared Rubble", 10);
        }

        // Notify clients (including host) to run any client-side feedback
        NotifyClearClientRpc();

        // Despawn the networked object so it's removed across all clients
        var netObj = GetComponent<NetworkObject>();
        if (netObj != null && netObj.IsSpawned)
        {
            netObj.Despawn(true); // true => destroy GameObject on clients
        }
        else
        {
            // Fallback for non-networked objects
            Destroy(gameObject);
        }
    }

    // Server -> Clients: inform them of the clear (runs on clients and host)
    [ClientRpc]
    private void NotifyClearClientRpc(ClientRpcParams clientRpcParams = default)
    {
        Debug.Log($"[RubbleInteractable] Clear notification received on client for '{gameObject.name}'");

        // Client-side point UI / feedback (optional)
        if (PointManager.Instance != null)
        {
            PointManager.Instance.AddPoints("Cleared Rubble", 10);
        }
    }

    public string GetInteractText()
    {
        // Use networked state so clients show correct text
        return isClearedNet.Value ? "" : interactText;
    }

    public Transform GetTransform()
    {
        return transform;
    }
}
