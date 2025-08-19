using UnityEngine;
using System.Collections.Generic;

public class VictimSpawner : MonoBehaviour
{
    public Transform[] spawnPoints; // Assign in inspector
    public GameObject[] victimPrefabs; // Prefabs named "optionA", "optionB", etc.

    void Start()
    {
        int maxQuestions = 3;
        List<int> availableSpawnIndices = new List<int>();
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            availableSpawnIndices.Add(i);
        }

        for (int i = 0; i < maxQuestions; i++)
        {
            // Get the dropdown option name for this question
            string optionName = PlayerPrefs.GetString($"DropdownOptionName_{i}", "");
            if (string.IsNullOrEmpty(optionName)) continue;

            // Find the prefab with the matching name
            GameObject prefab = null;
            foreach (var v in victimPrefabs)
            {
                if (v.name == optionName)
                {
                    prefab = v;
                    break;
                }
            }
            if (prefab == null) continue;

            // No more available spawn points
            if (availableSpawnIndices.Count == 0) break;

            // Pick a random available spawn point
            int randomIndex = Random.Range(0, availableSpawnIndices.Count);
            int spawnPointIndex = availableSpawnIndices[randomIndex];
            availableSpawnIndices.RemoveAt(randomIndex);

            // Spawn the victim at the chosen spawn point locally
            GameObject victim = Instantiate(prefab, spawnPoints[spawnPointIndex].position, spawnPoints[spawnPointIndex].rotation);
            victim.tag = "Victim";

            // Assign the question index
            VictimQuestion vq = victim.GetComponent<VictimQuestion>();
            if (vq != null)
            {
                vq.questionIndex = i;
            }
        }
    }
}
