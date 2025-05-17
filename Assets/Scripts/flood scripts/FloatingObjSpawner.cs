using UnityEngine;

public class FloatingObjSpawner : MonoBehaviour
{
    public GameObject[] floatingObjects; // Array of floating object prefabs
    public Transform[] spawnPoints; // Array of spawn points

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            int randomIndex = Random.Range(0, floatingObjects.Length); //store a random number from 0 to the length of the array
            Vector3 randomSpawnPosition = new Vector3(Random.Range(-10, 11), 10, Random.Range(-10, 11)); // Random spawn point within a range

            // Instantiate the floating object at the random spawn point
            Instantiate(floatingObjects[randomIndex], randomSpawnPosition, Quaternion.identity);
        }
    }
}
