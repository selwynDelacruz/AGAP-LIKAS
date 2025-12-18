using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

#if METAVC_NGO
using MetaVoiceChat;
using MetaVoiceChat.NetProviders.NGO;
#endif

/// <summary>
/// Central manager for voice chat system. Tracks all voice chat instances and speaking states.
/// Place this on a persistent GameObject in the scene (e.g., GameManager or dedicated VoiceChatManager object).
/// </summary>
public class VoiceChatManager : MonoBehaviour
{
    public static VoiceChatManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private bool debugMode = true;

    // Track all registered voice chat users
    private Dictionary<ulong, VoiceChatUser> _registeredUsers = new Dictionary<ulong, VoiceChatUser>();

    // Events for UI updates
    public event System.Action<ulong, bool> OnUserSpeakingChanged;
    public event System.Action<ulong, VoiceChatUser> OnUserRegistered;
    public event System.Action<ulong> OnUserUnregistered;

    /// <summary>
    /// Data class to hold voice chat user information
    /// </summary>
    public class VoiceChatUser
    {
        public ulong ClientId;
        public string Username;
        public string Role; // "instructor" or "trainee"
#if METAVC_NGO
        public MetaVc MetaVc;
#endif
        public bool IsSpeaking;
        public bool IsMuted;
        public bool IsLocalPlayer;

        // Delegate references for proper unsubscription
        public System.Action<bool> SpeakingCallback;
        public System.Action<bool> MutedCallback;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        // Clean up all subscriptions
        foreach (var user in _registeredUsers.Values)
        {
            UnsubscribeFromUser(user);
        }
        _registeredUsers.Clear();
    }

#if METAVC_NGO
    /// <summary>
    /// Register a voice chat user (called by player prefabs on spawn)
    /// </summary>
    public void RegisterUser(ulong clientId, MetaVc metaVc, string username, string role, bool isLocalPlayer)
    {
        if (_registeredUsers.ContainsKey(clientId))
        {
            if (debugMode)
            {
                Debug.Log($"[VoiceChatManager] User {clientId} already registered. Updating...");
            }
            
            // Unsubscribe old and update
            UnsubscribeFromUser(_registeredUsers[clientId]);
            _registeredUsers[clientId].MetaVc = metaVc;
            SubscribeToUser(_registeredUsers[clientId]);
            return;
        }

        var user = new VoiceChatUser
        {
            ClientId = clientId,
            Username = username,
            Role = role,
            MetaVc = metaVc,
            IsSpeaking = false,
            IsMuted = false,
            IsLocalPlayer = isLocalPlayer
        };

        _registeredUsers[clientId] = user;

        // Subscribe to events using stored delegates
        SubscribeToUser(user);

        if (debugMode)
        {
            Debug.Log($"[VoiceChatManager] Registered user: {username} (ID: {clientId}, Role: {role}, IsLocal: {isLocalPlayer})");
        }

        OnUserRegistered?.Invoke(clientId, user);
    }

    private void SubscribeToUser(VoiceChatUser user)
    {
        if (user.MetaVc == null) return;

        // Create and store delegate references for proper unsubscription
        user.SpeakingCallback = (speaking) => HandleSpeakingChanged(user.ClientId, speaking);
        user.MutedCallback = (muted) => HandleMutedChanged(user.ClientId, muted);

        user.MetaVc.isSpeaking.OnValueChanged += user.SpeakingCallback;
        user.MetaVc.isInputMuted.OnValueChanged += user.MutedCallback;
    }

    private void UnsubscribeFromUser(VoiceChatUser user)
    {
        if (user.MetaVc == null) return;

        if (user.SpeakingCallback != null)
        {
            user.MetaVc.isSpeaking.OnValueChanged -= user.SpeakingCallback;
        }
        if (user.MutedCallback != null)
        {
            user.MetaVc.isInputMuted.OnValueChanged -= user.MutedCallback;
        }
    }
#else
    /// <summary>
    /// Stub method when METAVC_NGO is not defined
    /// </summary>
    public void RegisterUser(ulong clientId, object metaVc, string username, string role, bool isLocalPlayer)
    {
        Debug.LogWarning("[VoiceChatManager] METAVC_NGO not defined. Voice chat registration disabled.");
    }
#endif

    /// <summary>
    /// Unregister a voice chat user (called when player despawns)
    /// </summary>
    public void UnregisterUser(ulong clientId)
    {
        if (_registeredUsers.TryGetValue(clientId, out var user))
        {
#if METAVC_NGO
            UnsubscribeFromUser(user);
#endif

            string username = user.Username;
            _registeredUsers.Remove(clientId);

            if (debugMode)
            {
                Debug.Log($"[VoiceChatManager] Unregistered user: {username} (ID: {clientId})");
            }

            OnUserUnregistered?.Invoke(clientId);
        }
    }

    private void HandleSpeakingChanged(ulong clientId, bool isSpeaking)
    {
        if (_registeredUsers.TryGetValue(clientId, out var user))
        {
            user.IsSpeaking = isSpeaking;

            if (debugMode && isSpeaking)
            {
                Debug.Log($"[VoiceChatManager] {user.Username} ({user.Role}) is speaking");
            }

            OnUserSpeakingChanged?.Invoke(clientId, isSpeaking);
        }
    }

    private void HandleMutedChanged(ulong clientId, bool isMuted)
    {
        if (_registeredUsers.TryGetValue(clientId, out var user))
        {
            user.IsMuted = isMuted;

            if (debugMode)
            {
                Debug.Log($"[VoiceChatManager] {user.Username} muted: {isMuted}");
            }
        }
    }

    /// <summary>
    /// Get all registered users
    /// </summary>
    public IEnumerable<VoiceChatUser> GetAllUsers()
    {
        return _registeredUsers.Values;
    }

    /// <summary>
    /// Get all trainee users
    /// </summary>
    public IEnumerable<VoiceChatUser> GetTrainees()
    {
        foreach (var user in _registeredUsers.Values)
        {
            if (user.Role == "trainee")
            {
                yield return user;
            }
        }
    }

    /// <summary>
    /// Get all users who are currently speaking
    /// </summary>
    public IEnumerable<VoiceChatUser> GetSpeakingUsers()
    {
        foreach (var user in _registeredUsers.Values)
        {
            if (user.IsSpeaking && !user.IsMuted)
            {
                yield return user;
            }
        }
    }

    /// <summary>
    /// Get a specific user by client ID
    /// </summary>
    public VoiceChatUser GetUser(ulong clientId)
    {
        _registeredUsers.TryGetValue(clientId, out var user);
        return user;
    }

    /// <summary>
    /// Get the local player's voice chat user
    /// </summary>
    public VoiceChatUser GetLocalUser()
    {
        foreach (var user in _registeredUsers.Values)
        {
            if (user.IsLocalPlayer)
            {
                return user;
            }
        }
        return null;
    }

    /// <summary>
    /// Mute/unmute the local player's microphone
    /// </summary>
    public void SetLocalMuted(bool muted)
    {
#if METAVC_NGO
        var localUser = GetLocalUser();
        if (localUser?.MetaVc != null)
        {
            localUser.MetaVc.isInputMuted.Value = muted;
        }
#endif
    }

    /// <summary>
    /// Check if local player is muted
    /// </summary>
    public bool IsLocalMuted()
    {
#if METAVC_NGO
        var localUser = GetLocalUser();
        return localUser?.MetaVc?.isInputMuted.Value ?? true;
#else
        return true;
#endif
    }
}
