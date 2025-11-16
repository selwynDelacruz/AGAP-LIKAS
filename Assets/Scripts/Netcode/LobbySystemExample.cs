using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Example implementation showing how to use the Network Lobby System
/// This script demonstrates the complete flow for both instructor and trainee
/// </summary>
public class LobbySystemExample : MonoBehaviour
{
    [Header("Example UI")]
    [SerializeField] private Button exampleGenerateButton;
    [SerializeField] private Button exampleHostButton;
    [SerializeField] private TMP_InputField exampleCodeInput;
    [SerializeField] private Button exampleJoinButton;
    [SerializeField] private Button exampleDisconnectButton;
    [SerializeField] private TMP_Text exampleStatusText;

    private void Start()
    {
        SetupExampleButtons();
        SubscribeToLobbyEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromLobbyEvents();
    }

    private void SetupExampleButtons()
    {
        // Example: Generate lobby code button
        if (exampleGenerateButton != null)
        {
            exampleGenerateButton.onClick.AddListener(() =>
            {
                ExampleGenerateLobbyCode();
            });
        }

        // Example: Start host button
        if (exampleHostButton != null)
        {
            exampleHostButton.onClick.AddListener(() =>
            {
                ExampleStartHost();
            });
        }

        // Example: Join lobby button
        if (exampleJoinButton != null)
        {
            exampleJoinButton.onClick.AddListener(() =>
            {
                ExampleJoinLobby();
            });
        }

        // Example: Disconnect button
        if (exampleDisconnectButton != null)
        {
            exampleDisconnectButton.onClick.AddListener(() =>
            {
                ExampleDisconnect();
            });
        }
    }

    private void SubscribeToLobbyEvents()
    {
        if (NetworkLobbyManager.Instance != null)
        {
            // Subscribe to all lobby events
            NetworkLobbyManager.Instance.OnLobbyCodeGenerated += HandleCodeGenerated;
            NetworkLobbyManager.Instance.OnLobbyJoinAttempt += HandleJoinAttempt;
            NetworkLobbyManager.Instance.OnPlayersCountChanged += HandlePlayerCountChanged;
        }
    }

    private void UnsubscribeFromLobbyEvents()
    {
        if (NetworkLobbyManager.Instance != null)
        {
            NetworkLobbyManager.Instance.OnLobbyCodeGenerated -= HandleCodeGenerated;
            NetworkLobbyManager.Instance.OnLobbyJoinAttempt -= HandleJoinAttempt;
            NetworkLobbyManager.Instance.OnPlayersCountChanged -= HandlePlayerCountChanged;
        }
    }

    // ========== Example Methods ==========

    /// <summary>
    /// Example: How to generate a lobby code (Instructor only)
    /// </summary>
    private void ExampleGenerateLobbyCode()
    {
        if (NetworkLobbyManager.Instance == null)
        {
            UpdateStatus("Error: NetworkLobbyManager not found!");
            return;
        }

        // Generate the code
        string code = NetworkLobbyManager.Instance.GenerateLobbyCode();

        if (string.IsNullOrEmpty(code))
        {
            UpdateStatus("Failed to generate code. Are you an instructor?");
        }
        else
        {
            UpdateStatus($"Generated code: {code}");
            
            // The code can be shared with trainees
            // You could copy it to clipboard, display it, send it via network, etc.
            GUIUtility.systemCopyBuffer = code;
            Debug.Log($"Lobby code {code} copied to clipboard");
        }
    }

    /// <summary>
    /// Example: How to start hosting (Instructor only)
    /// </summary>
    private void ExampleStartHost()
    {
        if (NetworkLobbyManager.Instance == null)
        {
            UpdateStatus("Error: NetworkLobbyManager not found!");
            return;
        }

        // Start hosting with the previously generated code
        bool success = NetworkLobbyManager.Instance.StartHostWithLobbyCode();

        if (success)
        {
            UpdateStatus("Hosting started! Waiting for trainees...");
            
            // You can now transition to the game scene if needed
            // NetworkSceneManager.Instance.LoadNetworkScene("GameScene");
        }
        else
        {
            UpdateStatus("Failed to start hosting. Did you generate a code first?");
        }
    }

    /// <summary>
    /// Example: How to join a lobby (Trainee only)
    /// </summary>
    private void ExampleJoinLobby()
    {
        if (NetworkLobbyManager.Instance == null)
        {
            UpdateStatus("Error: NetworkLobbyManager not found!");
            return;
        }

        if (exampleCodeInput == null)
        {
            UpdateStatus("Error: Code input field not assigned!");
            return;
        }

        // Get the code from the input field
        string code = exampleCodeInput.text.Trim().ToUpper();

        if (string.IsNullOrEmpty(code))
        {
            UpdateStatus("Please enter a lobby code!");
            return;
        }

        // Attempt to join
        bool success = NetworkLobbyManager.Instance.JoinLobbyWithCode(code);

        if (success)
        {
            UpdateStatus($"Attempting to join lobby {code}...");
        }
        else
        {
            UpdateStatus("Failed to join lobby!");
        }
    }

    /// <summary>
    /// Example: How to disconnect/leave lobby
    /// </summary>
    private void ExampleDisconnect()
    {
        if (NetworkLobbyManager.Instance != null)
        {
            NetworkLobbyManager.Instance.LeaveLobby();
            UpdateStatus("Disconnected from lobby");
        }

        // Or use NetworkSceneManager for a complete return to lobby
        if (NetworkSceneManager.Instance != null)
        {
            NetworkSceneManager.Instance.ReturnToLobby();
        }
    }

    // ========== Event Handlers ==========

    private void HandleCodeGenerated(string code)
    {
        Debug.Log($"[Example] Lobby code generated: {code}");
        UpdateStatus($"Code generated: {code}");
        
        // Enable the host button now that we have a code
        if (exampleHostButton != null)
        {
            exampleHostButton.interactable = true;
        }
    }

    private void HandleJoinAttempt(bool success, string message)
    {
        Debug.Log($"[Example] Join attempt - Success: {success}, Message: {message}");
        UpdateStatus(message);
    }

    private void HandlePlayerCountChanged(int count)
    {
        Debug.Log($"[Example] Player count changed: {count}");
        UpdateStatus($"Players in lobby: {count}");
    }

    // ========== Utility Methods ==========

    private void UpdateStatus(string message)
    {
        if (exampleStatusText != null)
        {
            exampleStatusText.text = message;
        }
        Debug.Log($"[LobbyExample] {message}");
    }

    // ========== Advanced Examples ==========

    /// <summary>
    /// Example: Check if we're connected and get session info
    /// </summary>
    public void ExampleCheckConnection()
    {
        if (NetworkSceneManager.Instance != null)
        {
            var info = NetworkSceneManager.Instance.GetSessionInfo();
            Debug.Log($"Session Info: {info}");
            UpdateStatus(info.ToString());
        }
    }

    /// <summary>
    /// Example: Get current lobby code
    /// </summary>
    public string ExampleGetCurrentCode()
    {
        if (NetworkLobbyManager.Instance != null)
        {
            return NetworkLobbyManager.Instance.GetCurrentLobbyCode();
        }
        return null;
    }

    /// <summary>
    /// Example: Get player count
    /// </summary>
    public int ExampleGetPlayerCount()
    {
        if (NetworkLobbyManager.Instance != null)
        {
            return NetworkLobbyManager.Instance.GetConnectedPlayersCount();
        }
        return 0;
    }

    /// <summary>
    /// Example: Complete instructor flow
    /// </summary>
    public void ExampleInstructorFlow()
    {
        // 1. Generate code
        string code = NetworkLobbyManager.Instance.GenerateLobbyCode();
        Debug.Log($"Step 1: Generated code {code}");

        // 2. Start hosting
        bool hosted = NetworkLobbyManager.Instance.StartHostWithLobbyCode();
        Debug.Log($"Step 2: Hosting started: {hosted}");

        // 3. Wait for players (handled by events)
        // OnPlayersCountChanged will fire when trainees join

        // 4. When ready, load game scene
        // NetworkSceneManager.Instance.LoadNetworkScene("GameScene");
    }

    /// <summary>
    /// Example: Complete trainee flow
    /// </summary>
    public void ExampleTraineeFlow(string lobbyCode)
    {
        // 1. Join with code
        bool joined = NetworkLobbyManager.Instance.JoinLobbyWithCode(lobbyCode);
        Debug.Log($"Step 1: Join attempt: {joined}");

        // 2. Wait for connection (handled by events)
        // OnLobbyJoinAttempt will fire with success/failure

        // 3. Once connected, game scene will be loaded automatically when host loads it
    }
}
