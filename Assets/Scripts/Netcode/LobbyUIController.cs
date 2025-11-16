using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles UI interactions for the lobby system
/// Provides a clean interface for displaying lobby codes and connection status
/// </summary>
public class LobbyUIController : MonoBehaviour
{
    [Header("Instructor UI")]
    [SerializeField] private GameObject instructorPanel;
    [SerializeField] private Button generateCodeButton;
    [SerializeField] private Button startHostButton;
    [SerializeField] private TMP_Text lobbyCodeDisplay;
    [SerializeField] private Button copyCodeButton;

    [Header("Trainee UI")]
    [SerializeField] private GameObject traineePanel;
    [SerializeField] private TMP_InputField lobbyCodeInput;
    [SerializeField] private Button joinButton;

    [Header("Shared UI")]
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text playerCountText;
    [SerializeField] private Button disconnectButton;
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private GameObject connectedPanel;

    [Header("Settings")]
    [SerializeField] private bool autoDetectRole = true;

    private bool isInstructor = false;
    private string currentLobbyCode = "";

    private void Start()
    {
        InitializeUI();
        SetupButtonListeners();
        SubscribeToEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void InitializeUI()
    {
        // Detect user role
        if (autoDetectRole)
        {
            DetermineUserRole();
        }

        // Setup panels based on role
        if (instructorPanel != null)
            instructorPanel.SetActive(isInstructor);

        if (traineePanel != null)
            traineePanel.SetActive(!isInstructor);

        // Initially show lobby panel, hide connected panel
        if (lobbyPanel != null)
            lobbyPanel.SetActive(true);

        if (connectedPanel != null)
            connectedPanel.SetActive(false);

        // Disable host button until code is generated
        if (startHostButton != null && isInstructor)
            startHostButton.interactable = false;

        UpdateStatus(isInstructor ? "Instructor: Generate a lobby code to start" : "Trainee: Enter lobby code to join");
    }

    private void SetupButtonListeners()
    {
        // Instructor buttons
        if (generateCodeButton != null)
            generateCodeButton.onClick.AddListener(OnGenerateCodeClicked);

        if (startHostButton != null)
            startHostButton.onClick.AddListener(OnStartHostClicked);

        if (copyCodeButton != null)
            copyCodeButton.onClick.AddListener(OnCopyCodeClicked);

        // Trainee buttons
        if (joinButton != null)
            joinButton.onClick.AddListener(OnJoinClicked);

        // Shared buttons
        if (disconnectButton != null)
            disconnectButton.onClick.AddListener(OnDisconnectClicked);
    }

    private void SubscribeToEvents()
    {
        if (NetworkLobbyManager.Instance != null)
        {
            NetworkLobbyManager.Instance.OnLobbyCodeGenerated += HandleLobbyCodeGenerated;
            NetworkLobbyManager.Instance.OnLobbyJoinAttempt += HandleLobbyJoinAttempt;
            NetworkLobbyManager.Instance.OnPlayersCountChanged += HandlePlayersCountChanged;
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (NetworkLobbyManager.Instance != null)
        {
            NetworkLobbyManager.Instance.OnLobbyCodeGenerated -= HandleLobbyCodeGenerated;
            NetworkLobbyManager.Instance.OnLobbyJoinAttempt -= HandleLobbyJoinAttempt;
            NetworkLobbyManager.Instance.OnPlayersCountChanged -= HandlePlayersCountChanged;
        }
    }

    private void DetermineUserRole()
    {
        // Check RoomManager first
        if (RoomManager.Instance != null)
        {
            isInstructor = RoomManager.Instance.isInstructor;
        }
        else
        {
            // Fallback to PlayerPrefs
            string userType = PlayerPrefs.GetString("Type_Of_User", "trainee");
            isInstructor = userType.ToLower() == "instructor";
        }
    }

    // ========== Button Callbacks ==========

    private void OnGenerateCodeClicked()
    {
        if (NetworkLobbyManager.Instance != null)
        {
            string code = NetworkLobbyManager.Instance.GenerateLobbyCode();
            if (string.IsNullOrEmpty(code))
            {
                UpdateStatus("Failed to generate lobby code!");
            }
        }
        else
        {
            UpdateStatus("NetworkLobbyManager not found!");
        }
    }

    private void OnStartHostClicked()
    {
        if (NetworkLobbyManager.Instance != null)
        {
            bool success = NetworkLobbyManager.Instance.StartHostWithLobbyCode();
            if (success)
            {
                UpdateStatus("Starting host...");
                ShowConnectedUI();
            }
            else
            {
                UpdateStatus("Failed to start host!");
            }
        }
    }

    private void OnJoinClicked()
    {
        if (lobbyCodeInput == null)
            return;

        string code = lobbyCodeInput.text.Trim().ToUpper();

        if (string.IsNullOrEmpty(code))
        {
            UpdateStatus("Please enter a lobby code");
            return;
        }

        if (NetworkLobbyManager.Instance != null)
        {
            NetworkLobbyManager.Instance.JoinLobbyWithCode(code);
        }
    }

    private void OnCopyCodeClicked()
    {
        if (!string.IsNullOrEmpty(currentLobbyCode))
        {
            GUIUtility.systemCopyBuffer = currentLobbyCode;
            UpdateStatus($"Lobby code {currentLobbyCode} copied to clipboard!");
        }
    }

    private void OnDisconnectClicked()
    {
        if (NetworkLobbyManager.Instance != null)
        {
            NetworkLobbyManager.Instance.LeaveLobby();
            UpdateStatus("Disconnected from lobby");
            ShowLobbyUI();
        }
    }

    // ========== Event Handlers ==========

    private void HandleLobbyCodeGenerated(string code)
    {
        currentLobbyCode = code;

        if (lobbyCodeDisplay != null)
        {
            lobbyCodeDisplay.text = code;
        }

        if (startHostButton != null)
        {
            startHostButton.interactable = true;
        }

        UpdateStatus($"Lobby code generated: {code}");
    }

    private void HandleLobbyJoinAttempt(bool success, string message)
    {
        UpdateStatus(message);

        if (success)
        {
            ShowConnectedUI();
        }
    }

    private void HandlePlayersCountChanged(int count)
    {
        if (playerCountText != null)
        {
            playerCountText.text = $"Players: {count}";
        }
    }

    // ========== UI Helpers ==========

    private void UpdateStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        Debug.Log($"Lobby Status: {message}");
    }

    private void ShowLobbyUI()
    {
        if (lobbyPanel != null)
            lobbyPanel.SetActive(true);

        if (connectedPanel != null)
            connectedPanel.SetActive(false);

        // Reset UI state
        if (startHostButton != null && isInstructor)
            startHostButton.interactable = false;

        if (lobbyCodeDisplay != null && isInstructor)
            lobbyCodeDisplay.text = "---";

        currentLobbyCode = "";
    }

    private void ShowConnectedUI()
    {
        if (lobbyPanel != null)
            lobbyPanel.SetActive(false);

        if (connectedPanel != null)
            connectedPanel.SetActive(true);
    }

    // ========== Public Methods ==========

    public void SetInstructorMode(bool instructor)
    {
        isInstructor = instructor;
        autoDetectRole = false;

        if (instructorPanel != null)
            instructorPanel.SetActive(isInstructor);

        if (traineePanel != null)
            traineePanel.SetActive(!isInstructor);
    }

    public string GetCurrentLobbyCode()
    {
        return currentLobbyCode;
    }
}
