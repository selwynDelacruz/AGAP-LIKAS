using UnityEngine;
using System.Collections.Generic;

public class VictimSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("Array of spawn point transforms where victims can be spawned")]
    public Transform[] spawnPoints;
    
    [Tooltip("Array of victim prefabs to randomly spawn")]
    public GameObject[] victimPrefabs;
    
    [Header("Spawn Behavior")]
    [Tooltip("If true, spawns victims on Start. If false, call SpawnVictims() manually")]
    [SerializeField] private bool spawnOnStart = true;
    
    [Tooltip("Prevent spawning multiple victims at the same spawn point")]
    [SerializeField] private bool preventDuplicateSpawnPoints = true;

    private List<GameObject> spawnedVictims = new List<GameObject>();
    private int taskCount = 0;

    private void Start()
    {
        // Get task count from PlayerPrefs (set by LobbyManager)
        taskCount = PlayerPrefs.GetInt("TaskCount", 1);
        
        Debug.Log($"[VictimSpawner] Retrieved task count: {taskCount}");

        // Validate configuration
        if (!ValidateConfiguration())
        {
            return;
        }

        // Spawn victims if enabled
        if (spawnOnStart)
        {
            SpawnVictims();
        }
    }

    /// <summary>
    /// Validates that the spawner is properly configured
    /// </summary>
    private bool ValidateConfiguration()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("[VictimSpawner] No spawn points assigned! Please assign spawn points in the inspector.");
            return false;
        }

        if (victimPrefabs == null || victimPrefabs.Length == 0)
        {
            Debug.LogError("[VictimSpawner] No victim prefabs assigned! Please assign victim prefabs in the inspector.");
            return false;
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
    /// Spawns victims based on the task count from LobbyManager
    /// </summary>
    public void SpawnVictims()
    {
        // Clear any previously spawned victims
        ClearSpawnedVictims();

        // Determine how many victims to spawn
        int victimsToSpawn = preventDuplicateSpawnPoints 
            ? Mathf.Min(taskCount, spawnPoints.Length) 
            : taskCount;

        Debug.Log($"[VictimSpawner] Spawning {victimsToSpawn} victims...");

        if (preventDuplicateSpawnPoints)
        {
            SpawnVictimsWithUniqueSpawnPoints(victimsToSpawn);
        }
        else
        {
            SpawnVictimsRandomly(victimsToSpawn);
        }

        Debug.Log($"[VictimSpawner] Successfully spawned {spawnedVictims.Count} victims");
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
    /// Spawns a random victim prefab at the specified spawn point
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

        // Optional: Parent the spawned victim to this spawner for organization
        spawnedVictim.transform.SetParent(transform);

        // Add to spawned list for tracking
        spawnedVictims.Add(spawnedVictim);

        Debug.Log($"[VictimSpawner] Spawned '{victimPrefab.name}' at spawn point {spawnPointIndex} ({spawnPoint.name})");
    }

    /// <summary>
    /// Clears all previously spawned victims
    /// </summary>
    public void ClearSpawnedVictims()
    {
        foreach (GameObject victim in spawnedVictims)
        {
            if (victim != null)
            {
                Destroy(victim);
            }
        }
        spawnedVictims.Clear();
        Debug.Log("[VictimSpawner] Cleared all spawned victims");
    }

    /// <summary>
    /// Gets the list of currently spawned victims
    /// </summary>
    public List<GameObject> GetSpawnedVictims()
    {
        return spawnedVictims;
    }

    /// <summary>
    /// Gets the number of victims currently spawned
    /// </summary>
    public int GetSpawnedVictimCount()
    {
        return spawnedVictims.Count;
    }

    /// <summary>
    /// Manually set task count and respawn victims
    /// </summary>
    public void SetTaskCountAndRespawn(int newTaskCount)
    {
        taskCount = newTaskCount;
        Debug.Log($"[VictimSpawner] Task count manually set to {taskCount}");
        SpawnVictims();
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
