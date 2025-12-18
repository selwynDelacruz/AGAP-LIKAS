using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI Controller for trainee voice chat controls
/// Shows mute button, volume slider, and keyboard shortcuts
/// </summary>
public class TraineeVoiceChatUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject voiceChatPanel;
    [SerializeField] private Button muteButton;
    [SerializeField] private TMP_Text muteButtonText;
    [SerializeField] private Image muteButtonImage;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TMP_Text volumeText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text keyboardHintText;

    [Header("Mute Button Colors")]
    [SerializeField] private Color unmutedColor = new Color(0.2f, 0.8f, 0.2f); // Green
    [SerializeField] private Color mutedColor = new Color(0.9f, 0.2f, 0.2f); // Red

    [Header("Icons (Optional)")]
    [SerializeField] private Sprite micOnIcon;
    [SerializeField] private Sprite micOffIcon;
    [SerializeField] private Image micIconImage;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;

    private bool isInitialized = false;

    private void Start()
    {
        // Check if user is trainee
        string userRole = PlayerPrefs.GetString("Type_Of_User", "");
        
        if (userRole != "trainee")
        {
            // Hide panel if not trainee
            if (voiceChatPanel != null)
                voiceChatPanel.SetActive(false);
            
            if (showDebugLogs)
                Debug.Log("[TraineeVoiceChatUI] Not a trainee, hiding UI");
            return;
        }

        // Setup UI
        SetupUI();

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
        GameVoiceChatManager.Instance.OnMuteStateChanged += OnMuteStateChanged;
        GameVoiceChatManager.Instance.OnVolumeChanged += OnVolumeChanged;
        GameVoiceChatManager.Instance.OnVoiceChatReady += OnVoiceChatReady;

        isInitialized = true;

        if (showDebugLogs)
            Debug.Log("[TraineeVoiceChatUI] Subscribed to voice chat events");

        // Initialize UI state
        UpdateMuteButton(false);
        UpdateVolumeDisplay(50);
        UpdateStatus("Connecting...");
    }

    private void SetupUI()
    {
        // Setup mute button
        if (muteButton != null)
        {
            muteButton.onClick.AddListener(OnMuteButtonClicked);
        }

        // Setup volume slider
        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0;
            volumeSlider.maxValue = 100;
            volumeSlider.value = 50;
            volumeSlider.onValueChanged.AddListener(OnVolumeSliderChanged);
        }

        // Setup keyboard hints
        if (keyboardHintText != null)
        {
            keyboardHintText.text = "M: Mute | -/+: Volume";
        }

        if (showDebugLogs)
            Debug.Log("[TraineeVoiceChatUI] UI setup complete");
    }

    private void OnMuteButtonClicked()
    {
        if (GameVoiceChatManager.Instance != null)
        {
            GameVoiceChatManager.Instance.ToggleMute();
        }
    }

    private void OnVolumeSliderChanged(float value)
    {
        if (GameVoiceChatManager.Instance != null)
        {
            // Use the synchronous SetVolume method (which calls async internally)
            GameVoiceChatManager.Instance.SetVolume((int)value);
        }
    }

    private void OnMuteStateChanged(bool isMuted)
    {
        UpdateMuteButton(isMuted);
    }

    private void OnVolumeChanged(int volume)
    {
        UpdateVolumeDisplay(volume);
        
        // Update slider without triggering event
        if (volumeSlider != null)
        {
            volumeSlider.SetValueWithoutNotify(volume);
        }
    }

    private void OnVoiceChatReady()
    {
        UpdateStatus("Voice chat ready");
        
        if (showDebugLogs)
            Debug.Log("[TraineeVoiceChatUI] Voice chat ready");
    }

    private void UpdateMuteButton(bool isMuted)
    {
        if (muteButtonText != null)
        {
            muteButtonText.text = isMuted ? "UNMUTE (M)" : "MUTE (M)";
        }

        if (muteButtonImage != null)
        {
            muteButtonImage.color = isMuted ? mutedColor : unmutedColor;
        }

        if (micIconImage != null && micOnIcon != null && micOffIcon != null)
        {
            micIconImage.sprite = isMuted ? micOffIcon : micOnIcon;
        }

        UpdateStatus(isMuted ? "Microphone muted" : "Microphone active");
    }

    private void UpdateVolumeDisplay(int volume)
    {
        if (volumeText != null)
        {
            volumeText.text = $"{volume}%";
        }
    }

    private void UpdateStatus(string message)
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
            GameVoiceChatManager.Instance.OnMuteStateChanged -= OnMuteStateChanged;
            GameVoiceChatManager.Instance.OnVolumeChanged -= OnVolumeChanged;
            GameVoiceChatManager.Instance.OnVoiceChatReady -= OnVoiceChatReady;
        }
    }
}
