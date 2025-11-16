using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class SceneLoader : MonoBehaviour
{
    [Header("Multiplayer Settings")]
    [SerializeField] private bool useNetworkSceneLoading = true;

    /// <summary>
    /// Call this from a UI Button, passing the scene name
    /// Works for both single-player and multiplayer modes
    /// </summary>
    public void LoadSceneByName(string sceneName)
    {
        // Check if we should use network scene loading
        if (useNetworkSceneLoading && NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            // We're in a networked session
            if (NetworkManager.Singleton.IsServer)
            {
                // Only the server/host can load scenes in multiplayer
                Debug.Log($"[SceneLoader] Server loading scene: {sceneName}");
                NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
            }
            else
            {
                Debug.LogWarning("[SceneLoader] Only the host can change scenes in multiplayer mode!");
            }
        }
        else
        {
            // Single-player mode or network not active
            Debug.Log($"[SceneLoader] Loading scene (single-player): {sceneName}");
            SceneManager.LoadScene(sceneName);
        }
    }

    /// <summary>
    /// Load scene additively (loads scene without unloading current scene)
    /// </summary>
    public void LoadSceneAdditive(string sceneName)
    {
        if (useNetworkSceneLoading && NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                Debug.Log($"[SceneLoader] Server loading scene additively: {sceneName}");
                NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
            }
            else
            {
                Debug.LogWarning("[SceneLoader] Only the host can load scenes in multiplayer mode!");
            }
        }
        else
        {
            Debug.Log($"[SceneLoader] Loading scene additively (single-player): {sceneName}");
            SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        }
    }

    /// <summary>
    /// Force single-player scene loading (bypasses network)
    /// </summary>
    public void LoadSceneSinglePlayer(string sceneName)
    {
        Debug.Log($"[SceneLoader] Force loading scene (single-player mode): {sceneName}");
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Load the lobby scene and disconnect from network if connected
    /// </summary>
    public void ReturnToLobby()
    {
        // Disconnect from network if connected
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            Debug.Log("[SceneLoader] Disconnecting from network session...");
            NetworkManager.Singleton.Shutdown();
        }

        // Load lobby scene
        SceneManager.LoadScene("Lobby");
    }

    /// <summary>
    /// Return to main menu and clean up network
    /// </summary>
    public void ReturnToMainMenu()
    {
        if (NetworkConnectionManager.Instance != null)
        {
            NetworkConnectionManager.Instance.ReturnToMainMenu();
        }
        else
        {
            // Fallback if NetworkConnectionManager not available
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            {
                NetworkManager.Singleton.Shutdown();
            }
            SceneManager.LoadScene("MainMenu");
        }
    }
}
