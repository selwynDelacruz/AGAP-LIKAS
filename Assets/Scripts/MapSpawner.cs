using UnityEngine;
using Unity.Netcode;

public class MapSpawner : NetworkBehaviour
{
    [Header("Assign your map PREFABS here")]
    public GameObject[] mapPrefabs;   // All 6 of your map prefabs

    [Header("Size of each map (adjust to your prefab size)")]
    public float mapSize = 121f;

    [Header("Safe Zone Prefab")]
    public GameObject safeZonePrefab;

    [Header("Network Settings")]
    [Tooltip("Enable detailed logging for debugging")]
    public bool debugMode = true;

    private GameObject[] selectedMaps = new GameObject[4];

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // ONLY THE HOST/SERVER SPAWNS THE MAPS
        // Clients will receive the spawned objects automatically via Netcode
        if (!IsServer)
        {
            if (debugMode)
            {
                Debug.Log("[MapSpawner] Client - waiting for server to spawn maps");
            }
            return;
        }

        if (debugMode)
        {
            Debug.Log("[MapSpawner] Server - starting map spawn process");
        }

        SpawnMaps();
    }

    private void SpawnMaps()
    {
        // Validate prefabs are assigned
        if (mapPrefabs == null || mapPrefabs.Length < 4)
        {
            Debug.LogError("[MapSpawner] Not enough map prefabs assigned! Need at least 4 prefabs.");
            return;
        }

        // STEP 1: Shuffle the mapPrefabs array
        ShuffleArray(mapPrefabs);

        // STEP 2: Take the first 4 maps after shuffle
        for (int i = 0; i < 4; i++)
        {
            selectedMaps[i] = mapPrefabs[i];
        }

        // STEP 3: Spawn them in a 2x2 grid
        GameObject chunk0 = SpawnMap(selectedMaps[0], new Vector3(0, 0, 0));                     // bottom-right
        GameObject chunk1 = SpawnMap(selectedMaps[1], new Vector3(0, 0, mapSize));               // top-right
        GameObject chunk2 = SpawnMap(selectedMaps[2], new Vector3(mapSize, 0, 0));               // bottom-left
        GameObject chunk3 = SpawnMap(selectedMaps[3], new Vector3(mapSize, 0, mapSize));         // top-left

        // STEP 4: Place the safe zone inside the final chunk
        if (chunk3 != null)
        {
            PlaceSafeZone(chunk3);
        }

        if (debugMode)
        {
            Debug.Log("[MapSpawner] Map spawning complete!");
        }
    }

    GameObject SpawnMap(GameObject prefab, Vector3 position)
    {
        if (prefab == null)
        {
            Debug.LogError("[MapSpawner] Trying to spawn null prefab!");
            return null;
        }

        // Instantiate the map locally first
        GameObject map = Instantiate(prefab, position, Quaternion.identity);
        
        // Get NetworkObject component
        NetworkObject networkObject = map.GetComponent<NetworkObject>();
        
        if (networkObject == null)
        {
            Debug.LogWarning($"[MapSpawner] Map prefab '{prefab.name}' is missing NetworkObject component! Adding one...");
            networkObject = map.AddComponent<NetworkObject>();
        }

        // Spawn on the network
        // true = destroy with scene (maps are cleaned up when scene changes)
        networkObject.Spawn(true);
        
        if (debugMode)
        {
            Debug.Log($"[MapSpawner] Spawned networked map: {prefab.name} at position {position}");
        }

        return map;
    }

    void PlaceSafeZone(GameObject mapChunk)
    {
        if (safeZonePrefab == null)
        {
            Debug.LogWarning("[MapSpawner] Safe zone prefab is not assigned!");
            return;
        }

        // Find the ExitPoint in the mapChunk
        Transform exitPoint = mapChunk.transform.Find("ExitPoint");

        if (exitPoint == null)
        {
            Debug.LogWarning($"[MapSpawner] Chunk {mapChunk.name} does not have an ExitPoint! Add one in the prefab.");
            Debug.LogWarning("[MapSpawner] Spawning safe zone at chunk center as fallback.");
            exitPoint = mapChunk.transform; // Use chunk transform as fallback
        }

        // Instantiate safe zone
        GameObject safeZone = Instantiate(safeZonePrefab, exitPoint.position, exitPoint.rotation);
        
        // Get NetworkObject component
        NetworkObject networkObject = safeZone.GetComponent<NetworkObject>();
        
        if (networkObject == null)
        {
            Debug.LogWarning("[MapSpawner] SafeZone prefab is missing NetworkObject component! Adding one...");
            networkObject = safeZone.AddComponent<NetworkObject>();
        }

        // Spawn safe zone on network
        networkObject.Spawn(true);
        
        if (debugMode)
        {
            Debug.Log($"[MapSpawner] Spawned networked safe zone at {exitPoint.position}");
        }
    }

    // Fisher-Yates shuffle algorithm
    void ShuffleArray(GameObject[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            GameObject temp = array[i];
            array[i] = array[randomIndex];
            array[randomIndex] = temp;
        }
    }
}
