using UnityEngine;

public class MapSpawner : MonoBehaviour
{
    [Header("Assign your map PREFABS here")]
    public GameObject[] mapPrefabs;   // All 6 of your map prefabs

    [Header("Size of each map (adjust to your prefab size)")]
    public float mapSize = 121f;

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
        SpawnMap(selectedMaps[0], new Vector3(0, 0, 0));                     // bottom-right
        SpawnMap(selectedMaps[1], new Vector3(0, 0, mapSize));               // top-right
        SpawnMap(selectedMaps[2], new Vector3(mapSize, 0, 0));               // bottom-left
        SpawnMap(selectedMaps[3], new Vector3(mapSize, 0, mapSize));         // top-left
    }

    void SpawnMap(GameObject prefab, Vector3 position)
    {
        Instantiate(prefab, position, Quaternion.identity);
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
