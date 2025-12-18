using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// UI Controller for instructor voice chat monitoring
/// Shows list of trainees and indicates who is speaking
/// </summary>
public class InstructorVoiceMonitorUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject monitorPanel;
    [SerializeField] private Transform participantListContent;
    [SerializeField] private GameObject participantItemPrefab;
    [SerializeField] private TMP_Text headerText;
    [SerializeField] private TMP_Text statusText;

    [Header("Speaking Indicator Colors")]
    [SerializeField] private Color speakingColor = new Color(0.2f, 1f, 0.2f); // Bright green
    [SerializeField] private Color notSpeakingColor = new Color(0.5f, 0.5f, 0.5f); // Gray

    [Header("Update Settings")]
    [SerializeField] private float refreshInterval = 2.0f;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;

    private Dictionary<string, GameObject> participantUIItems = new Dictionary<string, GameObject>();
    private Dictionary<string, bool> participantSpeakingState = new Dictionary<string, bool>();
    private float lastRefreshTime = 0f;
    private bool isInitialized = false;

    private void Start()
    {
        // Check if user is instructor
        string userRole = PlayerPrefs.GetString("Type_Of_User", "");
        
        if (userRole != "instructor")
        {
            // Hide panel if not instructor
            if (monitorPanel != null)
                monitorPanel.SetActive(false);
            
            if (showDebugLogs)
                Debug.Log("[InstructorVoiceMonitorUI] Not an instructor, hiding UI");
            return;
        }

        // Setup UI
        if (headerText != null)
        {
            headerText.text = "TRAINEE VOICE MONITOR";
        }

        UpdateStatusText("Connecting...");

        // Wait for GameVoiceChatManager to be ready
        if (GameVoiceChatManager.Instance != null)
        {
            SubscribeToEvents();
        }
        else
        {
            // Retry after a delay
            Invoke(nameof(RetryInitialization), 1f);
        }
    }

    private void RetryInitialization()
    {
        if (GameVoiceChatManager.Instance != null && !isInitialized)
        {
            SubscribeToEvents();
        }
        else if (GameVoiceChatManager.Instance == null)
        {
            // Try again
            Invoke(nameof(RetryInitialization), 1f);
        }
    }

    private void SubscribeToEvents()
    {
        if (isInitialized)
            return;

        // Subscribe to voice chat events
        GameVoiceChatManager.Instance.OnParticipantSpeakingChanged += OnParticipantSpeakingChanged;
        GameVoiceChatManager.Instance.OnVoiceChatReady += OnVoiceChatReady;

        isInitialized = true;

        if (showDebugLogs)
            Debug.Log("[InstructorVoiceMonitorUI] Subscribed to voice chat events");
    }

    private void Update()
    {
        if (!isInitialized)
            return;

        // Periodically refresh participant list
        if (Time.time - lastRefreshTime > refreshInterval)
        {
            RefreshParticipantList();
            lastRefreshTime = Time.time;
        }
    }

    private void OnVoiceChatReady()
    {
        UpdateStatusText("Voice monitor ready");
        RefreshParticipantList();
        
        if (showDebugLogs)
            Debug.Log("[InstructorVoiceMonitorUI] Voice chat ready");
    }

    private void RefreshParticipantList()
    {
        if (GameVoiceChatManager.Instance == null || !GameVoiceChatManager.Instance.IsReady())
            return;

        var participants = GameVoiceChatManager.Instance.GetParticipants();

        // Remove UI items for participants that left
        List<string> toRemove = new List<string>();
        foreach (var kvp in participantUIItems)
        {
            bool stillInChannel = false;
            foreach (var participant in participants)
            {
                if (participant.DisplayName == kvp.Key)
                {
                    stillInChannel = true;
                    break;
                }
            }

            if (!stillInChannel)
            {
                toRemove.Add(kvp.Key);
            }
        }

        foreach (var name in toRemove)
        {
            if (participantUIItems.ContainsKey(name))
            {
                Destroy(participantUIItems[name]);
                participantUIItems.Remove(name);
                participantSpeakingState.Remove(name);
            }
        }

        // Add UI items for new participants
        foreach (var participant in participants)
        {
            if (!participantUIItems.ContainsKey(participant.DisplayName))
            {
                CreateParticipantUIItem(participant.DisplayName);
            }
        }

        // Update status text
        int participantCount = participantUIItems.Count;
        UpdateStatusText(participantCount > 0 ? $"{participantCount} trainee(s) connected" : "Waiting for trainees...");
    }

    private void CreateParticipantUIItem(string participantName)
    {
        if (participantItemPrefab == null || participantListContent == null)
        {
            Debug.LogError("[InstructorVoiceMonitorUI] Participant item prefab or list content not assigned!");
            return;
        }

        GameObject item = Instantiate(participantItemPrefab, participantListContent);
        participantUIItems[participantName] = item;
        participantSpeakingState[participantName] = false;

        // Setup UI components
        TMP_Text nameText = item.GetComponentInChildren<TMP_Text>();
        if (nameText != null)
        {
            // Extract just the player name (remove role and ID suffixes)
            string displayName = ExtractPlayerName(participantName);
            nameText.text = displayName;
        }

        // Setup speaking indicator
        Image indicatorImage = item.GetComponent<Image>();
        if (indicatorImage != null)
        {
            indicatorImage.color = notSpeakingColor;
        }

        if (showDebugLogs)
            Debug.Log($"[InstructorVoiceMonitorUI] Created UI item for: {participantName}");
    }

    private void OnParticipantSpeakingChanged(string participantName, bool isSpeaking)
    {
        // Update speaking state cache
        participantSpeakingState[participantName] = isSpeaking;

        if (!participantUIItems.ContainsKey(participantName))
        {
            // Participant not in UI yet, trigger refresh
            RefreshParticipantList();
            return;
        }

        GameObject item = participantUIItems[participantName];
        if (item == null)
            return;

        // Update speaking indicator
        Image indicatorImage = item.GetComponent<Image>();
        if (indicatorImage != null)
        {
            indicatorImage.color = isSpeaking ? speakingColor : notSpeakingColor;
        }

        // Optional: Update text to show speaking status
        TMP_Text nameText = item.GetComponentInChildren<TMP_Text>();
        if (nameText != null)
        {
            string displayName = ExtractPlayerName(participantName);
            nameText.text = isSpeaking ? $"● {displayName}" : displayName;
        }
    }

    private string ExtractPlayerName(string fullName)
    {
        // Format is typically: "PlayerName_role_clientId"
        // Extract just the player name
        if (string.IsNullOrEmpty(fullName))
            return "Unknown";

        string[] parts = fullName.Split('_');
        return parts.Length > 0 ? parts[0] : fullName;
    }

    private void UpdateStatusText(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (GameVoiceChatManager.Instance != null)
        {
            GameVoiceChatManager.Instance.OnParticipantSpeakingChanged -= OnParticipantSpeakingChanged;
            GameVoiceChatManager.Instance.OnVoiceChatReady -= OnVoiceChatReady;
        }
    }
}
