using UnityEngine;
using Unity.Netcode;

public class MedkitInteractable : NetworkBehaviour, IInteractable
{
    [Header("Interaction Settings")]
    [SerializeField] private string healInteractText = "Use medkit on victim";
    [SerializeField] private string rescueInteractText = "Rescue the victim";

    [Header("State Flags (networked)")]
    // Networked state so all clients see the same interaction state
    private NetworkVariable<bool> hasHealedNet = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    private NetworkVariable<bool> hasRescuedNet = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    // Local cached convenience properties (read from NetworkVariables)
    private bool HasHealed => hasHealedNet.Value;
    private bool HasRescued => hasRescuedNet.Value;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        // Optional: subscribe to changes if you want to update visuals when state changes
        hasHealedNet.OnValueChanged += (_, newVal) => OnHealedStateChanged(newVal);
        hasRescuedNet.OnValueChanged += (_, newVal) => OnRescuedStateChanged(newVal);
    }

    public override void OnNetworkDespawn()
    {
        // Unsubscribe to avoid leaks
        hasHealedNet.OnValueChanged -= (_, __) => { };
        hasRescuedNet.OnValueChanged -= (_, __) => { };
        base.OnNetworkDespawn();
    }

    private void OnHealedStateChanged(bool newVal)
    {
        // Hook for visual/audio feedback when healed state changes.
        // Keep minimal here; expand as needed (play sound, change material, etc.)
        if (newVal)
        {
            Debug.Log($"[MedkitInteractable] Victim {gameObject.name} healed (networked).");
        }
    }

    private void OnRescuedStateChanged(bool newVal)
    {
        if (newVal)
        {
            Debug.Log($"[MedkitInteractable] Victim {gameObject.name} rescued (networked).");
        }
    }

    /// <summary>
    /// Called by local player interact logic. Will route the request to the server.
    /// </summary>
    public void Interact(Transform interactorTransform)
    {
        // If this instance is running on the server, perform interaction directly.
        if (IsServer)
        {
            HandleInteractServer(NetworkManager.Singleton.LocalClientId);
            return;
        }

        // Otherwise, request the server to perform the interaction.
        // Use a ServerRpc so the server authoritatively applies state and spawns/despawns.
        InteractRequestServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void InteractRequestServerRpc(ServerRpcParams rpcParams = default)
    {
        // The server will receive the request and handle it for the requesting client.
        HandleInteractServer(rpcParams.Receive.SenderClientId);
    }

    /// <summary>
    /// Server-side interaction handler (authoritative).
    /// </summary>
    private void HandleInteractServer(ulong requestingClientId)
    {
        // STAGE 1: Apply Medkit
        if (!hasHealedNet.Value)
        {
            bool success = ApplyMedkitServer();
            if (success)
            {
                hasHealedNet.Value = true;

                if (PointManager.Instance != null)
                {
                    PointManager.Instance.AddPoints("Healed Victim", 10);
                }

                // Optionally notify the requesting client (could use ClientRpc for UI feedback)
                Debug.Log($"[MedkitInteractable] Client {requestingClientId} healed victim {gameObject.name}.");
            }
            return;
        }

        // STAGE 2: Rescue Victim (only after healing)
        if (hasHealedNet.Value && !hasRescuedNet.Value)
        {
            RescueVictimServer();
            if (PointManager.Instance != null)
            {
                PointManager.Instance.AddPoints("Rescued Victim", 20);
            }
            Debug.Log($"[MedkitInteractable] Client {requestingClientId} rescued victim {gameObject.name}.");
            return;
        }

        // Already rescued
        Debug.Log($"[MedkitInteractable] Victim {gameObject.name} has already been rescued.");
    }

    /// <summary>
    /// Server-side medkit application. Returns true if medkit was consumed.
    /// </summary>
    private bool ApplyMedkitServer()
    {
        // Check if GameManager exists on server
        if (GameManager.Instance == null)
        {
            Debug.LogError("[MedkitInteractable] GameManager.Instance not found on server!");
            return false;
        }

        // Check medkit availability on server-side GameManager
        if (GameManager.Instance.CurrentMedkits == 0)
        {
            Debug.Log("[MedkitInteractable] You don't have medkit!");
            GameManager.Instance.TriggerBlinkEffect();
            return false;
        }

        // Use a medkit via GameManager (server-authoritative)
        bool success = GameManager.Instance.UseMedkit();
        if (success)
        {
            Debug.Log($"[MedkitInteractable] Used 1 medkit on {gameObject.name}. Remaining: {GameManager.Instance.CurrentMedkits}");
            return true;
        }

        return false;
    }

    /// <summary>
    /// Server-side rescue logic: mark rescued and remove the victim across network.
    /// </summary>
    private void RescueVictimServer()
    {
        hasRescuedNet.Value = true;

        // Increment saved victims count in GameManager (server authoritative)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.IncrementSavedVictims();
        }

        // If this victim has a NetworkObject, despawn it so all clients remove it cleanly.
        var netObj = GetComponent<NetworkObject>();
        if (netObj != null && netObj.IsSpawned)
        {
            // Despawn will remove the object across networked clients.
            netObj.Despawn(true);
        }
        else
        {
            // Fallback for non-networked victims
            Destroy(gameObject);
        }
    }

    public string GetInteractText()
    {
        // Return text based on authoritative networked state
        if (!HasHealed)
        {
            return healInteractText;
        }
        else if (HasHealed && !HasRescued)
        {
            return rescueInteractText;
        }
        else
        {
            return ""; // Already rescued
        }
    }

    public Transform GetTransform()
    {
        return transform;
    }
}