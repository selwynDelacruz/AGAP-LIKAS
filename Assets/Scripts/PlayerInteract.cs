using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerInteract : NetworkBehaviour
{
    [Header("Player Interaction Settings")]
    [Tooltip("Time in seconds the key must be held to interact")]
    public float holdDuration = 5.0f;
    public float interactRange = 2.0f;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;

    private float holdTimer = 0f;
    private bool isHolding = false;
    private IInteractable currentInteractable = null;

    void Update()
    {
        // Only allow input on the local player (owner)
        if (!IsOwner) return;

        if (Input.GetKey(KeyCode.E))
        {   
            if (!isHolding)
            {
                // Start holding
                isHolding = true;
                currentInteractable = GetInteractableObject();
                holdTimer = 0f;

                if (showDebugLogs)
                {
                    Debug.Log($"[PlayerInteract] Started holding E. Found interactable: {(currentInteractable != null ? currentInteractable.GetTransform().name : "None")}");
                }

                // Check if interacting with MedkitInteractable and if player has no medkits
                if (currentInteractable != null)
                {
                    // Check if the interactable is a MedkitInteractable
                    MonoBehaviour interactableMono = currentInteractable as MonoBehaviour;
                    if (interactableMono != null && interactableMono.GetComponent<MedkitInteractable>() != null)
                    {
                        // Check if GameManager exists before accessing it
                        if (GameManager.Instance == null)
                        {
                            Debug.LogError("[PlayerInteract] GameManager.Instance is null! Make sure GameManager exists in the scene.");
                            isHolding = false;
                            currentInteractable = null;
                            return;
                        }

                        // Only check medkit availability for MedkitInteractable objects
                        if (GameManager.Instance.CurrentMedkits == 0)
                        {
                            // No medkits available, do not allow interaction
                            Debug.Log("You don't have medkit!");
                            GameManager.Instance.TriggerBlinkEffect();
                            
                            // Reset holding state
                            isHolding = false;
                            currentInteractable = null;
                            return;
                        }
                    }
                }
            }
            else if (currentInteractable != null)
            {
                // Continue holding
                holdTimer += Time.deltaTime;

                if (holdTimer >= holdDuration)
                {
                    if (showDebugLogs)
                    {
                        Debug.Log($"[PlayerInteract] Hold complete! Triggering network interaction.");
                    }

                    // Hold complete - trigger network interaction
                    TriggerNetworkInteraction(currentInteractable);

                    // Reset to prevent continuous interaction
                    isHolding = false;
                    holdTimer = 0f;
                    currentInteractable = null;
                }
            }
        }
        else if (Input.GetKeyUp(KeyCode.E))
        {
            if (isHolding && showDebugLogs)
            {
                Debug.Log($"[PlayerInteract] E key released early at {holdTimer:F1}s");
            }

            // Key released - reset
            isHolding = false;
            holdTimer = 0f;
            currentInteractable = null;
        }
    }

    /// <summary>
    /// Triggers the network interaction for the given interactable
    /// </summary>
    private void TriggerNetworkInteraction(IInteractable interactable)
    {
        if (interactable == null) return;

        MonoBehaviour interactableMono = interactable as MonoBehaviour;
        if (interactableMono == null) return;

        // Get the NetworkObject if it exists
        NetworkObject networkObject = interactableMono.GetComponent<NetworkObject>();

        if (networkObject != null)
        {
            // Network object - use ServerRpc
            if (showDebugLogs)
            {
                Debug.Log($"[PlayerInteract] Networked interactable detected: {interactableMono.name} (NetworkObjectId: {networkObject.NetworkObjectId})");
            }

            // Check type and call appropriate ServerRpc
            if (interactableMono.GetComponent<NPCInteractable>() != null)
            {
                RequestVictimRescueServerRpc(networkObject.NetworkObjectId);
            }
            else if (interactableMono.GetComponent<MedkitInteractable>() != null)
            {
                RequestMedkitUseServerRpc(networkObject.NetworkObjectId);
            }
            else if (interactableMono.GetComponent<RubbleInteractable>() != null)
            {
                RequestRubbleClearServerRpc(networkObject.NetworkObjectId);
            }
            else
            {
                // Generic networked interactable
                RequestGenericInteractionServerRpc(networkObject.NetworkObjectId);
            }
        }
        else
        {
            // Non-networked object - interact locally (backwards compatibility)
            if (showDebugLogs)
            {
                Debug.LogWarning($"[PlayerInteract] Non-networked interactable: {interactableMono.name}. Using local interaction.");
            }
            interactable.Interact(transform);
        }
    }

    #region Server RPCs

    /// <summary>
    /// Server RPC to rescue a victim (NPC)
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void RequestVictimRescueServerRpc(ulong networkObjectId)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[PlayerInteract] Server: Processing victim rescue request for NetworkObjectId: {networkObjectId}");
        }

        // Find the NetworkObject
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject netObj))
        {
            Debug.LogWarning($"[PlayerInteract] Server: NetworkObject {networkObjectId} not found!");
            return;
        }

        // Verify it's an NPCInteractable
        NPCInteractable npc = netObj.GetComponent<NPCInteractable>();
        if (npc == null)
        {
            Debug.LogWarning($"[PlayerInteract] Server: Object {networkObjectId} is not an NPCInteractable!");
            return;
        }

        // Award points on server (PointManager should be server-authoritative)
        if (PointManager.Instance != null)
        {
            PointManager.Instance.AddPoints("Rescued Victim", 20);
            if (showDebugLogs)
            {
                Debug.Log($"[PlayerInteract] Server: Awarded 20 points for rescuing victim");
            }
        }

        // Increment saved victims count in GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.IncrementSavedVictims();
        }

        // Despawn and destroy the victim NetworkObject
        netObj.Despawn(true);

        if (showDebugLogs)
        {
            Debug.Log($"[PlayerInteract] Server: Victim rescued and despawned");
        }
    }

    /// <summary>
    /// Server RPC to use medkit on a victim
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void RequestMedkitUseServerRpc(ulong networkObjectId, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        if (showDebugLogs)
        {
            Debug.Log($"[PlayerInteract] Server: Processing medkit use request from client {clientId} for NetworkObjectId: {networkObjectId}");
        }

        // Validate GameManager exists
        if (GameManager.Instance == null)
        {
            Debug.LogError("[PlayerInteract] Server: GameManager not found!");
            NotifyMedkitUseFailedClientRpc(new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = new[] { clientId } }
            });
            return;
        }

        // Check if player has medkits
        if (GameManager.Instance.CurrentMedkits <= 0)
        {
            if (showDebugLogs)
            {
                Debug.LogWarning($"[PlayerInteract] Server: Client {clientId} has no medkits available");
            }
            NotifyMedkitUseFailedClientRpc(new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = new[] { clientId } }
            });
            return;
        }

        // Use medkit
        bool success = GameManager.Instance.UseMedkit();
        if (!success)
        {
            NotifyMedkitUseFailedClientRpc(new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = new[] { clientId } }
            });
            return;
        }

        // Find the NetworkObject
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject netObj))
        {
            Debug.LogWarning($"[PlayerInteract] Server: NetworkObject {networkObjectId} not found!");
            return;
        }

        // Award points on server
        if (PointManager.Instance != null)
        {
            PointManager.Instance.AddPoints("Healed Victim", 10);
            if (showDebugLogs)
            {
                Debug.Log($"[PlayerInteract] Server: Awarded 10 points for healing victim");
            }
        }

        // Despawn the healed victim
        netObj.Despawn(true);

        if (showDebugLogs)
        {
            Debug.Log($"[PlayerInteract] Server: Medkit used successfully, victim healed and despawned");
        }
    }

    /// <summary>
    /// Server RPC to clear rubble
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void RequestRubbleClearServerRpc(ulong networkObjectId)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[PlayerInteract] Server: Processing rubble clear request for NetworkObjectId: {networkObjectId}");
        }

        // Find the NetworkObject
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject netObj))
        {
            Debug.LogWarning($"[PlayerInteract] Server: NetworkObject {networkObjectId} not found!");
            return;
        }

        // Verify it's a RubbleInteractable
        RubbleInteractable rubble = netObj.GetComponent<RubbleInteractable>();
        if (rubble == null)
        {
            Debug.LogWarning($"[PlayerInteract] Server: Object {networkObjectId} is not a RubbleInteractable!");
            return;
        }

        // Award points on server
        if (PointManager.Instance != null)
        {
            PointManager.Instance.AddPoints("Cleared Rubble", 10);
            if (showDebugLogs)
            {
                Debug.Log($"[PlayerInteract] Server: Awarded 10 points for clearing rubble");
            }
        }

        // Despawn and destroy the rubble NetworkObject
        netObj.Despawn(true);

        if (showDebugLogs)
        {
            Debug.Log($"[PlayerInteract] Server: Rubble cleared and despawned");
        }
    }

    /// <summary>
    /// Generic server RPC for other interactable types
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void RequestGenericInteractionServerRpc(ulong networkObjectId)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[PlayerInteract] Server: Processing generic interaction for NetworkObjectId: {networkObjectId}");
        }

        // Find the NetworkObject
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject netObj))
        {
            Debug.LogWarning($"[PlayerInteract] Server: NetworkObject {networkObjectId} not found!");
            return;
        }

        // Try to get IInteractable component
        IInteractable interactable = netObj.GetComponent<IInteractable>();
        if (interactable != null)
        {
            // Call interact on server
            interactable.Interact(transform);
        }
    }

    #endregion

    #region Client RPCs

    /// <summary>
    /// Notifies a specific client that medkit use failed
    /// </summary>
    [ClientRpc]
    private void NotifyMedkitUseFailedClientRpc(ClientRpcParams clientRpcParams = default)
    {
        Debug.Log("[PlayerInteract] Medkit use failed! You don't have any medkits.");
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TriggerBlinkEffect();
        }
    }

    #endregion

    public IInteractable GetInteractableObject()
    {
        List<IInteractable> interactableList = new List<IInteractable>();
        Collider[] colliderArray = Physics.OverlapSphere(transform.position, interactRange);
        
        if (showDebugLogs && colliderArray.Length > 0)
        {
            Debug.Log($"[PlayerInteract] Found {colliderArray.Length} colliders in range");
        }

        foreach (Collider collider in colliderArray)
        {
            if (collider.TryGetComponent(out IInteractable interactable))
            {
                interactableList.Add(interactable);
                if (showDebugLogs)
                {
                    Debug.Log($"[PlayerInteract] Found IInteractable on {collider.gameObject.name}");
                }
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

    /// <summary>
    /// Get the current hold progress (0 to 1)
    /// </summary>
    public float GetHoldProgress()
    {
        return holdDuration > 0 ? Mathf.Clamp01(holdTimer / holdDuration) : 0f;
    }

    /// <summary>
    /// Check if currently holding the interact key
    /// </summary>
    public bool IsHolding()
    {
        return isHolding;
    }
}
