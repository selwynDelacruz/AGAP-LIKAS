using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the multiplayer lobby functionality integrating Unity Netcode with the existing LobbyManager
/// </summary>
public class NetworkLobbyManager : NetworkBehaviour
{
    public static NetworkLobbyManager Instance { get; private set; }

    [Header("Lobby Settings")]
    [SerializeField] private int maxPlayers = 4;
    
    [Header("Game Settings (Synced from Host)")]
    private NetworkVariable<int> networkTaskCount = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int> networkDuration = new NetworkVariable<int>(300, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
    // Using int to represent disaster type: 0 = Flood, 1 = Earthquake, 2 = TestKen
    private NetworkVariable<int> networkDisasterType = new NetworkVariable<int>(2, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Header("Connected Players")]
    private NetworkVariable<int> connectedPlayerCount = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // If we're the host/server, initialize lobby settings from PlayerPrefs
        if (IsServer)
        {
            InitializeLobbySettingsFromPlayerPrefs();
            UpdateConnectedPlayerCount();
        }

        // Subscribe to network variable changes
        networkTaskCount.OnValueChanged += OnTaskCountChanged;
        networkDuration.OnValueChanged += OnDurationChanged;
        networkDisasterType.OnValueChanged += OnDisasterTypeChanged;
        connectedPlayerCount.OnValueChanged += OnPlayerCountChanged;

        // Notify clients of current settings
        if (IsClient && !IsServer)
        {
            Debug.Log($"[Client] Received lobby settings - Tasks: {networkTaskCount.Value}, Duration: {networkDuration.Value}, Disaster: {GetDisasterName(networkDisasterType.Value)}");
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        
        // Unsubscribe from network variable changes
        networkTaskCount.OnValueChanged -= OnTaskCountChanged;
        networkDuration.OnValueChanged -= OnDurationChanged;
        networkDisasterType.OnValueChanged -= OnDisasterTypeChanged;
        connectedPlayerCount.OnValueChanged -= OnPlayerCountChanged;
    }

    /// <summary>
    /// Initialize lobby settings from PlayerPrefs (called by host)
    /// </summary>
    private void InitializeLobbySettingsFromPlayerPrefs()
    {
        if (!IsServer) return;

        int taskCount = PlayerPrefs.GetInt("TaskCount", 1);
        int duration = PlayerPrefs.GetInt("GameDuration", 300);
        string disasterType = PlayerPrefs.GetString("DisasterType", "TestKen");

        networkTaskCount.Value = taskCount;
        networkDuration.Value = duration;
        networkDisasterType.Value = GetDisasterTypeInt(disasterType);

        Debug.Log($"[Host] Initialized lobby settings - Tasks: {taskCount}, Duration: {duration}, Disaster: {disasterType}");
    }

    /// <summary>
    /// Update lobby settings (only host can call this)
    /// </summary>
    public void UpdateLobbySettings(int taskCount, int duration, string disasterType)
    {
        if (!IsServer)
        {
            Debug.LogWarning("Only the host can update lobby settings!");
            return;
        }

        networkTaskCount.Value = taskCount;
        networkDuration.Value = duration;
        networkDisasterType.Value = GetDisasterTypeInt(disasterType);

        // Update PlayerPrefs so they're synchronized
        PlayerPrefs.SetInt("TaskCount", taskCount);
        PlayerPrefs.SetInt("GameDuration", duration);
        PlayerPrefs.SetString("DisasterType", disasterType);
        PlayerPrefs.Save();

        Debug.Log($"[Host] Updated lobby settings - Tasks: {taskCount}, Duration: {duration}, Disaster: {disasterType}");
    }

    /// <summary>
    /// Start the game (load the game scene)
    /// </summary>
    public void StartGame()
    {
        if (!IsServer)
        {
            Debug.LogWarning("Only the host can start the game!");
            return;
        }

        // Ensure PlayerPrefs are up to date before loading scene
        PlayerPrefs.SetInt("TaskCount", networkTaskCount.Value);
        PlayerPrefs.SetInt("GameDuration", networkDuration.Value);
        PlayerPrefs.SetString("DisasterType", GetDisasterName(networkDisasterType.Value));
        PlayerPrefs.Save();

        string sceneToLoad = GetDisasterName(networkDisasterType.Value);
        Debug.Log($"[Host] Starting game - Loading scene: {sceneToLoad}");

        // Use NetworkManager to load scene for all clients
        NetworkManager.Singleton.SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Single);
    }

    /// <summary>
    /// Update the connected player count
    /// </summary>
    private void UpdateConnectedPlayerCount()
    {
        if (!IsServer) return;
        connectedPlayerCount.Value = NetworkManager.Singleton.ConnectedClients.Count;
    }

    /// <summary>
    /// Called when a client connects
    /// </summary>
    public void OnClientConnected(ulong clientId)
    {
        if (!IsServer) return;
        UpdateConnectedPlayerCount();
        Debug.Log($"[Server] Client {clientId} connected. Total players: {connectedPlayerCount.Value}");
    }

    /// <summary>
    /// Called when a client disconnects
    /// </summary>
    public void OnClientDisconnected(ulong clientId)
    {
        if (!IsServer) return;
        UpdateConnectedPlayerCount();
        Debug.Log($"[Server] Client {clientId} disconnected. Total players: {connectedPlayerCount.Value}");
    }

    // Network variable change callbacks
    private void OnTaskCountChanged(int oldValue, int newValue)
    {
        Debug.Log($"Task count updated: {oldValue} -> {newValue}");
    }

    private void OnDurationChanged(int oldValue, int newValue)
    {
        Debug.Log($"Duration updated: {oldValue} -> {newValue}");
    }

    private void OnDisasterTypeChanged(int oldValue, int newValue)
    {
        Debug.Log($"Disaster type updated: {GetDisasterName(oldValue)} -> {GetDisasterName(newValue)}");
    }

    private void OnPlayerCountChanged(int oldValue, int newValue)
    {
        Debug.Log($"Player count updated: {oldValue} -> {newValue}");
    }

    // Helper methods
    private int GetDisasterTypeInt(string disasterType)
    {
        switch (disasterType)
        {
            case "Flood": return 0;
            case "Earthquake": return 1;
            case "TestKen": return 2;
            default: return 2;
        }
    }

    private string GetDisasterName(int disasterType)
    {
        switch (disasterType)
        {
            case 0: return "Flood";
            case 1: return "Earthquake";
            case 2: return "TestKen";
            default: return "TestKen";
        }
    }

    // Public getters for current lobby settings
    public int GetTaskCount() => networkTaskCount.Value;
    public int GetDuration() => networkDuration.Value;
    public string GetDisasterType() => GetDisasterName(networkDisasterType.Value);
    public int GetConnectedPlayerCount() => connectedPlayerCount.Value;
    public int GetMaxPlayers() => maxPlayers;
    public bool CanStartGame() => IsServer && connectedPlayerCount.Value > 0;
}
