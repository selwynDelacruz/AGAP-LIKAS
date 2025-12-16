using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;

public class VictimSpawner : NetworkBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("Array of victim prefabs to randomly spawn (must have NetworkObject component)")]
    public GameObject[] victimPrefabs;
    
    [Header("Spawn Behavior")]
    [Tooltip("If true, spawns victims on Start. If false, call SpawnVictims() manually")]
    [SerializeField] private bool spawnOnStart = true;
    
    [Tooltip("Prevent spawning multiple victims at the same spawn point")]
    [SerializeField] private bool preventDuplicateSpawnPoints = true;

    [Header("Spawn Point Names")]
    [Tooltip("Name of the spawn point container for Earthquake disaster")]
    [SerializeField] private string earthquakeSpawnPointName = "EarthquakeVictim";
    
    [Tooltip("Name of the spawn point container for Flood disaster")]
    [SerializeField] private string floodSpawnPointName = "FloodVictim";

    [Header("Timing")]
    [Tooltip("Delay in seconds before spawning victims (allows MapSpawner to complete)")]
    [SerializeField] private float spawnDelay = 1.5f;

    [Header("Debug")]
    [Tooltip("Enable detailed logging for debugging")]
    [SerializeField] private bool debugMode = true;

    // Synced random seed so all clients use same randomization
    private NetworkVariable<int> randomSeed = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private List<NetworkObject> spawnedVictims = new List<NetworkObject>();
    private int taskCount = 0;
    private Transform[] spawnPoints;

    // Track victim type counts
    private int medkitVictimCount = 0;
    private int rubbleCount = 0;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Get task count from PlayerPrefs (set by LobbyManager)
        taskCount = PlayerPrefs.GetInt("TaskCount", 5);
        
        if (debugMode)
        {
            Debug.Log($"[VictimSpawner] OnNetworkSpawn - IsServer: {IsServer}, TaskCount: {taskCount}");
        }

        // Only server spawns victims
        if (IsServer && spawnOnStart)
        {
            // Generate random seed for deterministic spawning
            randomSeed.Value = Random.Range(int.MinValue, int.MaxValue);
            StartCoroutine(DelayedSpawnVictims());
        }
    }

    /// <summary>
    /// Coroutine to spawn victims after a delay, ensuring maps are spawned first
    /// </summary>
    private IEnumerator DelayedSpawnVictims()
    {
        if (debugMode)
        {
            Debug.Log($"[VictimSpawner] Waiting {spawnDelay} seconds for maps to spawn...");
        }

        yield return new WaitForSeconds(spawnDelay);

        // Find spawn points based on disaster mode
        FindSpawnPointsBasedOnDisaster();

        // Validate configuration
        if (!ValidateConfiguration())
        {
            yield break;
        }

        // Spawn victims
        SpawnVictims();

        // Wait a frame to ensure all victims are spawned
        yield return null;

        // Count and save victim types
        CountAndSaveVictimTypes();
    }

    /// <summary>
    /// Finds spawn points in the scene based on the selected disaster mode
    /// </summary>
    private void FindSpawnPointsBasedOnDisaster()
    {
        // Get disaster type from PlayerPrefs (set by LobbyManager)
        string disasterType = PlayerPrefs.GetString("DisasterType", "Earthquake");
        
        string spawnPointContainerName = "";

        switch (disasterType)
        {
            case "Flood":
                spawnPointContainerName = floodSpawnPointName;
                if (debugMode)
                {
                    Debug.Log($"[VictimSpawner] Flood mode detected - searching for '{spawnPointContainerName}' container");
                }
                break;
            case "Earthquake":
                spawnPointContainerName = earthquakeSpawnPointName;
                if (debugMode)
                {
                    Debug.Log($"[VictimSpawner] Earthquake mode detected - searching for '{spawnPointContainerName}' container");
                }
                break;
            case "TestKen":
                spawnPointContainerName = earthquakeSpawnPointName;
                if (debugMode)
                {
                    Debug.Log($"[VictimSpawner] TestKen mode detected - using '{spawnPointContainerName}' container");
                }
                break;
            default:
                spawnPointContainerName = earthquakeSpawnPointName;
                Debug.LogWarning($"[VictimSpawner] Unknown disaster type: {disasterType}, defaulting to Earthquake");
                break;
        }

        // Find the spawn point container in the scene
        List<Transform> foundSpawnPoints = new List<Transform>();
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        foreach (GameObject obj in allObjects)
        {
            if (obj.name == spawnPointContainerName)
            {
                if (debugMode)
                {
                    Debug.Log($"[VictimSpawner] Found spawn container: {obj.name}");
                }

                // Get all direct children as spawn points
                foreach (Transform child in obj.transform)
                {
                    foundSpawnPoints.Add(child);
                    if (debugMode)
                    {
                        Debug.Log($"[VictimSpawner] Found spawn point: {child.name}");
                    }
                }
            }
        }

        spawnPoints = foundSpawnPoints.ToArray();

        if (debugMode)
        {
            Debug.Log($"[VictimSpawner] Total spawn points found: {spawnPoints.Length}");
        }
    }

    /// <summary>
    /// Validates that the spawner is properly configured
    /// </summary>
    private bool ValidateConfiguration()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("[VictimSpawner] No spawn points found in the scene! Make sure map prefabs have spawn point children.");
            return false;
        }

        if (victimPrefabs == null || victimPrefabs.Length == 0)
        {
            Debug.LogError("[VictimSpawner] No victim prefabs assigned! Please assign victim prefabs in the inspector.");
            return false;
        }

        // Validate prefabs have NetworkObject component
        foreach (var prefab in victimPrefabs)
        {
            if (prefab != null && prefab.GetComponent<NetworkObject>() == null)
            {
                Debug.LogError($"[VictimSpawner] Victim prefab '{prefab.name}' is missing NetworkObject component! Add it to the prefab.");
                return false;
            }
        }

        if (taskCount <= 0)
        {
            Debug.LogWarning("[VictimSpawner] Task count is 0 or negative. No victims will be spawned.");
            return false;
        }

        if (preventDuplicateSpawnPoints && taskCount > spawnPoints.Length)
        {
            Debug.LogWarning($"[VictimSpawner] Task count ({taskCount}) exceeds spawn points ({spawnPoints.Length}). " +
                           $"Only {spawnPoints.Length} victims will be spawned.");
        }

        return true;
    }

    /// <summary>
    /// Spawns victims based on the task count from PlayerPrefs (Server only)
    /// </summary>
    public void SpawnVictims()
    {
        if (!IsServer)
        {
            Debug.LogWarning("[VictimSpawner] SpawnVictims called on client - only server can spawn victims!");
            return;
        }

        // Clear any previously spawned victims
        ClearSpawnedVictims();

        // Reset counts
        medkitVictimCount = 0;
        rubbleCount = 0;

        // Use synced seed for deterministic randomization
        Random.State oldState = Random.state;
        Random.InitState(randomSeed.Value);

        // Determine how many victims to spawn
        int victimsToSpawn = preventDuplicateSpawnPoints 
            ? Mathf.Min(taskCount, spawnPoints.Length) 
            : taskCount;

        Debug.Log($"[VictimSpawner] Server spawning {victimsToSpawn} victims (seed: {randomSeed.Value})...");

        if (preventDuplicateSpawnPoints)
        {
            SpawnVictimsWithUniqueSpawnPoints(victimsToSpawn);
        }
        else
        {
            SpawnVictimsRandomly(victimsToSpawn);
        }

        // Restore random state
        Random.state = oldState;

        Debug.Log($"[VictimSpawner] Successfully spawned {spawnedVictims.Count} networked victims");
    }

    /// <summary>
    /// Spawns victims ensuring each spawn point is used only once
    /// </summary>
    private void SpawnVictimsWithUniqueSpawnPoints(int count)
    {
        // Create a list of available spawn point indices
        List<int> availableSpawnIndices = new List<int>();
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i] != null)
            {
                availableSpawnIndices.Add(i);
            }
        }

        // Spawn victims at unique spawn points
        for (int i = 0; i < count && availableSpawnIndices.Count > 0; i++)
        {
            // Pick a random spawn point from available ones
            int randomIndex = Random.Range(0, availableSpawnIndices.Count);
            int spawnPointIndex = availableSpawnIndices[randomIndex];
            
            // Remove this spawn point from available list
            availableSpawnIndices.RemoveAt(randomIndex);

            // Spawn victim at this point
            SpawnVictimAtPoint(spawnPointIndex);
        }
    }

    /// <summary>
    /// Spawns victims randomly, allowing duplicate spawn points
    /// </summary>
    private void SpawnVictimsRandomly(int count)
    {
        for (int i = 0; i < count; i++)
        {
            // Pick a random spawn point
            int spawnPointIndex = Random.Range(0, spawnPoints.Length);
            
            // Spawn victim at this point
            SpawnVictimAtPoint(spawnPointIndex);
        }
    }

    /// <summary>
    /// Spawns a random victim prefab at the specified spawn point (Server only, networked)
    /// </summary>
    private void SpawnVictimAtPoint(int spawnPointIndex)
    {
        Transform spawnPoint = spawnPoints[spawnPointIndex];
        
        if (spawnPoint == null)
        {
            Debug.LogWarning($"[VictimSpawner] Spawn point at index {spawnPointIndex} is null. Skipping...");
            return;
        }

        // Pick a random victim prefab
        int randomPrefabIndex = Random.Range(0, victimPrefabs.Length);
        GameObject victimPrefab = victimPrefabs[randomPrefabIndex];

        if (victimPrefab == null)
        {
            Debug.LogWarning($"[VictimSpawner] Victim prefab at index {randomPrefabIndex} is null. Skipping...");
            return;
        }

        // Instantiate the victim at the spawn point
        GameObject spawnedVictim = Instantiate(
            victimPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        // Get NetworkObject and spawn on network
        NetworkObject netObj = spawnedVictim.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            try
            {
                netObj.Spawn(true); // true = destroy with scene
                spawnedVictims.Add(netObj);

                if (debugMode)
                {
                    Debug.Log($"[VictimSpawner] ✓ Spawned networked '{victimPrefab.name}' at spawn point {spawnPointIndex} ({spawnPoint.name}) - NetworkObjectId: {netObj.NetworkObjectId}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[VictimSpawner] ✗ Failed to network spawn victim: {e.Message}");
                Destroy(spawnedVictim);
            }
        }
        else
        {
            Debug.LogError($"[VictimSpawner] Victim prefab '{victimPrefab.name}' missing NetworkObject component!");
            Destroy(spawnedVictim);
        }
    }

    /// <summary>
    /// Counts spawned victims by type and saves to PlayerPrefs (Server only)
    /// </summary>
    private void CountAndSaveVictimTypes()
    {
        if (!IsServer)
        {
            Debug.LogWarning("[VictimSpawner] CountAndSaveVictimTypes called on client - only server should call this!");
            return;
        }

        medkitVictimCount = 0;
        rubbleCount = 0;

        // Count victims with MedkitInteractable and rubble objects
        foreach (NetworkObject netObj in spawnedVictims)
        {
            if (netObj != null && netObj.gameObject != null)
            {
                // Check for MedkitInteractable component
                MedkitInteractable medkitComponent = netObj.GetComponent<MedkitInteractable>();
                if (medkitComponent != null)
                {
                    medkitVictimCount++;
                    if (debugMode)
                    {
                        Debug.Log($"[VictimSpawner] Found MedkitInteractable on: {netObj.gameObject.name}");
                    }
                }

                // Check for RubbleInteractable component
                RubbleInteractable rubbleComponent = netObj.GetComponent<RubbleInteractable>();
                if (rubbleComponent != null)
                {
                    rubbleCount++;
                    if (debugMode)
                    {
                        Debug.Log($"[VictimSpawner] Found RubbleInteractable on: {netObj.gameObject.name}");
                    }
                }
            }
        }

        // Save counts to PlayerPrefs for SimulationResultSummary to read
        PlayerPrefs.SetInt("MedkitVictimCount", medkitVictimCount);
        PlayerPrefs.SetInt("RubbleCount", rubbleCount);
        PlayerPrefs.Save();

        Debug.Log($"[VictimSpawner] ✓ Victim type counts saved - Medkit Victims: {medkitVictimCount}, Rubble: {rubbleCount}");

        // Sync to all clients via RPC
        SyncVictimCountsClientRpc(medkitVictimCount, rubbleCount);
    }

    /// <summary>
    /// Syncs victim counts to all clients
    /// </summary>
    [Rpc(SendTo.ClientsAndHost)]
    private void SyncVictimCountsClientRpc(int medkitCount, int rubble)
    {
        // Save to PlayerPrefs on all clients
        PlayerPrefs.SetInt("MedkitVictimCount", medkitCount);
        PlayerPrefs.SetInt("RubbleCount", rubble);
        PlayerPrefs.Save();

        if (debugMode)
        {
            Debug.Log($"[VictimSpawner] Client received victim counts - Medkit: {medkitCount}, Rubble: {rubble}");
        }
    }

    /// <summary>
    /// Clears all previously spawned victims (Server only)
    /// </summary>
    public void ClearSpawnedVictims()
    {
        if (!IsServer) return;

        foreach (NetworkObject victim in spawnedVictims)
        {
            if (victim != null && victim.IsSpawned)
            {
                victim.Despawn(true); // true = destroy
            }
        }
        spawnedVictims.Clear();
        
        if (debugMode)
        {
            Debug.Log("[VictimSpawner] Cleared all spawned victims");
        }
    }

    /// <summary>
    /// Gets the number of victims currently spawned
    /// </summary>
    public int GetSpawnedVictimCount()
    {
        return spawnedVictims.Count;
    }

    /// <summary>
    /// Gets the number of medkit victims spawned
    /// </summary>
    public int GetMedkitVictimCount()
    {
        return medkitVictimCount;
    }

    /// <summary>
    /// Gets the number of rubble objects spawned
    /// </summary>
    public int GetRubbleCount()
    {
        return rubbleCount;
    }

    /// <summary>
    /// Manually set task count and respawn victims (Server only)
    /// </summary>
    public void SetTaskCountAndRespawn(int newTaskCount)
    {
        if (!IsServer) return;

        taskCount = newTaskCount;
        Debug.Log($"[VictimSpawner] Task count manually set to {taskCount}");
        
        // Generate new seed for new spawn
        randomSeed.Value = Random.Range(int.MinValue, int.MaxValue);
        
        // Refresh spawn points and respawn
        FindSpawnPointsBasedOnDisaster();
        SpawnVictims();
        
        // Count and save victim types after respawning
        StartCoroutine(DelayedCountVictimTypes());
    }

    /// <summary>
    /// Coroutine to count victim types after a short delay
    /// </summary>
    private IEnumerator DelayedCountVictimTypes()
    {
        yield return null; // Wait one frame
        CountAndSaveVictimTypes();
    }

    /// <summary>
    /// Public method to refresh spawn points (useful if maps are spawned after VictimSpawner starts)
    /// </summary>
    public void RefreshSpawnPoints()
    {
        FindSpawnPointsBasedOnDisaster();
        Debug.Log($"[VictimSpawner] Spawn points refreshed. Found {spawnPoints.Length} spawn points.");
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        
        // Clean up spawned victims if we're the server
        if (IsServer)
        {
            ClearSpawnedVictims();
        }
    }

    // Visualize spawn points in the Scene view
    private void OnDrawGizmos()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
            return;

        Gizmos.color = Color.green;
        foreach (Transform spawnPoint in spawnPoints)
        {
            if (spawnPoint != null)
            {
                Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
                Gizmos.DrawLine(spawnPoint.position, spawnPoint.position + Vector3.up * 2f);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
            return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i] != null)
            {
                Gizmos.DrawWireSphere(spawnPoints[i].position, 0.7f);
            }
        }
    }
}
