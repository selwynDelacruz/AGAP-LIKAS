using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages network connection state and transitions between scenes
/// Ensures NetworkManager persists and handles connection properly
/// </summary>
public class NetworkConnectionManager : MonoBehaviour
{
    public static NetworkConnectionManager Instance { get; private set; }

    [Header("Configuration")]
    [SerializeField] private bool dontDestroyOnLoad = true;
    
    [Header("Scene Management")]
    [SerializeField] private string lobbySceneName = "Lobby";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private void Awake()
    {
        // Singleton pattern
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
    }

    private void Start()
    {
        // Ensure NetworkManager exists
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager.Singleton is null! Make sure you have a NetworkManager in the scene.");
        }
        else
        {
            // Subscribe to connection events
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            NetworkManager.Singleton.OnServerStarted += OnServerStarted;
            
            Debug.Log("[NetworkConnectionManager] Initialized successfully");
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
        }
    }

    /// <summary>
    /// Start hosting a game
    /// </summary>
    public bool StartHost()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("Cannot start host - NetworkManager is null");
            return false;
        }

        Debug.Log("[NetworkConnectionManager] Starting host...");
        return NetworkManager.Singleton.StartHost();
    }

    /// <summary>
    /// Start as a client
    /// </summary>
    public bool StartClient()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("Cannot start client - NetworkManager is null");
            return false;
        }

        Debug.Log("[NetworkConnectionManager] Starting client...");
        return NetworkManager.Singleton.StartClient();
    }

    /// <summary>
    /// Start as a dedicated server
    /// </summary>
    public bool StartServer()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("Cannot start server - NetworkManager is null");
            return false;
        }

        Debug.Log("[NetworkConnectionManager] Starting server...");
        return NetworkManager.Singleton.StartServer();
    }

    /// <summary>
    /// Disconnect from the current session
    /// </summary>
    public void Disconnect()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogWarning("Cannot disconnect - NetworkManager is null");
            return;
        }

        Debug.Log("[NetworkConnectionManager] Disconnecting...");
        NetworkManager.Singleton.Shutdown();
    }

    /// <summary>
    /// Check if we're currently connected
    /// </summary>
    public bool IsConnected()
    {
        return NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient;
    }

    /// <summary>
    /// Check if we're the host
    /// </summary>
    public bool IsHost()
    {
        return NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost;
    }

    /// <summary>
    /// Check if we're a client
    /// </summary>
    public bool IsClient()
    {
        return NetworkManager.Singleton != null && NetworkManager.Singleton.IsClient;
    }

    /// <summary>
    /// Get the local client ID
    /// </summary>
    public ulong GetLocalClientId()
    {
        if (NetworkManager.Singleton != null)
        {
            return NetworkManager.Singleton.LocalClientId;
        }
        return 0;
    }

    /// <summary>
    /// Get the number of connected clients
    /// </summary>
    public int GetConnectedClientCount()
    {
        if (NetworkManager.Singleton != null)
        {
            return NetworkManager.Singleton.ConnectedClients.Count;
        }
        return 0;
    }

    /// <summary>
    /// Return to main menu and disconnect
    /// </summary>
    public void ReturnToMainMenu()
    {
        Debug.Log("[NetworkConnectionManager] Returning to main menu...");
        
        // Disconnect from network
        Disconnect();
        
        // Load main menu scene
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // Event callbacks
    private void OnServerStarted()
    {
        Debug.Log("[NetworkConnectionManager] Server started");
    }

    private void OnClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            Debug.Log($"[NetworkConnectionManager] Local client connected with ID: {clientId}");
        }
        else
        {
            Debug.Log($"[NetworkConnectionManager] Client {clientId} connected");
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.LocalClientId == clientId)
        {
            Debug.Log($"[NetworkConnectionManager] Local client disconnected");
            
            // If we're not in the main menu, return to it
            if (SceneManager.GetActiveScene().name != mainMenuSceneName)
            {
                SceneManager.LoadScene(mainMenuSceneName);
            }
        }
        else
        {
            Debug.Log($"[NetworkConnectionManager] Client {clientId} disconnected");
        }
    }

    /// <summary>
    /// Display connection information for debugging
    /// </summary>
    private void OnGUI()
    {
        if (!Application.isEditor) return; // Only show in editor

        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("=== Network Debug Info ===");
        
        if (NetworkManager.Singleton != null)
        {
            GUILayout.Label($"Connected: {IsConnected()}");
            GUILayout.Label($"Is Host: {IsHost()}");
            GUILayout.Label($"Is Client: {IsClient()}");
            GUILayout.Label($"Local Client ID: {GetLocalClientId()}");
            GUILayout.Label($"Connected Clients: {GetConnectedClientCount()}");
        }
        else
        {
            GUILayout.Label("NetworkManager: NULL");
        }
        
        GUILayout.EndArea();
    }
}
