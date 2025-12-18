using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

#if METAVC_NGO
using MetaVoiceChat;
using MetaVoiceChat.NetProviders.NGO;
#endif

/// <summary>
/// Voice control UI for Instructors.
/// Attach this to a GameObject inside the instructorUIPanel.
/// Shows mic status, mute button, and who is currently speaking.
/// </summary>
public class InstructorVoiceControlUI : MonoBehaviour
{
    [Header("Microphone Controls")]
    [Tooltip("Button to toggle instructor's microphone")]
    [SerializeField] private Button micToggleButton;
    
    [Tooltip("Text showing microphone status")]
    [SerializeField] private TextMeshProUGUI micStatusText;
    
    [Tooltip("Optional: Mic icon image")]
    [SerializeField] private Image micIcon;

    [Header("Speaking Indicator (Simple Text)")]
    [Tooltip("Single text field showing all users and their speaking status")]
    [SerializeField] private TextMeshProUGUI speakingListText;

    [Header("Visual Settings")]
    [SerializeField] private Color micOnColor = Color.green;
    [SerializeField] private Color micOffColor = Color.red;
    [SerializeField] private Color speakingColor = Color.cyan;
    [SerializeField] private Color mutedColor = Color.gray;

    [Header("Icons (Optional)")]
    [SerializeField] private Sprite micOnSprite;
    [SerializeField] private Sprite micOffSprite;

    [Header("Performance Settings")]
    [Tooltip("How often to update speaking list when VoiceChatManager is unavailable (seconds)")]
    [SerializeField] private float fallbackUpdateInterval = 0.5f;

#if METAVC_NGO
    private MetaVc _localMetaVc;
#endif
    private bool _isMuted = false;
    private float _lastFallbackUpdate = 0f;

    private void Start()
    {
        // Setup button listener
        if (micToggleButton != null)
        {
            micToggleButton.onClick.AddListener(ToggleMic);
        }

        // Find local MetaVc
        Invoke(nameof(FindLocalMetaVc), 0.5f);

        // Subscribe to VoiceChatManager events if available
        if (VoiceChatManager.Instance != null)
        {
            VoiceChatManager.Instance.OnUserSpeakingChanged += OnUserSpeakingChanged;
            VoiceChatManager.Instance.OnUserRegistered += OnUserRegistered;
            VoiceChatManager.Instance.OnUserUnregistered += OnUserUnregistered;
        }
        else
        {
            // Retry subscription
            Invoke(nameof(SubscribeToVoiceChatManager), 1f);
        }

        UpdateMicUI();
        UpdateSpeakingList();
    }

    private void Update()
    {
        // Update speaking list periodically
        if (VoiceChatManager.Instance == null)
        {
            // Fallback: use interval to avoid expensive FindObjectsOfType every frame
            if (Time.time - _lastFallbackUpdate >= fallbackUpdateInterval)
            {
                _lastFallbackUpdate = Time.time;
                UpdateSpeakingListFromMetaVc();
            }
        }
        else
        {
            // With VoiceChatManager, update every frame (lightweight)
            UpdateSpeakingList();
        }
    }

    private void SubscribeToVoiceChatManager()
    {
        if (VoiceChatManager.Instance != null)
        {
            VoiceChatManager.Instance.OnUserSpeakingChanged += OnUserSpeakingChanged;
            VoiceChatManager.Instance.OnUserRegistered += OnUserRegistered;
            VoiceChatManager.Instance.OnUserUnregistered += OnUserUnregistered;
        }
    }

    private void FindLocalMetaVc()
    {
#if METAVC_NGO
        var allMetaVc = FindObjectsOfType<MetaVc>();

        foreach (var vc in allMetaVc)
        {
            var netProvider = vc.GetComponent<NGONetProvider>();
            if (netProvider != null && netProvider.IsOwner)
            {
                _localMetaVc = vc;
                
                // Subscribe to changes
                _localMetaVc.isInputMuted.OnValueChanged += OnMuteChanged;
                
                // Get initial state
                _isMuted = _localMetaVc.isInputMuted.Value;
                
                Debug.Log("[InstructorVoiceControlUI] Found local MetaVc (Instructor)");
                UpdateMicUI();
                return;
            }
        }

        Debug.LogWarning("[InstructorVoiceControlUI] Could not find local MetaVc. Retrying...");
        Invoke(nameof(FindLocalMetaVc), 1f);
#else
        Debug.LogWarning("[InstructorVoiceControlUI] METAVC_NGO not defined. Voice chat disabled.");
#endif
    }

    private void OnMuteChanged(bool muted)
    {
        _isMuted = muted;
        UpdateMicUI();
    }

    /// <summary>
    /// Toggle instructor's microphone (called by button)
    /// </summary>
    public void ToggleMic()
    {
#if METAVC_NGO
        if (_localMetaVc == null) return;

        _isMuted = !_isMuted;
        _localMetaVc.isInputMuted.Value = _isMuted;
        
        Debug.Log($"[InstructorVoiceControlUI] Instructor mic toggled: {(_isMuted ? "OFF" : "ON")}");
        UpdateMicUI();
#endif
    }

    /// <summary>
    /// Set mic state directly
    /// </summary>
    public void SetMicMuted(bool muted)
    {
#if METAVC_NGO
        if (_localMetaVc == null) return;

        _isMuted = muted;
        _localMetaVc.isInputMuted.Value = muted;
        UpdateMicUI();
#endif
    }

    private void UpdateMicUI()
    {
        // Update status text
        if (micStatusText != null)
        {
            micStatusText.text = _isMuted ? "Mic: OFF" : "Mic: ON";
            micStatusText.color = _isMuted ? micOffColor : micOnColor;
        }

        // Update icon color
        if (micIcon != null)
        {
            micIcon.color = _isMuted ? micOffColor : micOnColor;

            // Update sprite if available
            if (micOnSprite != null && micOffSprite != null)
            {
                micIcon.sprite = _isMuted ? micOffSprite : micOnSprite;
            }
        }
    }

    #region Speaking Indicators

    private void OnUserSpeakingChanged(ulong clientId, bool isSpeaking)
    {
        UpdateSpeakingList();
    }

    private void OnUserRegistered(ulong clientId, VoiceChatManager.VoiceChatUser user)
    {
        UpdateSpeakingList();
    }

    private void OnUserUnregistered(ulong clientId)
    {
        UpdateSpeakingList();
    }

    private void UpdateSpeakingList()
    {
        if (VoiceChatManager.Instance == null || speakingListText == null) return;

        var userLines = new List<string>();

        // Get all users and their speaking status
        foreach (var user in VoiceChatManager.Instance.GetAllUsers())
        {
            // Skip instructor themselves
            if (user.IsLocalPlayer) continue;

            // Get role initial (T for Trainee, I for Instructor)
            string roleInitial = user.Role == "instructor" ? "I" : "T";
            
            // Determine speaking status
            string status = user.IsSpeaking && !user.IsMuted ? "Speaking" : "Muted";
            
            // Format: T: Username - Speaking
            userLines.Add($"{roleInitial}: {user.Username} - {status}");
        }

        // Update text
        if (userLines.Count > 0)
        {
            speakingListText.text = string.Join("\n", userLines);
            speakingListText.color = speakingColor;
        }
        else
        {
            speakingListText.text = "No other users";
            speakingListText.color = mutedColor;
        }
    }

    /// <summary>
    /// Fallback method when VoiceChatManager isn't available
    /// Directly scans MetaVc instances
    /// </summary>
    private void UpdateSpeakingListFromMetaVc()
    {
#if METAVC_NGO
        if (speakingListText == null) return;

        var allMetaVc = FindObjectsOfType<MetaVc>();
        var userLines = new List<string>();

        foreach (var vc in allMetaVc)
        {
            if (vc == _localMetaVc) continue; // Skip instructor

            var netProvider = vc.GetComponent<NGONetProvider>();
            if (netProvider != null)
            {
                // Assume trainees for fallback
                string roleInitial = "T";
                string username = $"Trainee {netProvider.OwnerClientId}";
                
                // Determine speaking status
                string status = vc.isSpeaking.Value && !vc.isInputMuted.Value ? "Speaking" : "Muted";
                
                // Format: T: Username - Speaking
                userLines.Add($"{roleInitial}: {username} - {status}");
            }
        }

        // Update text
        if (userLines.Count > 0)
        {
            speakingListText.text = string.Join("\n", userLines);
            speakingListText.color = speakingColor;
        }
        else
        {
            speakingListText.text = "No other users";
            speakingListText.color = mutedColor;
        }
#endif
    }

    #endregion

    private void OnDestroy()
    {
#if METAVC_NGO
        if (_localMetaVc != null)
        {
            _localMetaVc.isInputMuted.OnValueChanged -= OnMuteChanged;
        }
#endif

        if (VoiceChatManager.Instance != null)
        {
            VoiceChatManager.Instance.OnUserSpeakingChanged -= OnUserSpeakingChanged;
            VoiceChatManager.Instance.OnUserRegistered -= OnUserRegistered;
            VoiceChatManager.Instance.OnUserUnregistered -= OnUserUnregistered;
        }
    }

    /// <summary>
    /// Check if instructor mic is muted
    /// </summary>
    public bool IsMuted => _isMuted;
}
