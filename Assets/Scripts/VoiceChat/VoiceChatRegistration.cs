using UnityEngine;
using Unity.Netcode;

#if METAVC_NGO
using MetaVoiceChat;
using MetaVoiceChat.NetProviders.NGO;
#endif

/// <summary>
/// Attach this to player prefabs (trainee and instructor) alongside MetaVc.
/// Automatically registers the player with VoiceChatManager when spawned.
/// </summary>
public class VoiceChatRegistration : NetworkBehaviour
{
    [Header("User Info")]
    [Tooltip("Username - if empty, will use 'Player {ClientId}'")]
    [SerializeField] private string username = "";
    
    [Tooltip("Role - if empty, will try to get from PlayerPrefs")]
    [SerializeField] private string role = "";

    [Header("Debug")]
    [Tooltip("Enable detailed logging")]
    [SerializeField] private bool debugMode = true;

    // Network variables to sync username and role across all clients
    private NetworkVariable<NetworkString> networkUsername = new NetworkVariable<NetworkString>(
        new NetworkString(""),
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    private NetworkVariable<NetworkString> networkRole = new NetworkVariable<NetworkString>(
        new NetworkString(""),
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

#if METAVC_NGO
    private MetaVc _metaVc;
#endif

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

#if METAVC_NGO
        // Find MetaVc component - search in children and parent
        _metaVc = GetComponentInChildren<MetaVc>();
        
        if (_metaVc == null)
        {
            _metaVc = GetComponentInParent<MetaVc>();
        }

        if (_metaVc == null)
        {
            if (debugMode)
            {
                Debug.LogWarning($"[VoiceChatRegistration] No MetaVc found on {gameObject.name}! Voice chat will not work. This is OK if this prefab doesn't need voice chat.");
            }
            
            // Don't return - still register the player for tracking purposes
        }

        // Determine username
        string displayName = username;
        if (string.IsNullOrEmpty(displayName))
        {
            // Try to get from PlayerPrefs (if local player)
            if (IsOwner)
            {
                // Try multiple possible keys
                displayName = PlayerPrefs.GetString("Current_Username", "");
                if (string.IsNullOrEmpty(displayName))
                {
                    displayName = PlayerPrefs.GetString("Username", "");
                }
                if (string.IsNullOrEmpty(displayName))
                {
                    displayName = PlayerPrefs.GetString("PlayerName", "");
                }
                if (string.IsNullOrEmpty(displayName))
                {
                    displayName = PlayerPrefs.GetString("Current_Name", "");
                }
            }

            // Fallback to generic name
            if (string.IsNullOrEmpty(displayName))
            {
                displayName = $"Player {OwnerClientId}";
            }
        }

        // Determine role
        string userRole = role;
        if (string.IsNullOrEmpty(userRole))
        {
            if (IsOwner)
            {
                userRole = PlayerPrefs.GetString("Type_Of_User", "");
            }

            // Fallback: assume trainee for clients, instructor for host
            if (string.IsNullOrEmpty(userRole))
            {
                userRole = IsHost ? "instructor" : "trainee";
            }
        }

        // If this is the owner, set the network variables
        if (IsOwner)
        {
            networkUsername.Value = new NetworkString(displayName);
            networkRole.Value = new NetworkString(userRole);
            
            if (debugMode)
            {
                Debug.Log($"[VoiceChatRegistration] Owner setting networked data: {displayName} ({userRole})");
            }
        }
        else
        {
            // For remote players, wait a bit for network variables to sync
            Invoke(nameof(RegisterAfterSync), 0.3f);
            return;
        }

        // Register with VoiceChatManager (for local player immediately)
        RegisterWithManager(displayName, userRole);
#else
        if (debugMode)
        {
            Debug.LogWarning("[VoiceChatRegistration] METAVC_NGO not defined. Voice chat registration disabled.");
        }
#endif
    }

    private void RegisterAfterSync()
    {
#if METAVC_NGO
        // Use networked values for remote players
        string displayName = networkUsername.Value.ToString();
        string userRole = networkRole.Value.ToString();

        // Fallback if sync failed
        if (string.IsNullOrEmpty(displayName))
        {
            displayName = $"Player {OwnerClientId}";
        }
        if (string.IsNullOrEmpty(userRole))
        {
            userRole = IsHost ? "instructor" : "trainee";
        }

        if (debugMode)
        {
            Debug.Log($"[VoiceChatRegistration] Remote player synced: {displayName} ({userRole})");
        }

        RegisterWithManager(displayName, userRole);
#endif
    }

    private void RegisterWithManager(string displayName, string userRole)
    {
#if METAVC_NGO
        // Register with VoiceChatManager (even if MetaVc is null - for tracking purposes)
        if (VoiceChatManager.Instance != null)
        {
            VoiceChatManager.Instance.RegisterUser(
                OwnerClientId,
                _metaVc, // Can be null - VoiceChatManager will handle it
                displayName,
                userRole,
                IsOwner
            );

            if (debugMode)
            {
                string vcStatus = _metaVc != null ? "with voice chat" : "WITHOUT voice chat (MetaVc missing)";
                Debug.Log($"[VoiceChatRegistration] Registered: {displayName} (ID: {OwnerClientId}, Role: {userRole}, IsLocal: {IsOwner}) {vcStatus}");
            }
        }
        else
        {
            if (debugMode)
            {
                Debug.LogWarning("[VoiceChatRegistration] VoiceChatManager not found! Voice chat tracking won't work.");
            }
        }
#endif
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        // Unregister from VoiceChatManager
        if (VoiceChatManager.Instance != null)
        {
            VoiceChatManager.Instance.UnregisterUser(OwnerClientId);
        }
    }

    /// <summary>
    /// Set username at runtime (useful for late initialization)
    /// </summary>
    public void SetUsername(string newUsername)
    {
        username = newUsername;
        if (IsOwner)
        {
            networkUsername.Value = new NetworkString(newUsername);
        }
    }

    /// <summary>
    /// Set role at runtime
    /// </summary>
    public void SetRole(string newRole)
    {
        role = newRole;
        if (IsOwner)
        {
            networkRole.Value = new NetworkString(newRole);
        }
    }
}

/// <summary>
/// Helper struct for NetworkVariable string support
/// </summary>
public struct NetworkString : INetworkSerializable
{
    private string value;

    public NetworkString(string value)
    {
        this.value = value ?? string.Empty;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        if (serializer.IsReader)
        {
            serializer.SerializeValue(ref value);
        }
        else
        {
            serializer.SerializeValue(ref value);
        }
    }

    public override string ToString()
    {
        return value ?? string.Empty;
    }

    public static implicit operator string(NetworkString ns) => ns.value ?? string.Empty;
    public static implicit operator NetworkString(string s) => new NetworkString(s);
}
