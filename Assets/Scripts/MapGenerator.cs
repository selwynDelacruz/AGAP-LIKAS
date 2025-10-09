using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("All map prefabs")] 
    public GameObject[] mapPrefabs;   // Holds 6 map prefabs

    [Header("Number of maps to spawn")]
    public int mapsToSpawn = 4;

    [Header("Offset between maps")]
    public Vector3 mapOffset = new Vector3(0, 0, 100);  // distance between spawned chunks

    private List<int> chosenIndexes = new List<int>();

    void Start()
    {
        GenerateMap();
    }

    void GenerateMap()
    {
        // Make sure we don’t pick the same chunk twice
        List<int> availableIndexes = new List<int>();
        for (int i = 0; i < mapPrefabs.Length; i++)
        {
            availableIndexes.Add(i);
        }

        for (int i = 0; i < mapsToSpawn; i++)
        {
            // Pick a random index from the available pool
            int randomIndex = Random.Range(0, availableIndexes.Count);
            int chosenIndex = availableIndexes[randomIndex];

            // Instantiate the chunk at the correct position
            Vector3 spawnPosition = i * mapOffset;
            Instantiate(mapPrefabs[chosenIndex], spawnPosition, Quaternion.identity);

            // Remove it from pool to avoid duplicates
            availableIndexes.RemoveAt(randomIndex);
            chosenIndexes.Add(chosenIndex);
        }
    }
}
