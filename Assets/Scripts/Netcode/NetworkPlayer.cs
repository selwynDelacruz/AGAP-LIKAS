using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

/// <summary>
/// Serializable string struct for NetworkVariable
/// </summary>
public struct NetworkString : INetworkSerializable
{
    private FixedString128Bytes info;

    public NetworkString(string value)
    {
        info = new FixedString128Bytes(value);
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref info);
    }

    public override string ToString()
    {
        return info.ToString();
    }

    public static implicit operator string(NetworkString s) => s.ToString();
    public static implicit operator NetworkString(string s) => new NetworkString(s);
}

/// <summary>
/// Handles networked player behavior and synchronization
/// This script should be attached to player prefabs to enable network functionality
/// </summary>
public class NetworkPlayer : NetworkBehaviour
{
    [Header("Player Info")]
    [SerializeField] private string playerName = "Player";
    [SerializeField] private string playerUsername = "";
    
    // Network variables for player data
    private NetworkVariable<bool> isInstructorPlayer = new NetworkVariable<bool>(
        false, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Owner
    );

    private NetworkVariable<int> playerTeam = new NetworkVariable<int>(
        0, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server
    );

    // Network variable for player name (synced across network)
    private NetworkVariable<NetworkString> networkPlayerName = new NetworkVariable<NetworkString>(
        new NetworkString("Player"),
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    private NetworkVariable<NetworkString> networkPlayerUsername = new NetworkVariable<NetworkString>(
        new NetworkString(""),
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    public bool IsInstructor => isInstructorPlayer.Value;
    public int Team => playerTeam.Value;
    public string PlayerName => networkPlayerName.Value.ToString();
    public string PlayerUsername => networkPlayerUsername.Value.ToString();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            InitializePlayerData();
        }

        // Subscribe to network variable changes
        isInstructorPlayer.OnValueChanged += OnInstructorStatusChanged;
        playerTeam.OnValueChanged += OnTeamChanged;
        networkPlayerName.OnValueChanged += OnPlayerNameChanged;
        networkPlayerUsername.OnValueChanged += OnPlayerUsernameChanged;

        Debug.Log($"NetworkPlayer spawned - Owner: {IsOwner}, Instructor: {IsInstructor}, Team: {Team}, Name: {PlayerName}, Username: {PlayerUsername}");

        // Notify LobbySceneManager if we're in the lobby scene
        NotifyLobbySceneManager();
    }

    public override void OnNetworkDespawn()
    {
        // Unsubscribe from network variable changes
        isInstructorPlayer.OnValueChanged -= OnInstructorStatusChanged;
        playerTeam.OnValueChanged -= OnTeamChanged;
        networkPlayerName.OnValueChanged -= OnPlayerNameChanged;
        networkPlayerUsername.OnValueChanged -= OnPlayerUsernameChanged;

        base.OnNetworkDespawn();
    }

    private void InitializePlayerData()
    {
        // Get player info from PlayerPrefs (set by AuthManager after login)
        bool isInstructor = false;
        string playerName = "Player";
        string playerUsername = "";

        // Check RoomManager first
        if (RoomManager.Instance != null)
        {
            isInstructor = RoomManager.Instance.isInstructor;
        }
        else
        {
            string userType = PlayerPrefs.GetString("Type_Of_User", "trainee");
            isInstructor = userType.ToLower() == "instructor";
        }

        // Get player name and username from PlayerPrefs (set by AuthManager)
        playerName = PlayerPrefs.GetString("PlayerName", "Player");
        playerUsername = PlayerPrefs.GetString("PlayerUsername", "User" + OwnerClientId);

        // If still empty, try AuthManager directly
        if (string.IsNullOrEmpty(playerName) && AuthManager.Instance != null)
        {
            playerName = AuthManager.Instance.Current_Name;
            playerUsername = AuthManager.Instance.Current_Username;
        }

        // Set the network variables
        isInstructorPlayer.Value = isInstructor;
        networkPlayerName.Value = new NetworkString(playerName);
        networkPlayerUsername.Value = new NetworkString(playerUsername);

        Debug.Log($"Initialized player data - Name: {playerName}, Username: {playerUsername}, Instructor: {isInstructor}");

        // Request team assignment from server
        if (IsClient && !IsServer)
        {
            RequestTeamAssignmentServerRpc();
        }
        else if (IsServer)
        {
            // Server assigns its own team
            AssignTeam();
        }
    }

    [ServerRpc]
    private void RequestTeamAssignmentServerRpc(ServerRpcParams rpcParams = default)
    {
        // Server assigns team based on connection order or other logic
        AssignTeam();
    }

    private void AssignTeam()
    {
        if (!IsServer)
            return;

        // Simple team assignment: team ID based on client ID
        // You can implement more complex logic here
        playerTeam.Value = (int)OwnerClientId;

        Debug.Log($"Assigned team {playerTeam.Value} to client {OwnerClientId}");
    }

    private void OnInstructorStatusChanged(bool previousValue, bool newValue)
    {
        Debug.Log($"Player instructor status changed from {previousValue} to {newValue}");
        // Handle UI updates or gameplay changes based on instructor status
        NotifyLobbySceneManager();
    }

    private void OnTeamChanged(int previousValue, int newValue)
    {
        Debug.Log($"Player team changed from {previousValue} to {newValue}");
        // Handle team color changes, spawn positions, etc.
    }

    private void OnPlayerNameChanged(NetworkString previousValue, NetworkString newValue)
    {
        Debug.Log($"Player name changed from {previousValue} to {newValue}");
        NotifyLobbySceneManager();
    }

    private void OnPlayerUsernameChanged(NetworkString previousValue, NetworkString newValue)
    {
        Debug.Log($"Player username changed from {previousValue} to {newValue}");
        NotifyLobbySceneManager();
    }

    /// <summary>
    /// Notifies the LobbySceneManager about this player's data
    /// </summary>
    private void NotifyLobbySceneManager()
    {
        LobbySceneManager lobbyManager = FindFirstObjectByType<LobbySceneManager>();
        if (lobbyManager != null)
        {
            string displayName = !string.IsNullOrEmpty(PlayerUsername) ? PlayerUsername : PlayerName;
            lobbyManager.UpdatePlayerData(OwnerClientId, displayName, IsInstructor);
        }
    }

    // ========== Public Methods ==========

    /// <summary>
    /// Sets the player's display name
    /// </summary>
    public void SetPlayerName(string name)
    {
        if (IsOwner)
        {
            networkPlayerName.Value = new NetworkString(name);
            SetPlayerNameServerRpc(name);
        }
    }

    [ServerRpc]
    private void SetPlayerNameServerRpc(string name)
    {
        networkPlayerName.Value = new NetworkString(name);
        SetPlayerNameClientRpc(name);
    }

    [ClientRpc]
    private void SetPlayerNameClientRpc(string name)
    {
        if (!IsOwner)
        {
            networkPlayerName.Value = new NetworkString(name);
        }
    }

    /// <summary>
    /// Gets the player's display name
    /// </summary>
    public string GetPlayerName()
    {
        return PlayerName;
    }

    /// <summary>
    /// Gets the player's username
    /// </summary>
    public string GetPlayerUsername()
    {
        return PlayerUsername;
    }

    /// <summary>
    /// Send a chat message or notification to all players
    /// </summary>
    public void BroadcastPlayerMessage(string message)
    {
        if (IsOwner)
        {
            SendMessageServerRpc(message);
        }
    }

    [ServerRpc]
    private void SendMessageServerRpc(string message)
    {
        BroadcastMessageClientRpc(PlayerName, message);
    }

    [ClientRpc]
    private void BroadcastMessageClientRpc(string senderName, string message)
    {
        Debug.Log($"[{senderName}]: {message}");
        // You can integrate with a chat UI here
    }
}
