using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

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

    // Synced random seed so all clients shuffle identically
    private NetworkVariable<int> randomSeed = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private GameObject[] selectedMaps = new GameObject[4];
    private bool hasSpawned = false;

    private string[] gameplayScenes = { "TestKen", "Flood", "Earthquake" };

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Only spawn maps in gameplay scenes
        string currentScene = SceneManager.GetActiveScene().name;
        if (System.Array.IndexOf(gameplayScenes, currentScene) < 0)
        {
            if (debugMode) Debug.Log($"[MapSpawner] Scene '{currentScene}' is not a gameplay scene. Skipping map spawn.");
            return;
        }

        if (IsServer)
        {
            // Server generates and sets seed
            randomSeed.Value = Random.Range(int.MinValue, int.MaxValue);
            if (debugMode) Debug.Log($"[MapSpawner] Server generated seed: {randomSeed.Value}");
            
            // Debug: Check if prefabs are registered
            if (debugMode)
            {
                Debug.Log($"[MapSpawner] Network Prefabs Count: {NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs.Count}");
            }
            
            SpawnMaps();
        }
        else
        {
            // Client logs that it's waiting for server to spawn maps
            if (debugMode) Debug.Log($"[MapSpawner] Client waiting for server to spawn maps (seed: {randomSeed.Value})");
            // Client doesn't spawn anything - just receives spawned NetworkObjects from server
        }
    }

    private void SpawnMaps()
    {
        if (hasSpawned)
        {
            if (debugMode) Debug.LogWarning("[MapSpawner] Already spawned maps!");
            return;
        }
        hasSpawned = true;

        // Validate prefabs are assigned
        if (mapPrefabs == null || mapPrefabs.Length < 4)
        {
            Debug.LogError("[MapSpawner] Not enough map prefabs assigned! Need at least 4 prefabs.");
            return;
        }

        // Use synced seed for deterministic shuffle
        Random.State oldState = Random.state;
        Random.InitState(randomSeed.Value);

        // Shuffle using the synced seed
        ShuffleArray(mapPrefabs);

        // Select first 4 maps after shuffle
        for (int i = 0; i < 4; i++)
        {
            selectedMaps[i] = mapPrefabs[i];
        }

        // Restore previous random state
        Random.state = oldState;

        if (debugMode)
        {
            Debug.Log($"[MapSpawner] Selected maps (seed {randomSeed.Value}): {selectedMaps[0].name}, {selectedMaps[1].name}, {selectedMaps[2].name}, {selectedMaps[3].name}");
        }

        // STEP 3: Spawn them in a 2x2 grid (only server does this)
        GameObject chunk0 = SpawnMap(selectedMaps[0], new Vector3(55, 0, 55));                         // bottom-left
        GameObject chunk1 = SpawnMap(selectedMaps[1], new Vector3(55, 0, 55 + mapSize));                // top-left
        GameObject chunk2 = SpawnMap(selectedMaps[2], new Vector3(55 + mapSize, 0, 55));               // bottom-right
        GameObject chunk3 = SpawnMap(selectedMaps[3], new Vector3(55 + mapSize, 0, 55 + mapSize));     // top-right

        // STEP 4: Place the safe zone inside the final chunk
        if (chunk3 != null)
        {
            PlaceSafeZone(chunk3);
        }

        if (debugMode)
        {
            Debug.Log("[MapSpawner] Server: Map spawning complete!");
        }
    }

    GameObject SpawnMap(GameObject prefab, Vector3 position)
    {
        if (prefab == null)
        {
            Debug.LogError("[MapSpawner] Trying to spawn null prefab!");
            return null;
        }

        if (prefab.GetComponent<NetworkObject>() == null)
        {
            Debug.LogError($"[MapSpawner] Prefab '{prefab.name}' MUST have NetworkObject component in the prefab asset!");
            return null;
        }

        // Check if prefab is registered in NetworkManager
        if (debugMode)
        {
            bool isRegistered = NetworkManager.Singleton.NetworkConfig.Prefabs.Contains(prefab);
            if (!isRegistered)
            {
                Debug.LogError($"[MapSpawner] Prefab '{prefab.name}' is NOT registered in NetworkManager's Network Prefabs List! Add it in NetworkManager inspector.");
                return null;
            }
        }

        // Instantiate the map locally first
        GameObject map = Instantiate(prefab, position, Quaternion.identity);
        
        // Get NetworkObject component
        NetworkObject networkObject = map.GetComponent<NetworkObject>();

        // Spawn on the network
        // true = destroy with scene (maps are cleaned up when scene changes)
        networkObject.Spawn(true);
        
        if (debugMode)
        {
            Debug.Log($"[MapSpawner] Server spawned: {prefab.name} at {position} (NetworkObjectId: {networkObject.NetworkObjectId})");
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

        // Check prefab has NetworkObject
        if (safeZonePrefab.GetComponent<NetworkObject>() == null)
        {
            Debug.LogError("[MapSpawner] SafeZone prefab MUST have NetworkObject component! Add it in the prefab editor.");
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

        // Spawn safe zone on network
        networkObject.Spawn(true);
        
        if (debugMode)
        {
            Debug.Log($"[MapSpawner] Server spawned networked safe zone at {exitPoint.position} (NetworkObjectId: {networkObject.NetworkObjectId})");
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
