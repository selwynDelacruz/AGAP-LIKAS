using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using System.Collections.Generic;

/// <summary>
/// Manages the Lobby scene where instructor configures game settings
/// and all players can see who's connected
/// Only instructor can modify settings and start the game
/// </summary>
public class LobbySceneManager : MonoBehaviour
{
    [Header("Game Settings UI - Instructor Only")]
    [SerializeField] private GameObject gameSettingsPanel;
    [SerializeField] private Button minusTaskButton;
    [SerializeField] private Button addTaskButton;
    [SerializeField] private TMP_Text taskCountText;
    [SerializeField] private TMP_Dropdown disasterDropdown;
    [SerializeField] private TMP_Dropdown durationDropdown;
    [SerializeField] private Button startGameButton;

    [Header("Player Display UI - Fixed Slots")]
    [SerializeField] private TMP_InputField instructorNameField;
    [SerializeField] private TMP_InputField trainee1NameField;
    [SerializeField] private TMP_InputField trainee2NameField;
    [SerializeField] private TMP_Text lobbyCodeDisplay;

    [Header("Status Display")]
    [SerializeField] private TMP_Text statusText;

    [Header("Game Scene Names")]
    [SerializeField] private string earthquakeSceneName = "Earthquake";
    [SerializeField] private string floodSceneName = "Flood";
    [SerializeField] private string testSceneName = "TestKen";

    // Game settings
    private int taskCount = 1;
    private const int MIN_TASKS = 0;
    private const int MAX_TASKS = 8;
    private readonly int[] durations = { 300, 480, 600 }; // 5, 8, 10 minutes

    private bool isInstructor = false;
    
    // Track connected players (max 1 instructor + 2 trainees = 3 total)
    private string instructorName = "";
    private string trainee1Name = "";
    private string trainee2Name = "";
    private int traineeCount = 0;

    private void Start()
    {
        DetermineUserRole();
        SetupUI();
        SetupGameSettings();
        SubscribeToEvents();
        DisplayLobbyCode();
        ClearPlayerSlots();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void DetermineUserRole()
    {
        string userType = PlayerPrefs.GetString("Type_Of_User", "");
        isInstructor = userType.ToLower() == "instructor";
        
        Debug.Log($"Lobby Scene: User is {userType} (Instructor: {isInstructor})");
    }

    private void SetupUI()
    {
        // Only instructor sees game settings
        if (gameSettingsPanel != null)
        {
            gameSettingsPanel.SetActive(isInstructor);
        }

        // Setup game settings buttons for instructor
        if (isInstructor)
        {
            if (minusTaskButton != null)
                minusTaskButton.onClick.AddListener(DecreaseTaskCount);

            if (addTaskButton != null)
                addTaskButton.onClick.AddListener(IncreaseTaskCount);

            if (startGameButton != null)
                startGameButton.onClick.AddListener(OnStartGameClicked);

            if (disasterDropdown != null)
                disasterDropdown.onValueChanged.AddListener(OnDisasterChanged);

            if (durationDropdown != null)
                durationDropdown.onValueChanged.AddListener(OnDurationChanged);
        }
        else
        {
            UpdateStatus("Waiting for instructor to start the game...");
        }
    }

    private void SetupGameSettings()
    {
        taskCount = 1;
        UpdateTaskCountDisplay();
    }

    private void SubscribeToEvents()
    {
        if (NetworkLobbyManager.Instance != null)
        {
            NetworkLobbyManager.Instance.OnPlayersCountChanged += HandlePlayersCountChanged;
        }

        // Subscribe to Netcode events
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (NetworkLobbyManager.Instance != null)
        {
            NetworkLobbyManager.Instance.OnPlayersCountChanged -= HandlePlayersCountChanged;
        }

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    private void DisplayLobbyCode()
    {
        if (NetworkLobbyManager.Instance != null && lobbyCodeDisplay != null)
        {
            string code = NetworkLobbyManager.Instance.GetCurrentLobbyCode();
            if (!string.IsNullOrEmpty(code))
            {
                lobbyCodeDisplay.text = $"Lobby Code: {code}";
            }
        }
    }

    // ========== Game Settings Methods ==========

    private void DecreaseTaskCount()
    {
        if (taskCount > MIN_TASKS)
        {
            taskCount--;
            UpdateTaskCountDisplay();
        }
    }

    private void IncreaseTaskCount()
    {
        if (taskCount < MAX_TASKS)
        {
            taskCount++;
            UpdateTaskCountDisplay();
        }
    }

    private void UpdateTaskCountDisplay()
    {
        if (taskCountText != null)
        {
            taskCountText.text = taskCount.ToString();
        }
    }

    private void OnDisasterChanged(int index)
    {
        // Just track the selection, will use when starting game
        Debug.Log($"Disaster changed to index: {index}");
    }

    private void OnDurationChanged(int index)
    {
        // Just track the selection, will use when starting game
        Debug.Log($"Duration changed to index: {index}");
    }

    private void OnStartGameClicked()
    {
        if (!isInstructor)
        {
            UpdateStatus("Only instructor can start the game!");
            return;
        }

        if (!NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsHost)
        {
            UpdateStatus("You must be hosting to start the game!");
            return;
        }

        // Save game settings to PlayerPrefs
        PlayerPrefs.SetInt("TaskCount", taskCount);
        
        int selectedDuration = (durationDropdown != null && durationDropdown.value < durations.Length) 
            ? durations[durationDropdown.value] 
            : 300;
        PlayerPrefs.SetInt("GameDuration", selectedDuration);

        // Determine which scene to load based on disaster selection
        string sceneToLoad = testSceneName; // default
        
        if (disasterDropdown != null)
        {
            switch (disasterDropdown.value)
            {
                case 0: // Flood
                    sceneToLoad = floodSceneName;
                    PlayerPrefs.SetString("DisasterType", "Flood");
                    break;
                case 1: // Earthquake
                    sceneToLoad = earthquakeSceneName;
                    PlayerPrefs.SetString("DisasterType", "Earthquake");
                    break;
                case 2: // Test
                default:
                    sceneToLoad = testSceneName;
                    PlayerPrefs.SetString("DisasterType", "TestKen");
                    break;
            }
        }

        PlayerPrefs.Save();

        UpdateStatus($"Starting game: {sceneToLoad}...");
        Debug.Log($"Loading game scene: {sceneToLoad}");

        // Load the scene for all clients using NetworkManager
        if (NetworkManager.Singleton.SceneManager != null)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Single);
        }
        else
        {
            // Fallback to regular scene loading
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    // ========== Player List Management ==========

    private void ClearPlayerSlots()
    {
        if (instructorNameField != null)
            instructorNameField.text = "";
        
        if (trainee1NameField != null)
            trainee1NameField.text = "";
        
        if (trainee2NameField != null)
            trainee2NameField.text = "";
        
        instructorName = "";
        trainee1Name = "";
        trainee2Name = "";
        traineeCount = 0;
    }

    private void UpdatePlayerSlot(string username, bool isInstructor)
    {
        if (isInstructor)
        {
            instructorName = username;
            if (instructorNameField != null)
                instructorNameField.text = username;
        }
        else
        {
            // Assign to first available trainee slot
            if (string.IsNullOrEmpty(trainee1Name))
            {
                trainee1Name = username;
                if (trainee1NameField != null)
                    trainee1NameField.text = username;
                traineeCount++;
            }
            else if (string.IsNullOrEmpty(trainee2Name))
            {
                trainee2Name = username;
                if (trainee2NameField != null)
                    trainee2NameField.text = username;
                traineeCount++;
            }
            else
            {
                Debug.LogWarning("Maximum trainee slots (2) already filled!");
            }
        }
    }

    private void RemovePlayerSlot(string username, bool wasInstructor)
    {
        if (wasInstructor)
        {
            instructorName = "";
            if (instructorNameField != null)
                instructorNameField.text = "";
        }
        else
        {
            // Remove from trainee slots
            if (trainee1Name == username)
            {
                trainee1Name = "";
                if (trainee1NameField != null)
                    trainee1NameField.text = "";
                traineeCount--;
            }
            else if (trainee2Name == username)
            {
                trainee2Name = "";
                if (trainee2NameField != null)
                    trainee2NameField.text = "";
                traineeCount--;
            }
        }
    }

    // ========== Network Event Handlers ==========

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} connected to lobby");
        // Player data will be updated via UpdatePlayerData call from NetworkPlayer
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} disconnected from lobby");
        // Note: You may need to track clientId to username mapping if you want to handle disconnects
    }

    private void HandlePlayersCountChanged(int count)
    {
        Debug.Log($"Player count changed: {count}");
        // Count is updated automatically by the slots
    }

    // ========== Public Methods ==========

    /// <summary>
    /// Updates a player's display information
    /// Called by NetworkPlayer when it spawns
    /// </summary>
    public void UpdatePlayerData(ulong clientId, string username, bool isInstructor)
    {
        Debug.Log($"Updating player data - ClientId: {clientId}, Username: {username}, Instructor: {isInstructor}");
        UpdatePlayerSlot(username, isInstructor);
    }

    // ========== Utility Methods ==========

    private void UpdateStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        Debug.Log($"[LobbyScene] {message}");
    }
}
