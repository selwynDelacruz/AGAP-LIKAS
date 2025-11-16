using UnityEngine;

public class DisasterSceneManager : MonoBehaviour
{
    [Header("Disaster GameObjects")]
    [Tooltip("The Flood Game GameObject in the scene")]
    [SerializeField] private GameObject floodGameObject;

    [Tooltip("The Earthquake Game GameObject in the scene")]
    [SerializeField] private GameObject earthquakeGameObject;

    void Start()
    {
        // Get the selected disaster from PlayerPrefs
        string selectedDisaster = PlayerPrefs.GetString("DisasterType", "Flood");

        // Enable/Disable GameObjects based on selection
        switch (selectedDisaster)
        {
            case "Flood":
                EnableFloodMode();
                break;
            case "Earthquake":
                EnableEarthquakeMode();
                break;
            case "TestKen":
                // For TestKen, you can choose to enable both or handle differently
                // Currently enabling both for testing purposes
                EnableBothModes();
                break;
            default:
                Debug.LogWarning($"Unknown disaster type: {selectedDisaster}. Defaulting to Flood.");
                EnableFloodMode();
                break;
        }
    }

    private void EnableFloodMode()
    {
        if (floodGameObject != null)
        {
            floodGameObject.SetActive(true);
            Debug.Log("Flood Game enabled");
        }
        else
        {
            Debug.LogError("Flood Game GameObject is not assigned!");
        }

        if (earthquakeGameObject != null)
        {
            earthquakeGameObject.SetActive(false);
            Debug.Log("Earthquake Game disabled");
        }
    }

    private void EnableEarthquakeMode()
    {
        if (earthquakeGameObject != null)
        {
            earthquakeGameObject.SetActive(true);
            Debug.Log("Earthquake Game enabled");
        }
        else
        {
            Debug.LogError("Earthquake Game GameObject is not assigned!");
        }

        if (floodGameObject != null)
        {
            floodGameObject.SetActive(false);
            Debug.Log("Flood Game disabled");
        }
    }

    private void EnableBothModes()
    {
        // For TestKen mode, enable both for testing
        if (floodGameObject != null)
        {
            floodGameObject.SetActive(true);
        }

        if (earthquakeGameObject != null)
        {
            earthquakeGameObject.SetActive(true);
        }

        Debug.Log("TestKen mode: Both disaster GameObjects enabled");
    }
}