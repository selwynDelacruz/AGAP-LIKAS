using UnityEngine;
using Unity.Services.Vivox;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Unity.Netcode;

/// <summary>
/// Manages Vivox voice chat in the game scene (TestKen)
/// Trainees can speak and hear each other
/// Instructors can only listen (no transmit)
/// </summary>
public class GameVoiceChatManager : NetworkBehaviour
{
    public static GameVoiceChatManager Instance { get; private set; }

    [Header("Channel Settings")]
    [SerializeField] private string channelName = "gameChannel";
    [SerializeField] private ChatCapability chatCapability = ChatCapability.AudioOnly;

    [Header("Audio Settings")]
    [SerializeField] private int defaultTransmissionVolume = 50; // 0-100
    [SerializeField] private int defaultReceptionVolume = 50; // 0-100
    [SerializeField] private int volumeStep = 10; // Volume change per keypress

    [Header("Input Settings")]
    [SerializeField] private KeyCode muteKey = KeyCode.M;
    [SerializeField] private KeyCode volumeDownKey = KeyCode.Minus;
    [SerializeField] private KeyCode volumeUpKey = KeyCode.Equals;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    // State
    private bool isInitialized = false;
    private bool isInChannel = false;
    private bool isMuted = false;
    private int currentVolume = 50;
    private string currentUserRole = "";
    private Dictionary<string, VivoxParticipant> participantCache = new Dictionary<string, VivoxParticipant>();
    private Dictionary<string, bool> participantSpeechState = new Dictionary<string, bool>();

    // Events for UI
    public event Action<bool> OnMuteStateChanged;
    public event Action<int> OnVolumeChanged;
    public event Action<string, bool> OnParticipantSpeakingChanged;
    public event Action OnVoiceChatReady;

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

    private async void Start()
    {
        currentUserRole = PlayerPrefs.GetString("Type_Of_User", "trainee");

        if (showDebugLogs)
            Debug.Log($"[GameVoiceChatManager] Starting voice chat initialization. Role: {currentUserRole}");

        // Wait using real time (unaffected by Time.timeScale)
        await Task.Delay(1500);

        await InitializeVoiceChat();
    }

    private void Update()
    {
        if (!isInChannel || !isInitialized)
            return;

        if (currentUserRole == "trainee")
        {
            HandleTraineeInput();
        }

        CheckParticipantSpeech();
    }

    private async Task InitializeVoiceChat()
    {
        try
        {
            if (VivoxServiceManager.Instance == null)
            {
                Debug.LogError("[GameVoiceChatManager] VivoxServiceManager not found!");
                OnVoiceChatReady?.Invoke();
                return;
            }

            // Wait for VivoxServiceManager to be ready (max 10 seconds)
            float waitTime = 0f;
            while (!VivoxServiceManager.Instance.IsInitialized && waitTime < 10f)
            {
                await Task.Delay(100);
                waitTime += 0.1f;
            }

            if (!VivoxServiceManager.Instance.IsInitialized)
            {
                Debug.LogError("[GameVoiceChatManager] VivoxServiceManager failed to initialize within timeout!");
                OnVoiceChatReady?.Invoke();
                return;
            }

            string playerName = GetUniquePlayerName();

            if (showDebugLogs)
                Debug.Log($"[GameVoiceChatManager] Logging in as: {playerName}");

            // Login to Vivox (if not already logged in)
            if (!VivoxServiceManager.Instance.IsLoggedIn())
            {
                await VivoxServiceManager.Instance.LoginAsync(playerName);
            }

            if (showDebugLogs)
                Debug.Log("[GameVoiceChatManager] Logged in to Vivox");

            // CRITICAL: Increase delay to ensure Vivox SDK is fully ready after scene transition
            await Task.Delay(2000); // Increased from 1000 to 2000ms

            isInitialized = true;

            // Join the game channel
            await JoinGameChannel();
        }
        catch (Exception e)
        {
            Debug.LogError($"[GameVoiceChatManager] Failed to initialize voice chat: {e.Message}\n{e.StackTrace}");
            OnVoiceChatReady?.Invoke();
        }
    }

    private async Task JoinGameChannel()
    {
        try
        {
            // EXTRA SAFETY: Verify VivoxService.Instance exists
            if (VivoxService.Instance == null)
            {
                Debug.LogError("[GameVoiceChatManager] VivoxService.Instance is null! Attempting to re-initialize...");
                
                // Try to force re-initialization
                if (VivoxServiceManager.Instance != null)
                {
                    await VivoxServiceManager.Instance.ForceReinitialize();
                    await Task.Delay(500); // Wait a bit for initialization
                }
                
                // Check again
                if (VivoxService.Instance == null)
                {
                    Debug.LogError("[GameVoiceChatManager] Re-initialization failed. Voice chat unavailable.");
                    OnVoiceChatReady?.Invoke(); // Allow game to continue
                    return;
                }
            }

            if (showDebugLogs)
                Debug.Log($"[GameVoiceChatManager] Joining channel: {channelName}");

            await VivoxService.Instance.JoinGroupChannelAsync(channelName, chatCapability);

            if (showDebugLogs)
                Debug.Log($"[GameVoiceChatManager] Successfully joined channel: {channelName}");

            isInChannel = true;

            SubscribeToVivoxEvents();
            ConfigureChannelForRole();

            currentVolume = defaultReceptionVolume;
            await SetVolumeAsync(currentVolume);

            OnVoiceChatReady?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError($"[GameVoiceChatManager] Failed to join channel: {e.Message}\n{e.StackTrace}");
            OnVoiceChatReady?.Invoke(); // Allow game to continue
        }
    }

    private void ConfigureChannelForRole()
    {
        if (!isInChannel)
            return;

        if (currentUserRole == "instructor")
        {
            VivoxService.Instance.MuteInputDevice();
            isMuted = true;

            if (showDebugLogs)
                Debug.Log("[GameVoiceChatManager] Instructor mode: Input muted (listen-only)");
        }
        else
        {
            VivoxService.Instance.UnmuteInputDevice();
            VivoxService.Instance.SetInputDeviceVolume(defaultTransmissionVolume);
            isMuted = false;

            if (showDebugLogs)
                Debug.Log("[GameVoiceChatManager] Trainee mode: Voice chat enabled");
        }

        VivoxService.Instance.SetOutputDeviceVolume(defaultReceptionVolume);
    }

    private void SubscribeToVivoxEvents()
    {
        VivoxService.Instance.ParticipantAddedToChannel += OnParticipantAdded;
        VivoxService.Instance.ParticipantRemovedFromChannel += OnParticipantRemoved;

        if (showDebugLogs)
            Debug.Log("[GameVoiceChatManager] Subscribed to Vivox events");
    }

    private void UnsubscribeFromVivoxEvents()
    {
        if (VivoxService.Instance != null)
        {
            VivoxService.Instance.ParticipantAddedToChannel -= OnParticipantAdded;
            VivoxService.Instance.ParticipantRemovedFromChannel -= OnParticipantRemoved;
        }

        participantCache.Clear();
        participantSpeechState.Clear();

        if (showDebugLogs)
            Debug.Log("[GameVoiceChatManager] Unsubscribed from Vivox events");
    }

    private void OnParticipantAdded(VivoxParticipant participant)
    {
        if (showDebugLogs)
            Debug.Log($"[GameVoiceChatManager] Participant joined: {participant.DisplayName} in channel: {participant.ChannelName} (IsSelf: {participant.IsSelf})");

        if (participant.IsSelf)
            return;

        string key = $"{participant.ChannelName}_{participant.PlayerId}";
        if (!participantCache.ContainsKey(key))
        {
            participantCache[key] = participant;
            participantSpeechState[key] = false;
        }
    }

    private void OnParticipantRemoved(VivoxParticipant participant)
    {
        if (showDebugLogs)
            Debug.Log($"[GameVoiceChatManager] Participant left: {participant.DisplayName} from channel: {participant.ChannelName}");

        string key = $"{participant.ChannelName}_{participant.PlayerId}";
        participantCache.Remove(key);
        participantSpeechState.Remove(key);

        OnParticipantSpeakingChanged?.Invoke(participant.DisplayName, false);
    }

    private void CheckParticipantSpeech()
    {
        foreach (var kvp in participantCache)
        {
            string key = kvp.Key;
            VivoxParticipant participant = kvp.Value;

            bool currentSpeechState = participant.SpeechDetected;
            bool previousSpeechState = participantSpeechState.ContainsKey(key) ? participantSpeechState[key] : false;

            if (currentSpeechState != previousSpeechState)
            {
                participantSpeechState[key] = currentSpeechState;
                OnParticipantSpeakingChanged?.Invoke(participant.DisplayName, currentSpeechState);

                if (showDebugLogs)
                    Debug.Log($"[GameVoiceChatManager] {participant.DisplayName} speech state: {currentSpeechState}");
            }
        }
    }

    private void HandleTraineeInput()
    {
        if (Input.GetKeyDown(muteKey))
        {
            ToggleMute();
        }

        if (Input.GetKeyDown(volumeDownKey))
        {
            AdjustVolume(-volumeStep);
        }

        if (Input.GetKeyDown(volumeUpKey))
        {
            AdjustVolume(volumeStep);
        }
    }

    public void ToggleMute()
    {
        if (currentUserRole != "trainee" || !isInChannel)
            return;

        isMuted = !isMuted;

        if (isMuted)
        {
            VivoxService.Instance.MuteInputDevice();
        }
        else
        {
            VivoxService.Instance.UnmuteInputDevice();
        }

        if (showDebugLogs)
            Debug.Log($"[GameVoiceChatManager] Mute toggled: {isMuted}");

        OnMuteStateChanged?.Invoke(isMuted);
    }

    public void SetMute(bool mute)
    {
        if (currentUserRole != "trainee" || !isInChannel)
            return;

        isMuted = mute;

        if (isMuted)
        {
            VivoxService.Instance.MuteInputDevice();
        }
        else
        {
            VivoxService.Instance.UnmuteInputDevice();
        }

        if (showDebugLogs)
            Debug.Log($"[GameVoiceChatManager] Mute set to: {isMuted}");

        OnMuteStateChanged?.Invoke(isMuted);
    }

    public void AdjustVolume(int delta)
    {
        currentVolume = Mathf.Clamp(currentVolume + delta, 0, 100);
        _ = SetVolumeAsync(currentVolume);
    }

    public void SetVolume(int volume)
    {
        _ = SetVolumeAsync(volume);
    }

    public async Task SetVolumeAsync(int volume)
    {
        if (!isInChannel)
            return;

        currentVolume = Mathf.Clamp(volume, 0, 100);

        try
        {
            int vivoxVolume = Mathf.RoundToInt(Mathf.Lerp(-50, 50, currentVolume / 100f));
            await VivoxService.Instance.SetChannelVolumeAsync(channelName, vivoxVolume);

            if (showDebugLogs)
                Debug.Log($"[GameVoiceChatManager] Volume set to: {currentVolume}% (Vivox: {vivoxVolume})");

            OnVolumeChanged?.Invoke(currentVolume);
        }
        catch (Exception e)
        {
            Debug.LogError($"[GameVoiceChatManager] Failed to set volume: {e.Message}");
        }
    }

    private string GetUniquePlayerName()
    {
        string baseName = PlayerPrefs.GetString("Current_Name", "Player");
        string role = currentUserRole;

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsClient)
        {
            ulong clientId = NetworkManager.Singleton.LocalClientId;
            return $"{baseName}_{role}_{clientId}";
        }

        return $"{baseName}_{role}_{DateTime.Now.Ticks}";
    }

    public List<VivoxParticipant> GetParticipants()
    {
        List<VivoxParticipant> participants = new List<VivoxParticipant>();

        if (!isInChannel || !VivoxService.Instance.ActiveChannels.ContainsKey(channelName))
            return participants;

        var channelParticipants = VivoxService.Instance.ActiveChannels[channelName];

        foreach (var participant in channelParticipants)
        {
            if (!participant.IsSelf)
            {
                participants.Add(participant);
            }
        }

        return participants;
    }

    public bool IsMuted()
    {
        return isMuted;
    }

    public int GetVolume()
    {
        return currentVolume;
    }

    public string GetUserRole()
    {
        return currentUserRole;
    }

    public bool IsReady()
    {
        return isInitialized && isInChannel;
    }

    private async Task LeaveChannel()
    {
        if (!isInChannel)
            return;

        try
        {
            UnsubscribeFromVivoxEvents();

            await VivoxService.Instance.LeaveChannelAsync(channelName);

            if (showDebugLogs)
                Debug.Log("[GameVoiceChatManager] Left voice channel");

            isInChannel = false;
        }
        catch (Exception e)
        {
            Debug.LogError($"[GameVoiceChatManager] Error leaving channel: {e.Message}");
        }
    }

    private new async void OnDestroy()
    {
        await LeaveChannel();

        if (Instance == this)
        {
            Instance = null;
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        _ = LeaveChannel();
    }
}
