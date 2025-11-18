using UnityEngine;

public class FloodMapSpawner : MonoBehaviour
{
    [Header("Assign your map PREFABS here")]
    public GameObject[] mapPrefabs;   // All 6 of your map prefabs

    [Header("Size of each map (adjust to your prefab size)")]
    public float mapSize = 121f;

    [Header("Safe Zone Prefab")]
    public GameObject safeZonePrefab;

    private GameObject[] selectedMaps = new GameObject[4];

    void Start()
    {
        // STEP 1: Shuffle the mapPrefabs array
        ShuffleArray(mapPrefabs);

        // STEP 2: Take the first 4 maps after shuffle
        for (int i = 0; i < 4; i++)
        {
            selectedMaps[i] = mapPrefabs[i];
        }

        // STEP 3: Spawn them in a 2x2 grid
        GameObject chunk0 = SpawnMap(selectedMaps[0], new Vector3(0, -3, 0));                     // bottom-right
        GameObject chunk1 = SpawnMap(selectedMaps[1], new Vector3(0, -3, mapSize));               // top-right
        GameObject chunk2 = SpawnMap(selectedMaps[2], new Vector3(mapSize, -3, 0));               // bottom-left
        GameObject chunk3 = SpawnMap(selectedMaps[3], new Vector3(mapSize, -3, mapSize));         // top-left

        // STEP 4: Place the safe zone inside the final chunk
        PlaceSafeZone(chunk3);
    }

    GameObject SpawnMap(GameObject prefab, Vector3 position)
    {
        return Instantiate(prefab, position, Quaternion.identity);
    }

    void PlaceSafeZone(GameObject mapChunk)
    {
        // find the ExitPoint in the mapChunk
        Transform exitPoint = mapChunk.transform.Find("ExitPoint");

        if (exitPoint == null)
        {
            Debug.LogWarning("Chunk " + mapChunk.name + " does not have an ExitPoint! Add one in the prefab.");
            return;
        }
            Instantiate(safeZonePrefab, exitPoint.position, exitPoint.rotation);
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
