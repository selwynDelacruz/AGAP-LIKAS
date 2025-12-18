using UnityEngine;
using Unity.Netcode;

public class BreakObject : NetworkBehaviour
{
    [Header("Prefab Settings")]
    [Tooltip("The broken/destroyed version of this object (must have NetworkObject component and be in Network Prefabs list)")]
    public GameObject breakedObjectPrefab;

    [Header("Collapse Settings")]
    [Tooltip("Downward force applied to simulate gravity-driven collapse")]
    public float downwardForce = 50f;
    
    [Tooltip("Random horizontal spread (lower = more vertical collapse)")]
    public float horizontalSpread = 2f;
    
    [Tooltip("Slight rotation force for realistic tumbling")]
    public float torqueAmount = 10f;

    [Header("Optional Effects")]
    [Tooltip("Delay before pieces start falling (seconds)")]
    public float collapseDelay = 0.1f;

    [Header("Camera Shake")]
    [Tooltip("Reference to the EarthquakeManager for camera shake")]
    public EarthquakeManager earthquakeManager;

    [Tooltip("Trigger camera shake when object breaks")]
    public bool triggerCameraShake = true;

    [Tooltip("Custom shake duration (overrides EarthquakeManager default if > 0)")]
    public float customShakeDuration = 0f;

    private BoxCollider triggerCollider;
    private bool hasTriggered = false;

    void Start()
    {
        // Get the trigger collider on this parent object
        triggerCollider = GetComponent<BoxCollider>();
        
        if (triggerCollider == null)
        {
            Debug.LogError($"BreakObject on {gameObject.name}: No BoxCollider found!", this);
        }
        else
        {
            triggerCollider.isTrigger = true;
        }

        if (breakedObjectPrefab == null)
        {
            Debug.LogError($"BreakObject on {gameObject.name}: No breakedObjectPrefab assigned!", this);
        }

        // Try to find EarthquakeManager if not assigned
        if (earthquakeManager == null && triggerCameraShake)
        {
            earthquakeManager = FindFirstObjectByType<EarthquakeManager>();
            
            if (earthquakeManager == null)
            {
                Debug.LogWarning($"BreakObject on {gameObject.name}: No EarthquakeManager found in scene. Camera shake will be disabled.");
                triggerCameraShake = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            
            // Trigger camera shake immediately when player enters (local effect)
            if (triggerCameraShake && earthquakeManager != null)
            {
                if (customShakeDuration > 0)
                {
                    earthquakeManager.TriggerEarthquake(customShakeDuration);
                }
                else
                {
                    earthquakeManager.TriggerEarthquake();
                }
            }
            
            // Only server handles the actual break/spawn
            if (IsServer)
            {
                if (collapseDelay > 0)
                {
                    Invoke(nameof(BreakAndCollapseNetworked), collapseDelay);
                }
                else
                {
                    BreakAndCollapseNetworked();
                }
            }
            else
            {
                // Client requests server to break this object
                RequestBreakServerRpc();
            }
        }
    }

    /// <summary>
    /// Client requests server to break this object
    /// </summary>
    [Rpc(SendTo.Server, RequireOwnership = false)]
    private void RequestBreakServerRpc()
    {
        // Server might have already triggered from its own collision
        if (hasTriggered) 
        {
            Debug.Log($"[BreakObject] Server: Break already triggered for {gameObject.name}");
            return;
        }
        
        hasTriggered = true;
        Debug.Log($"[BreakObject] Server received break request for {gameObject.name}");

        if (collapseDelay > 0)
        {
            Invoke(nameof(BreakAndCollapseNetworked), collapseDelay);
        }
        else
        {
            BreakAndCollapseNetworked();
        }
    }

    /// <summary>
    /// Spawns the broken prefab on the network (Server only)
    /// </summary>
    private void BreakAndCollapseNetworked()
    {
        if (!IsServer)
        {
            Debug.LogWarning("[BreakObject] BreakAndCollapseNetworked should only run on server!");
            return;
        }

        if (breakedObjectPrefab == null)
        {
            Debug.LogError($"[BreakObject] Cannot break {gameObject.name}: breakedObjectPrefab is not assigned!");
            return;
        }

        // Check if prefab has NetworkObject component
        NetworkObject prefabNetObj = breakedObjectPrefab.GetComponent<NetworkObject>();
        if (prefabNetObj == null)
        {
            Debug.LogError($"[BreakObject] breakedObjectPrefab '{breakedObjectPrefab.name}' is missing NetworkObject component! " +
                "Add NetworkObject to the prefab and add it to NetworkManager's Network Prefabs list.");
            // Fallback to local-only (won't work for multiplayer interaction)
            BreakAndCollapseLocal();
            return;
        }

        // Store position and rotation before destroying
        Vector3 spawnPosition = transform.position;
        Quaternion spawnRotation = transform.rotation;

        Debug.Log($"[BreakObject] Server spawning networked rubble at {spawnPosition}");

        // Instantiate the broken version
        GameObject brokenObject = Instantiate(breakedObjectPrefab, spawnPosition, spawnRotation);

        // Get NetworkObject and spawn on network
        NetworkObject netObj = brokenObject.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            try
            {
                // Spawn on network - this makes it visible to all clients
                netObj.Spawn(true); // true = destroy with scene
                
                Debug.Log($"[BreakObject] ? Server spawned networked rubble '{breakedObjectPrefab.name}' " +
                    $"(NetworkObjectId: {netObj.NetworkObjectId}, IsSpawned: {netObj.IsSpawned})");

                // Apply physics forces locally on server
                ApplyCollapseForces(brokenObject);
                
                // Notify clients to apply forces and trigger effects
                NotifyCollapseClientRpc(netObj.NetworkObjectId);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[BreakObject] ? Failed to network spawn rubble: {e.Message}\n" +
                    "Make sure the prefab is added to NetworkManager's Network Prefabs list!");
                Destroy(brokenObject);
            }
        }

        // Destroy the original intact building
        Debug.Log($"[BreakObject] Destroying original object {gameObject.name}");
        
        // If this object is networked, despawn it properly
        NetworkObject originalNetObj = GetComponent<NetworkObject>();
        if (originalNetObj != null && originalNetObj.IsSpawned)
        {
            originalNetObj.Despawn(true);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Fallback for non-networked rubble (won't work with networked interaction)
    /// </summary>
    private void BreakAndCollapseLocal()
    {
        Debug.LogWarning($"[BreakObject] Using local-only collapse for {gameObject.name}. Rubble won't be interactable in multiplayer!");
        
        Vector3 spawnPosition = transform.position;
        Quaternion spawnRotation = transform.rotation;

        GameObject brokenObject = Instantiate(breakedObjectPrefab, spawnPosition, spawnRotation);
        ApplyCollapseForces(brokenObject);

        Destroy(gameObject);
    }

    /// <summary>
    /// Apply collapse physics forces to rubble pieces
    /// </summary>
    private void ApplyCollapseForces(GameObject brokenObject)
    {
        Rigidbody[] rubblePieces = brokenObject.GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody rb in rubblePieces)
        {
            if (rb != null)
            {
                rb.isKinematic = false;

                Vector3 collapseForce = new Vector3(
                    Random.Range(-horizontalSpread, horizontalSpread),
                    -downwardForce,
                    Random.Range(-horizontalSpread, horizontalSpread)
                );

                rb.AddForce(collapseForce, ForceMode.Impulse);

                Vector3 randomTorque = new Vector3(
                    Random.Range(-torqueAmount, torqueAmount),
                    Random.Range(-torqueAmount, torqueAmount),
                    Random.Range(-torqueAmount, torqueAmount)
                );

                rb.AddTorque(randomTorque, ForceMode.Impulse);
            }
        }

        Debug.Log($"[BreakObject] Applied forces to {rubblePieces.Length} rubble pieces");
    }

    /// <summary>
    /// Notify all clients about the collapse (for effects like camera shake)
    /// </summary>
    [Rpc(SendTo.ClientsAndHost)]
    private void NotifyCollapseClientRpc(ulong rubbleNetworkObjectId)
    {
        Debug.Log($"[BreakObject] Client received collapse notification. Rubble NetworkObjectId: {rubbleNetworkObjectId}");

        // Trigger camera shake on all clients
        if (triggerCameraShake)
        {
            if (earthquakeManager == null)
            {
                earthquakeManager = FindFirstObjectByType<EarthquakeManager>();
            }

            if (earthquakeManager != null)
            {
                if (customShakeDuration > 0)
                {
                    earthquakeManager.TriggerEarthquake(customShakeDuration);
                }
                else
                {
                    earthquakeManager.TriggerEarthquake();
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawCube(transform.position, new Vector3(horizontalSpread * 2, 0.5f, horizontalSpread * 2));
        
        Gizmos.color = Color.red;
        Vector3 arrowStart = transform.position;
        Vector3 arrowEnd = transform.position + Vector3.down * 2f;
        Gizmos.DrawLine(arrowStart, arrowEnd);
    }
}
