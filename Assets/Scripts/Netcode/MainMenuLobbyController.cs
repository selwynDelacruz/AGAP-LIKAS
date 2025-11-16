using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles lobby code generation and joining in the Main Menu scene
/// After login, users can generate (instructor) or join (trainee) lobbies
/// Then transitions to the Lobby scene where game settings are configured
/// </summary>
public class MainMenuLobbyController : MonoBehaviour
{
    [Header("UI References - Instructor")]
    [SerializeField] private GameObject instructorLobbyPanel;
    [SerializeField] private Button generateCodeButton;
    [SerializeField] private TMP_Text lobbyCodeDisplay;
    [SerializeField] private Button continueToLobbyButton;

    [Header("UI References - Trainee")]
    [SerializeField] private GameObject traineeLobbyPanel;
    [SerializeField] private TMP_InputField lobbyCodeInput;
    [SerializeField] private Button joinLobbyButton;

    [Header("UI References - Shared")]
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text connectionStatusText;

    [Header("Scene Settings")]
    [SerializeField] private string lobbySceneName = "LobbyScene";

    private bool isInstructor = false;
    private bool isConnectedAndReady = false;
    private string currentLobbyCode = "";

    private void Start()
    {
        DetermineUserRole();
        SetupUI();
        SetupButtons();
        SubscribeToEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void DetermineUserRole()
    {
        // Get role from PlayerPrefs (set by AuthManager after login)
        string userType = PlayerPrefs.GetString("Type_Of_User", "");
        isInstructor = userType.ToLower() == "instructor";

        Debug.Log($"Main Menu: User role is {userType} (Instructor: {isInstructor})");
    }

    private void SetupUI()
    {
        // Show appropriate panel based on role
        if (instructorLobbyPanel != null)
            instructorLobbyPanel.SetActive(isInstructor);

        if (traineeLobbyPanel != null)
            traineeLobbyPanel.SetActive(!isInstructor);

        // Initially disable continue button
        if (continueToLobbyButton != null)
            continueToLobbyButton.interactable = false;

        UpdateStatus(isInstructor ? "Generate a lobby code to start" : "Enter lobby code to join");
    }

    private void SetupButtons()
    {
        // Instructor buttons
        if (generateCodeButton != null)
        {
            generateCodeButton.onClick.AddListener(OnGenerateCodeClicked);
        }

        if (continueToLobbyButton != null)
        {
            continueToLobbyButton.onClick.AddListener(OnContinueToLobbyClicked);
        }

        // Trainee buttons
        if (joinLobbyButton != null)
        {
            joinLobbyButton.onClick.AddListener(OnJoinLobbyClicked);
        }
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

    // ========== Button Handlers ==========

    private void OnGenerateCodeClicked()
    {
        if (!isInstructor)
        {
            UpdateStatus("Only instructors can generate lobby codes!");
            return;
        }

        if (NetworkLobbyManager.Instance == null)
        {
            UpdateStatus("Network system not ready!");
            return;
        }

        // Generate lobby code
        string code = NetworkLobbyManager.Instance.GenerateLobbyCode();
        
        if (!string.IsNullOrEmpty(code))
        {
            currentLobbyCode = code;
            
            // Start hosting immediately
            bool success = NetworkLobbyManager.Instance.StartHostWithLobbyCode();
            
            if (success)
            {
                // Get user data from AuthManager
                if (AuthManager.Instance != null)
                {
                    PlayerPrefs.SetString("PlayerName", AuthManager.Instance.Current_Name);
                    PlayerPrefs.SetString("PlayerUsername", AuthManager.Instance.Current_Username);
                    PlayerPrefs.Save();
                }

                UpdateStatus($"Lobby created! Code: {code}");
                isConnectedAndReady = true;
                
                // Enable continue button
                if (continueToLobbyButton != null)
                    continueToLobbyButton.interactable = true;
            }
            else
            {
                UpdateStatus("Failed to start hosting!");
            }
        }
    }

    private void OnJoinLobbyClicked()
    {
        if (isInstructor)
        {
            UpdateStatus("Instructors should create lobbies, not join them!");
            return;
        }

        if (lobbyCodeInput == null || string.IsNullOrEmpty(lobbyCodeInput.text))
        {
            UpdateStatus("Please enter a lobby code!");
            return;
        }

        if (NetworkLobbyManager.Instance == null)
        {
            UpdateStatus("Network system not ready!");
            return;
        }

        string code = lobbyCodeInput.text.Trim().ToUpper();
        currentLobbyCode = code;

        // Get user data from AuthManager before joining
        if (AuthManager.Instance != null)
        {
            PlayerPrefs.SetString("PlayerName", AuthManager.Instance.Current_Name);
            PlayerPrefs.SetString("PlayerUsername", AuthManager.Instance.Current_Username);
            PlayerPrefs.Save();
        }

        // Attempt to join
        NetworkLobbyManager.Instance.JoinLobbyWithCode(code);
        UpdateStatus($"Connecting to lobby {code}...");
    }

    private void OnContinueToLobbyClicked()
    {
        if (!isConnectedAndReady)
        {
            UpdateStatus("You must create/join a lobby first!");
            return;
        }

        // Store that we're transitioning to lobby scene
        PlayerPrefs.SetString("TransitioningToLobby", "true");
        PlayerPrefs.Save();

        // Load the lobby scene
        Debug.Log($"Loading lobby scene: {lobbySceneName}");
        SceneManager.LoadScene(lobbySceneName);
    }

    // ========== Event Handlers ==========

    private void HandleLobbyCodeGenerated(string code)
    {
        if (lobbyCodeDisplay != null)
        {
            lobbyCodeDisplay.text = $"Code: {code}";
        }

        UpdateStatus($"Lobby code: {code}");
    }

    private void HandleLobbyJoinAttempt(bool success, string message)
    {
        UpdateStatus(message);

        if (success)
        {
            // Successfully connected as trainee
            isConnectedAndReady = true;
            
            // Auto-transition to lobby scene after short delay
            Invoke(nameof(OnContinueToLobbyClicked), 1.5f);
        }
        else
        {
            UpdateStatus($"Failed to join: {message}");
        }
    }

    private void HandlePlayersCountChanged(int count)
    {
        if (connectionStatusText != null)
        {
            connectionStatusText.text = $"Connected Players: {count}";
        }
    }

    // ========== Utility Methods ==========

    private void UpdateStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        Debug.Log($"[MainMenuLobby] {message}");
    }

    // ========== Public Methods ==========

    /// <summary>
    /// Call this after user logs in to show the lobby code panel
    /// </summary>
    public void ShowLobbyCodePanel()
    {
        DetermineUserRole();
        SetupUI();
    }

    /// <summary>
    /// Copy lobby code to clipboard
    /// </summary>
    public void CopyLobbyCodeToClipboard()
    {
        if (!string.IsNullOrEmpty(currentLobbyCode))
        {
            GUIUtility.systemCopyBuffer = currentLobbyCode;
            UpdateStatus($"Code {currentLobbyCode} copied!");
        }
    }
}
