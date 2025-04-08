using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private string selectedDisasterScene = ""; // Stores the next scene to load
    public void Flood()
    {
        selectedDisasterScene = "Flood"; // Change to your actual scene name
        Debug.Log("Player chose the Flood disaster mode");
    }

    public void Earthquake()
    {
        selectedDisasterScene = "Earthquake"; // Change to your actual scene name
        Debug.Log($"{selectedDisasterScene}");
        Debug.Log("Player chose the Earthquake disaster mode");
    }

    public void StartGame()
    {
        if (!string.IsNullOrEmpty(selectedDisasterScene))
        {
            SceneManager.LoadScene(selectedDisasterScene);
        }
        else
        {
            Debug.LogWarning("No disaster mode selected! Choose Flood or Earthquake first.");
        }
    }

    public void QuitGame()
    {
        Debug.Log("Quit button pressed. Exiting application...");
        Application.Quit();

    }
}
