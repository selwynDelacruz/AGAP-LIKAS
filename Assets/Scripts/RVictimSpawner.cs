using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RVictimSpawner : MonoBehaviour
{
    [Header("Prefabs to Spawn")]
    [SerializeField] private List<GameObject> victimPrefabs = new List<GameObject>();

    [Header("Spawn Points")]
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();

    [Header("UI")]
    [SerializeField] private TMP_Text spawnedCountText;

    private HashSet<Transform> usedSpawnPoints = new HashSet<Transform>();
    private int spawnedCount = 0;
    private const int maxSpawnCount = 5;

    private void Start()
    {
        UpdateSpawnedCountText();
    }

    public void SpawnVictim()
    {
        if (spawnedCount >= maxSpawnCount)
        {
            Debug.LogWarning("Spawn limit reached!");
            return;
        }

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

        int spawnIndex = Random.Range(0, availablePoints.Count);
        Transform chosenPoint = availablePoints[spawnIndex];

        int prefabIndex = Random.Range(0, victimPrefabs.Count);
        GameObject chosenPrefab = victimPrefabs[prefabIndex];

        // Local spawn
        Instantiate(chosenPrefab, chosenPoint.position, chosenPoint.rotation);

        usedSpawnPoints.Add(chosenPoint);
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

