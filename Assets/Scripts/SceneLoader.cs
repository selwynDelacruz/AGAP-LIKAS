using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class SceneLoader : MonoBehaviour
{
    //Call this from a UI Button, passing the scene name
    public void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // Call this from "Return to Main Menu" button in Result Scene
    public void ResetAndReturnToMainMenu()
    {
        // Shutdown NetworkManager to ensure clean state for next lobby
        ShutdownNetwork();

        // Reset points before returning to main menu
        if (PointManager.Instance != null)
        {
            PointManager.Instance.ResetPoints();
            Debug.Log("Points reset. Returning to Main Menu.");
        }

        // Clear lobby-related PlayerPrefs
        PlayerPrefs.DeleteKey("LobbyCode");
        PlayerPrefs.DeleteKey("CameFromLobby");
        PlayerPrefs.Save();

        // Load the main menu scene (adjust scene name if different)
        SceneManager.LoadScene("Main Menu");
    }

    /// <summary>
    /// Shuts down the NetworkManager to ensure clean state for creating/joining new lobbies
    /// </summary>
    private void ShutdownNetwork()
    {
        if (NetworkManager.Singleton != null)
        {
            // Check if network is active
            if (NetworkManager.Singleton.IsListening)
            {
                Debug.Log("[SceneLoader] Shutting down NetworkManager before returning to Main Menu");
                NetworkManager.Singleton.Shutdown();
            }
        }
    }
}
