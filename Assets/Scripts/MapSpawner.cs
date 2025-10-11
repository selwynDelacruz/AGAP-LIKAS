using UnityEngine;

public class MapSpawner : MonoBehaviour
{
    [Header("Assign your map PREFABS here")]
    public GameObject[] mapPrefabs;   // All 6 of your map prefabs

    [Header("Size of each map (adjust to your prefab size)")]
    public float mapSize = 182.4f;

    private GameObject[] selectedMaps = new GameObject[4];

    void Start()
    {
        // STEP 1: Randomly select 4 maps from the 6
        for (int i = 0; i < 4; i++)
        {
            int randomIndex = Random.Range(0, mapPrefabs.Length);
            selectedMaps[i] = mapPrefabs[randomIndex];
        }

        // STEP 2: Spawn them in a 2x2 grid
        SpawnMap(selectedMaps[0], new Vector3(0, 0, 0));                     // bottom-left
        SpawnMap(selectedMaps[1], new Vector3(0, 0, mapSize));               // top-left
        SpawnMap(selectedMaps[2], new Vector3(mapSize, 0, 0));               // bottom-right
        SpawnMap(selectedMaps[3], new Vector3(mapSize, 0, mapSize));         // top-right
    }

    void SpawnMap(GameObject prefab, Vector3 position)
    {
        Instantiate(prefab, position, Quaternion.identity);
    }
}
