using UnityEngine;

public class MapSpawner : MonoBehaviour
{
    [Header("Assign your map PREFABS here")]
    public GameObject[] mapPrefabs;   // All 6 of your map prefabs

    [Header("Size of each map (adjust to your prefab size)")]
    public float mapSize = 121f;

    [Header("Safe Zone Prefab")]
    public GameObject safeZonePrefab;

    [Header("Settings")]
    [Tooltip("Enable detailed logging for debugging")]
    public bool debugMode = true;

    private GameObject[] selectedMaps = new GameObject[4];

    private void Start()
    {
        if (debugMode)
        {
            Debug.Log("[MapSpawner] Starting map spawn process");
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
        GameObject chunk0 = SpawnMap(selectedMaps[0], new Vector3(55, 0, 55));                         // bottom-left
        GameObject chunk1 = SpawnMap(selectedMaps[3], new Vector3(55    , 0, 55 + mapSize));                // top-left
        GameObject chunk2 = SpawnMap(selectedMaps[2], new Vector3(55 + mapSize, 0, 55));               // bottom-right
        GameObject chunk3 = SpawnMap(selectedMaps[3], new Vector3(55 + mapSize, 0, 55 + mapSize));     // top-right

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

        // Instantiate the map
        GameObject map = Instantiate(prefab, position, Quaternion.identity);
        
        if (debugMode)
        {
            Debug.Log($"[MapSpawner] Spawned map: {prefab.name} at position {position}");
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
        
        if (debugMode)
        {
            Debug.Log($"[MapSpawner] Spawned safe zone at {exitPoint.position}");
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
