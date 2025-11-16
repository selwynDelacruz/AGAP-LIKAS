using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System;
using System.Collections.Generic;

/// <summary>
/// Manages lobby creation and joining using generated lobby codes with Unity Netcode.
/// Instructors can generate lobby codes and host sessions.
/// Trainees can join using the generated lobby code.
/// </summary>
public class NetworkLobbyManager : MonoBehaviour
{
    public static NetworkLobbyManager Instance { get; private set; }

    [Header("Lobby Settings")]
    [SerializeField] private int lobbyCodeLength = 6;
    
    private string currentLobbyCode;
    private bool isInstructor = false;
    private Dictionary<string, LobbyData> activeLobbyies = new Dictionary<string, LobbyData>();

    // Events for UI updates
    public event Action<string> OnLobbyCodeGenerated;
    public event Action<bool, string> OnLobbyJoinAttempt; // success, message
    public event Action<int> OnPlayersCountChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        Debug.Log("NetworkLobbyManager initialized and set to DontDestroyOnLoad");
    }

    private void Start()
    {
        // Subscribe to network events
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    /// <summary>
    /// Generates a unique lobby code for instructors
    /// </summary>
    public string GenerateLobbyCode()
    {
        if (!CheckIfInstructor())
        {
            Debug.LogWarning("Only instructors can generate lobby codes!");
            return null;
        }

        // Generate a random alphanumeric code
        currentLobbyCode = GenerateRandomCode(lobbyCodeLength);
        
        // Store lobby data
        LobbyData lobbyData = new LobbyData
        {
            lobbyCode = currentLobbyCode,
            hostId = 0, // Will be set when host starts
            createdAt = DateTime.Now,
            maxPlayers = 10 // Configurable
        };
        
        activeLobbyies[currentLobbyCode] = lobbyData;
        
        Debug.Log($"Generated Lobby Code: {currentLobbyCode}");
        OnLobbyCodeGenerated?.Invoke(currentLobbyCode);
        
        return currentLobbyCode;
    }

    /// <summary>
    /// Starts hosting as an instructor with the generated lobby code
    /// </summary>
    public bool StartHostWithLobbyCode()
    {
        if (!CheckIfInstructor())
        {
            Debug.LogWarning("Only instructors can host lobbies!");
            return false;
        }

        if (string.IsNullOrEmpty(currentLobbyCode))
        {
            Debug.LogError("No lobby code generated! Generate a lobby code first.");
            return false;
        }

        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager not found!");
            return false;
        }

        // Configure transport settings for relay/direct connection
        ConfigureTransportForHost();

        // Start as host
        bool success = NetworkManager.Singleton.StartHost();
        
        if (success)
        {
            isInstructor = true;
            Debug.Log($"Started hosting with lobby code: {currentLobbyCode}");
            
            // Store the lobby code in PlayerPrefs for persistence
            PlayerPrefs.SetString("CurrentLobbyCode", currentLobbyCode);
            PlayerPrefs.SetString("NetworkRole", "Instructor");
            PlayerPrefs.Save();
        }
        else
        {
            Debug.LogError("Failed to start host!");
        }

        return success;
    }

    /// <summary>
    /// Joins a lobby as a trainee using the provided lobby code
    /// </summary>
    public bool JoinLobbyWithCode(string lobbyCode)
    {
        if (CheckIfInstructor())
        {
            Debug.LogWarning("Instructors should host, not join lobbies!");
            OnLobbyJoinAttempt?.Invoke(false, "Instructors cannot join lobbies");
            return false;
        }

        if (string.IsNullOrEmpty(lobbyCode))
        {
            Debug.LogError("Lobby code cannot be empty!");
            OnLobbyJoinAttempt?.Invoke(false, "Please enter a lobby code");
            return false;
        }

        // Validate lobby code format
        if (lobbyCode.Length != lobbyCodeLength)
        {
            Debug.LogError($"Invalid lobby code length! Expected {lobbyCodeLength} characters.");
            OnLobbyJoinAttempt?.Invoke(false, $"Lobby code must be {lobbyCodeLength} characters");
            return false;
        }

        currentLobbyCode = lobbyCode.ToUpper();

        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager not found!");
            OnLobbyJoinAttempt?.Invoke(false, "Network system not initialized");
            return false;
        }

        // Configure transport settings for client
        ConfigureTransportForClient(currentLobbyCode);

        // Start as client
        bool success = NetworkManager.Singleton.StartClient();
        
        if (success)
        {
            isInstructor = false;
            Debug.Log($"Attempting to join lobby with code: {currentLobbyCode}");
            
            // Store the lobby code in PlayerPrefs
            PlayerPrefs.SetString("CurrentLobbyCode", currentLobbyCode);
            PlayerPrefs.SetString("NetworkRole", "Trainee");
            PlayerPrefs.Save();
            
            OnLobbyJoinAttempt?.Invoke(true, "Connecting to lobby...");
        }
        else    
        {
            Debug.LogError("Failed to start client!");
            OnLobbyJoinAttempt?.Invoke(false, "Failed to connect");
        }

        return success;
    }

    /// <summary>
    /// Configures the Unity Transport for hosting
    /// </summary>
    private void ConfigureTransportForHost()
    {
        if (NetworkManager.Singleton.NetworkConfig.NetworkTransport is UnityTransport transport)
        {
            // For local testing, use localhost
            transport.SetConnectionData("127.0.0.1", 7777);
            
            // TODO: For production, integrate with Unity Relay or another solution
            // Example with Relay:
            // transport.SetRelayServerData(...);
            
            Debug.Log("Transport configured for host on port 7777");
        }
    }

    /// <summary>
    /// Configures the Unity Transport for joining as client
    /// </summary>
    private void ConfigureTransportForClient(string lobbyCode)
    {
        if (NetworkManager.Singleton.NetworkConfig.NetworkTransport is UnityTransport transport)
        {
            // For local testing, connect to localhost
            // In production, you would map the lobby code to an IP/Relay code
            transport.SetConnectionData("127.0.0.1", 7777);
            
            // TODO: For production, implement lobby code to connection mapping
            // This could involve:
            // 1. Using Unity Relay Service (recommended for production)
            // 2. Using a matchmaking server that maps codes to IP addresses
            // 3. Using a database to store code-to-relay mappings
            
            Debug.Log($"Transport configured for client joining lobby: {lobbyCode}");
        }
    }

    /// <summary>
    /// Generates a random alphanumeric code
    /// </summary>
    private string GenerateRandomCode(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        System.Text.StringBuilder result = new System.Text.StringBuilder(length);
        System.Random random = new System.Random();

        for (int i = 0; i < length; i++)
        {
            result.Append(chars[random.Next(chars.Length)]);
        }

        return result.ToString();
    }

    /// <summary>
    /// Checks if the current user is an instructor
    /// </summary>
    private bool CheckIfInstructor()
    {
        // Check from RoomManager if available
        if (RoomManager.Instance != null)
        {
            return RoomManager.Instance.isInstructor;
        }

        // Fallback to PlayerPrefs
        string userType = PlayerPrefs.GetString("Type_Of_User", "trainee");
        return userType == "instructor";
    }

    /// <summary>
    /// Leaves the current lobby
    /// </summary>
    public void LeaveLobby()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
            Debug.Log("Left the lobby");
        }

        currentLobbyCode = null;
        PlayerPrefs.DeleteKey("CurrentLobbyCode");
        PlayerPrefs.DeleteKey("NetworkRole");
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Gets the current lobby code
    /// </summary>
    public string GetCurrentLobbyCode()
    {
        return currentLobbyCode;
    }

    /// <summary>
    /// Gets the number of connected players
    /// </summary>
    public int GetConnectedPlayersCount()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            return (int)NetworkManager.Singleton.ConnectedClients.Count;
        }
        return 0;
    }

    /// <summary>
    /// Callback when a client connects
    /// </summary>
    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} connected to lobby");
        int playerCount = GetConnectedPlayersCount();
        OnPlayersCountChanged?.Invoke(playerCount);

        // Update RoomManager if available
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.playersCount = playerCount;
        }
    }

    /// <summary>
    /// Callback when a client disconnects
    /// </summary>
    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} disconnected from lobby");
        int playerCount = GetConnectedPlayersCount();
        OnPlayersCountChanged?.Invoke(playerCount);

        // Update RoomManager if available
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.playersCount = playerCount;
        }
    }

    /// <summary>
    /// Data class to store lobby information
    /// </summary>
    [System.Serializable]
    private class LobbyData
    {
        public string lobbyCode;
        public ulong hostId;
        public DateTime createdAt;
        public int maxPlayers;
    }
}
