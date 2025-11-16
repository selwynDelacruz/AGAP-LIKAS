using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Manages network state persistence across scenes
/// Ensures NetworkManager and lobby state persist during scene transitions
/// </summary>
public class NetworkSceneManager : MonoBehaviour
{
    public static NetworkSceneManager Instance { get; private set; }

    [Header("Scene Settings")]
    [SerializeField] private bool dontDestroyOnLoad = true;

    [Header("Auto-Setup")]
    [SerializeField] private bool autoSetupOnAwake = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (dontDestroyOnLoad)
        {
            DontDestroyOnLoad(gameObject);
        }

        if (autoSetupOnAwake)
        {
            SetupNetworkPersistence();
        }
    }

    /// <summary>
    /// Ensures NetworkManager and NetworkLobbyManager persist across scenes
    /// </summary>
    public void SetupNetworkPersistence()
    {
        // Ensure NetworkManager persists
        if (NetworkManager.Singleton != null)
        {
            DontDestroyOnLoad(NetworkManager.Singleton.gameObject);
            Debug.Log("NetworkManager set to persist across scenes");
        }

        // Ensure NetworkLobbyManager persists
        if (NetworkLobbyManager.Instance != null)
        {
            DontDestroyOnLoad(NetworkLobbyManager.Instance.gameObject);
            Debug.Log("NetworkLobbyManager set to persist across scenes");
        }
    }

    /// <summary>
    /// Loads a scene using NetworkManager's scene management
    /// This ensures all clients load the same scene
    /// </summary>
    public void LoadNetworkScene(string sceneName)
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager not found! Cannot load network scene.");
            return;
        }

        if (!NetworkManager.Singleton.IsServer)
        {
            Debug.LogWarning("Only the server/host can load network scenes!");
            return;
        }

        // Use NetworkManager's scene management for synchronized loading
        var status = NetworkManager.Singleton.SceneManager.LoadScene(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
        
        if (status != SceneEventProgressStatus.Started)
        {
            Debug.LogError($"Failed to start loading scene: {sceneName}");
        }
        else
        {
            Debug.Log($"Loading network scene: {sceneName}");
        }
    }

    /// <summary>
    /// Checks if we're currently in a networked session
    /// </summary>
    public bool IsInNetworkSession()
    {
        return NetworkManager.Singleton != null && 
               NetworkManager.Singleton.IsListening;
    }

    /// <summary>
    /// Gets the current network role
    /// </summary>
    public string GetNetworkRole()
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
        {
            return "Not Connected";
        }

        if (NetworkManager.Singleton.IsHost)
        {
            return "Host";
        }
        else if (NetworkManager.Singleton.IsServer)
        {
            return "Server";
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            return "Client";
        }

        return "Unknown";
    }

    /// <summary>
    /// Safely shutdown the network and return to lobby
    /// </summary>
    public void ReturnToLobby(string lobbySceneName = "Lobby")
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
        }

        // Clear lobby data
        if (NetworkLobbyManager.Instance != null)
        {
            NetworkLobbyManager.Instance.LeaveLobby();
        }

        // Load lobby scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(lobbySceneName);
    }

    /// <summary>
    /// Gets information about the current network session
    /// </summary>
    public NetworkSessionInfo GetSessionInfo()
    {
        NetworkSessionInfo info = new NetworkSessionInfo();

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            info.isConnected = true;
            info.isHost = NetworkManager.Singleton.IsHost;
            info.isServer = NetworkManager.Singleton.IsServer;
            info.isClient = NetworkManager.Singleton.IsClient;
            info.playerCount = (int)NetworkManager.Singleton.ConnectedClients.Count;
            info.localClientId = NetworkManager.Singleton.LocalClientId;
        }

        if (NetworkLobbyManager.Instance != null)
        {
            info.lobbyCode = NetworkLobbyManager.Instance.GetCurrentLobbyCode();
        }

        return info;
    }

    [System.Serializable]
    public class NetworkSessionInfo
    {
        public bool isConnected = false;
        public bool isHost = false;
        public bool isServer = false;
        public bool isClient = false;
        public int playerCount = 0;
        public ulong localClientId = 0;
        public string lobbyCode = "";

        public override string ToString()
        {
            if (!isConnected)
                return "Not connected to network";

            string role = isHost ? "Host" : isServer ? "Server" : "Client";
            return $"Role: {role} | Players: {playerCount} | Client ID: {localClientId} | Lobby: {lobbyCode}";
        }
    }
}
