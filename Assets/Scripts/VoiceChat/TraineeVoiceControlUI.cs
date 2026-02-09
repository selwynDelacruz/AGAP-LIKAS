using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if METAVC_NGO
using MetaVoiceChat;
using MetaVoiceChat.NetProviders.NGO;
#endif

/// <summary>
/// Voice control UI for Trainees.
/// Attach this to a GameObject inside the traineeUIPanel.
/// Provides mute/unmute toggle and push-to-talk functionality.
/// </summary>
public class TraineeVoiceControlUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Button to toggle mute/unmute")]
    [SerializeField] private Button muteToggleButton;
    
    [Tooltip("Text showing current mute state")]
    [SerializeField] private TextMeshProUGUI muteStatusText;
    
    [Tooltip("Optional: Image for mic icon")]
    [SerializeField] private Image micIcon;

    [Header("Push-to-Talk Settings")]
    [Tooltip("Enable push-to-talk mode (hold key to speak)")]
    [SerializeField] private bool usePushToTalk = false;
    
    [Tooltip("Key to hold for push-to-talk")]
    [SerializeField] private KeyCode pushToTalkKey = KeyCode.V;
    
    [Tooltip("Optional: Toggle button to switch between PTT and always-on")]
    [SerializeField] private Button pttModeToggleButton;
    
    [Tooltip("Text showing PTT mode status")]
    [SerializeField] private TextMeshProUGUI pttModeText;

    [Header("Cursor Toggle Settings")]
    [Tooltip("Key to toggle cursor visibility (default: C)")]
    [SerializeField] private KeyCode cursorToggleKey = KeyCode.C;
    
    [Tooltip("Show cursor hint text")]
    [SerializeField] private TextMeshProUGUI cursorHintText;

    [Header("Visual Settings")]
    [SerializeField] private Color micOnColor = Color.green;
    [SerializeField] private Color micOffColor = Color.red;
    [SerializeField] private Color pttActiveColor = Color.yellow;

    [Header("Icons (Optional)")]
    [SerializeField] private Sprite micOnSprite;
    [SerializeField] private Sprite micOffSprite;

#if METAVC_NGO
    private MetaVc _localMetaVc;
#endif
    private bool _isMuted = false;
    private bool _isPTTActive = false; // Is the PTT key currently held
    private bool _isCursorVisible = false;

    private void Start()
    {
        // Setup button listeners
        if (muteToggleButton != null)
        {
            muteToggleButton.onClick.AddListener(ToggleMute);
        }

        if (pttModeToggleButton != null)
        {
            pttModeToggleButton.onClick.AddListener(TogglePTTMode);
        }

        // Find local player's MetaVc after a short delay (wait for spawn)
        Invoke(nameof(FindLocalMetaVc), 0.5f);

        UpdateUI();
        UpdateCursorHint();
    }

    private void Update()
    {
#if METAVC_NGO
        // Handle Push-to-Talk input
        if (usePushToTalk && _localMetaVc != null)
        {
            bool keyHeld = Input.GetKey(pushToTalkKey);

            if (keyHeld != _isPTTActive)
            {
                _isPTTActive = keyHeld;
                
                // In PTT mode: unmuted when key held, muted when released
                _localMetaVc.isInputMuted.Value = !keyHeld;
                
                UpdateUI();
            }
        }
#endif

        // Handle cursor toggle input
        if (Input.GetKeyDown(cursorToggleKey))
        {
            ToggleCursor();
        }
    }

    private void FindLocalMetaVc()
    {
#if METAVC_NGO
        // Find all MetaVc instances and get the local player's one
        var allMetaVc = FindObjectsOfType<MetaVc>();

        foreach (var vc in allMetaVc)
        {
            var netProvider = vc.GetComponent<NGONetProvider>();
            if (netProvider != null && netProvider.IsOwner)
            {
                _localMetaVc = vc;
                
                // Subscribe to mute changes
                _localMetaVc.isInputMuted.OnValueChanged += OnMuteChanged;
                _localMetaVc.isSpeaking.OnValueChanged += OnSpeakingChanged;
                
                // Get initial state
                _isMuted = _localMetaVc.isInputMuted.Value;
                
                // If using PTT, start muted
                if (usePushToTalk)
                {
                    _localMetaVc.isInputMuted.Value = true;
                    _isMuted = true;
                }
                
                Debug.Log("[TraineeVoiceControlUI] Found local MetaVc");
                UpdateUI();
                return;
            }
        }

        Debug.LogWarning("[TraineeVoiceControlUI] Could not find local MetaVc. Retrying...");
        Invoke(nameof(FindLocalMetaVc), 1f);
#else
        Debug.LogWarning("[TraineeVoiceControlUI] METAVC_NGO not defined. Voice chat disabled.");
#endif
    }

    private void OnMuteChanged(bool muted)
    {
        if (!usePushToTalk)
        {
            _isMuted = muted;
            UpdateUI();
        }
    }

    private void OnSpeakingChanged(bool speaking)
    {
        // Optional: Add visual feedback when speaking
        UpdateUI();
    }

    /// <summary>
    /// Toggle cursor visibility (called by pressing C key)
    /// </summary>
    public void ToggleCursor()
    {
        _isCursorVisible = !_isCursorVisible;
        
        Cursor.visible = _isCursorVisible;
        Cursor.lockState = _isCursorVisible ? CursorLockMode.None : CursorLockMode.Locked;
        
        Debug.Log($"[TraineeVoiceControlUI] Cursor toggled: {(_isCursorVisible ? "VISIBLE" : "HIDDEN")}");
        
        UpdateCursorHint();
    }

    /// <summary>
    /// Set cursor visibility directly
    /// </summary>
    public void SetCursorVisible(bool visible)
    {
        _isCursorVisible = visible;
        Cursor.visible = visible;
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
        
        UpdateCursorHint();
    }

    private void UpdateCursorHint()
    {
        if (cursorHintText != null)
        {
            cursorHintText.text = _isCursorVisible 
                ? $"Press [{cursorToggleKey}] to hide cursor" 
                : $"Press [{cursorToggleKey}] to show cursor";
        }
    }

    /// <summary>
    /// Toggle mute state (called by button)
    /// </summary>
    public void ToggleMute()
    {
#if METAVC_NGO
        if (_localMetaVc == null) return;

        // In PTT mode, this button toggles PTT on/off
        if (usePushToTalk)
        {
            TogglePTTMode();
            return;
        }

        _isMuted = !_isMuted;
        _localMetaVc.isInputMuted.Value = _isMuted;
        
        Debug.Log($"[TraineeVoiceControlUI] Mute toggled: {_isMuted}");
        UpdateUI();
#endif
    }

    /// <summary>
    /// Toggle between Push-to-Talk and always-on mode
    /// </summary>
    public void TogglePTTMode()
    {
        usePushToTalk = !usePushToTalk;

#if METAVC_NGO
        if (_localMetaVc != null)
        {
            if (usePushToTalk)
            {
                // Switching to PTT: start muted (need to hold key to talk)
                _localMetaVc.isInputMuted.Value = true;
                _isMuted = true;
            }
            else
            {
                // Switching to always-on: unmute
                _localMetaVc.isInputMuted.Value = false;
                _isMuted = false;
            }
        }
#endif

        Debug.Log($"[TraineeVoiceControlUI] PTT Mode: {(usePushToTalk ? "ON" : "OFF")}");
        UpdateUI();
    }

    /// <summary>
    /// Set mute state directly
    /// </summary>
    public void SetMuted(bool muted)
    {
#if METAVC_NGO
        if (_localMetaVc == null) return;

        _isMuted = muted;
        _localMetaVc.isInputMuted.Value = muted;
        UpdateUI();
#endif
    }

    private void UpdateUI()
    {
        // Determine current state
        bool isCurrentlyMuted = usePushToTalk ? !_isPTTActive : _isMuted;
        
#if METAVC_NGO
        bool isSpeaking = _localMetaVc?.isSpeaking.Value ?? false;
#else
        bool isSpeaking = false;
#endif

        // Update mute status text
        if (muteStatusText != null)
        {
            if (usePushToTalk)
            {
                muteStatusText.text = _isPTTActive ? "Speaking..." : $"Hold [{pushToTalkKey}] to Talk";
            }
            else
            {
                muteStatusText.text = isCurrentlyMuted ? "Mic: OFF" : "Mic: ON";
            }
        }

        // Update mic icon color
        if (micIcon != null)
        {
            if (usePushToTalk && _isPTTActive)
            {
                micIcon.color = pttActiveColor;
            }
            else if (isCurrentlyMuted)
            {
                micIcon.color = micOffColor;
            }
            else
            {
                micIcon.color = isSpeaking ? pttActiveColor : micOnColor;
            }

            // Update sprite if available
            if (micOnSprite != null && micOffSprite != null)
            {
                micIcon.sprite = isCurrentlyMuted ? micOffSprite : micOnSprite;
            }
        }

        // Update PTT mode text
        if (pttModeText != null)
        {
            pttModeText.text = usePushToTalk ? "Mode: Push-to-Talk" : "Mode: Always On";
        }
    }

    private void OnDestroy()
    {
#if METAVC_NGO
        if (_localMetaVc != null)
        {
            _localMetaVc.isInputMuted.OnValueChanged -= OnMuteChanged;
            _localMetaVc.isSpeaking.OnValueChanged -= OnSpeakingChanged;
        }
#endif
    }

    /// <summary>
    /// Check if cursor is currently visible
    /// </summary>
    public bool IsCursorVisible => _isCursorVisible;

    /// <summary>
    /// Check if currently using PTT mode
    /// </summary>
    public bool IsPushToTalkEnabled => usePushToTalk;

    /// <summary>
    /// Check if currently muted
    /// </summary>
    public bool IsMuted => usePushToTalk ? !_isPTTActive : _isMuted;
}
