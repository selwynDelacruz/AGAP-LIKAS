using UnityEngine;
using System.Collections.Generic;

public class ProceduralMapGenerator : MonoBehaviour
{
    [Header("Map Prefabs")]
    public GameObject[] mapPrefabs;  // Drag your 6 map prefabs here

    [Header("Grid Settings")]
    public int gridRows = 2;         // 2 rows
    public int gridCols = 2;         // 2 columns
    public float mapSize = 100f;     // Size of each map (adjust to your prefab’s size)

    private void Start()
    {
        GenerateMap();
    }

    void GenerateMap()
    {
        if (mapPrefabs.Length < gridRows * gridCols)
        {
            Debug.LogError("Not enough map prefabs to fill the grid!");
            return;
        }

        // Create a list of available maps
        List<GameObject> availableMaps = new List<GameObject>(mapPrefabs);

        for (int row = 0; row < gridRows; row++)
        {
            for (int col = 0; col < gridCols; col++)
            {
                // Pick a random map from the list
                int randomIndex = Random.Range(0, availableMaps.Count);
                GameObject chosenMap = availableMaps[randomIndex];

                // Calculate spawn position based on row & column
                Vector3 spawnPos = new Vector3(col * mapSize, 0, row * mapSize);

                // Optional random Y rotation for variety
                Quaternion randomRot = Quaternion.Euler(0, Random.Range(0, 4) * 90, 0);

                // Spawn map
                Instantiate(chosenMap, spawnPos, randomRot, transform);

                // Remove chosen map so it's not reused
                availableMaps.RemoveAt(randomIndex);
            }
        }
    }
}
