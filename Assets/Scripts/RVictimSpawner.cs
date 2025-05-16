using System.Collections.Generic;
using UnityEngine;
using TMPro; // Add this for TMP_Text

public class RVictimSpawner : MonoBehaviour
{
    [Header("Prefabs to Spawn")]
    [SerializeField] private List<GameObject> victimPrefabs = new List<GameObject>();

    [Header("Spawn Points")]
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();

    [Header("UI")]
    [SerializeField] private TMP_Text spawnedCountText; // Reference to TMP text

    // Internal tracking of used spawn points
    private HashSet<Transform> usedSpawnPoints = new HashSet<Transform>();

    // Track number of spawned prefabs
    private int spawnedCount = 0;
    private const int maxSpawnCount = 5; // Maximum allowed spawns

    private void Start()
    {
        UpdateSpawnedCountText();
    }

    // Call this from TMP button OnClick
    public void SpawnVictim()
    {
        if (spawnedCount >= maxSpawnCount)
        {
            Debug.LogWarning("Spawn limit reached!");
            return;
        }

        // Get available spawn points
        List<Transform> availablePoints = new List<Transform>();
        foreach (var point in spawnPoints)
        {
            if (!usedSpawnPoints.Contains(point))
                availablePoints.Add(point);
        }

        if (availablePoints.Count == 0)
        {
            Debug.LogWarning("No available spawn points left!");
            return;
        }

        if (victimPrefabs.Count == 0)
        {
            Debug.LogWarning("No victim prefabs assigned!");
            return;
        }

        // Pick a random available spawn point
        int spawnIndex = Random.Range(0, availablePoints.Count);
        Transform chosenPoint = availablePoints[spawnIndex];

        // Pick a random prefab
        int prefabIndex = Random.Range(0, victimPrefabs.Count);
        GameObject chosenPrefab = victimPrefabs[prefabIndex];

        // Spawn the prefab
        Instantiate(chosenPrefab, chosenPoint.position, chosenPoint.rotation);

        // Mark this spawn point as used
        usedSpawnPoints.Add(chosenPoint);

        // Increment and update UI
        spawnedCount++;
        UpdateSpawnedCountText();
    }

    private void UpdateSpawnedCountText()
    {
        if (spawnedCountText != null)
        {
            if (spawnedCount >= maxSpawnCount)
            {
                spawnedCountText.text = $"Spawned: {spawnedCount} spawn limit reached!";
            }
            else
            {
                spawnedCountText.text = $"Spawned: {spawnedCount}";
            }
        }
    }
}
