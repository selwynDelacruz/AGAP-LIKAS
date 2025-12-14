using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;
using System.Collections;

/// <summary>
/// Manages the game objective display and ready system for AGAP-LIKAS.
/// Shows objectives UI when game scene loads, waits for all players to be ready,
/// then starts the actual gameplay and timer.
/// </summary>
public class ObjectiveManager : NetworkBehaviour
{
    public static ObjectiveManager Instance { get; private set; }

    #region UI References
    [Header("Objective Panel UI")]
    [Tooltip("Main panel containing objectives - shown at game start")]
    [SerializeField] private GameObject objectivePanel;

    [Tooltip("Title text for the objective panel")]
    [SerializeField] private TMP_Text titleText;

    [Tooltip("Description text explaining the game/mission")]
    [SerializeField] private TMP_Text descriptionText;

    [Tooltip("List of objectives text")]
    [SerializeField] private TMP_Text objectivesListText;

    [Tooltip("Ready button for players to click")]
    [SerializeField] private Button readyButton;

    [Tooltip("Text on the ready button")]
    [SerializeField] private TMP_Text readyButtonText;

    [Tooltip("Status text showing ready count (e.g., '1/3 Players Ready')")]
    [SerializeField] private TMP_Text readyStatusText;
    
    [Tooltip("Text shown when player is waiting for others to be ready")]
    [SerializeField] private TMP_Text waitingForOthersText;

    [Tooltip("Optional: Countdown text shown before game starts")]
    [SerializeField] private TMP_Text countdownText;
    
    [Tooltip("Optional: Loading indicator shown while map is loading")]
    [SerializeField] private GameObject loadingIndicator;
    
    [Tooltip("Optional: Loading status text")]
    [SerializeField] private TMP_Text loadingStatusText;
    
    [Header("User UI Reference")]
    [Tooltip("The main User UI panel to hide during objective display")]
    [SerializeField] private GameObject userUIPanel;
    #endregion

    #region Content Settings
    [Header("Objective Content")]
    [SerializeField] private string defaultTitle = "MISSION BRIEFING";
    
    [SerializeField, TextArea(3, 6)] 
    private string defaultDescription = "Welcome to AGAP-LIKAS Disaster Response Training.\n\nYour mission is to rescue victims affected by the disaster. Work together with your team to save as many lives as possible within the time limit.";

    [SerializeField, TextArea(5, 10)]
    private string defaultObjectives = "• Locate and rescue trapped victims\n• Provide first aid using medkits\n• Clear rubble to access victims\n• Return rescued victims to the safe zone\n• Complete objectives before time runs out";
    #endregion

    #region Countdown Settings
    [Header("Countdown Settings")]
    [Tooltip("Seconds to countdown after all players are ready")]
    [SerializeField] private int countdownSeconds = 3;

    [Tooltip("Sound to play during countdown (optional)")]
    [SerializeField] private AudioClip countdownSound;

    [Tooltip("Sound to play when game starts (optional)")]
    [SerializeField] private AudioClip gameStartSound;
    #endregion

    #region Debug Settings
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    #endregion

    #region Network Variables
    // Track how many players are ready
    private NetworkVariable<int> readyPlayerCount = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // Track total expected players
    private NetworkVariable<int> totalPlayerCount = new NetworkVariable<int>(
        1,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // Flag to indicate game has started (prevents double-start)
    private NetworkVariable<bool> hasGameStarted = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    #endregion

    #region Private State
    private bool localPlayerIsReady = false;
    private AudioSource audioSource;
    private bool isCountingDown = false;
    private bool mapsLoaded = false;
    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Get or add AudioSource for sounds
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (countdownSound != null || gameStartSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void Start()
    {
        // Setup button listener
        if (readyButton != null)
        {
            readyButton.onClick.AddListener(OnReadyButtonClicked);
        }

        // Initialize UI content
        SetupObjectiveContent();

        // Show objective panel
        if (objectivePanel != null)
        {
            objectivePanel.SetActive(true);
        }
        
        // Hide UserUI while objective panel is showing
        if (userUIPanel != null)
        {
            userUIPanel.SetActive(false);
            if (showDebugLogs)
                Debug.Log("[ObjectiveManager] UserUI hidden during objective display");
        }

        // Hide countdown initially
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }
        
        // Hide waiting text initially
        if (waitingForOthersText != null)
        {
            waitingForOthersText.gameObject.SetActive(false);
        }
        
        // Setup loading indicator - check if maps are already loaded
        mapsLoaded = MapSpawner.MapsReady;
        UpdateLoadingIndicator();
        
        // Subscribe to map spawning event
        MapSpawner.OnMapsSpawned += OnMapsLoaded;

        // Unlock cursor for UI interaction - do this AFTER other setup
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Pause the game time while showing objectives
        Time.timeScale = 0f;

        if (showDebugLogs)
            Debug.Log("[ObjectiveManager] Initialized - Showing objective panel");
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Subscribe to network variable changes
        readyPlayerCount.OnValueChanged += OnReadyCountChanged;
        totalPlayerCount.OnValueChanged += OnTotalCountChanged;
        hasGameStarted.OnValueChanged += OnGameStartedChanged;

        // Server: Initialize player count based on connected clients
        if (IsServer)
        {
            totalPlayerCount.Value = NetworkManager.Singleton.ConnectedClients.Count;

            // Subscribe to connection events for dynamic player count
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

            if (showDebugLogs)
                Debug.Log($"[ObjectiveManager] Server initialized with {totalPlayerCount.Value} players");
        }

        // Update UI with current state
        UpdateReadyStatusUI();

        // If joining mid-game and game already started, skip objectives
        if (hasGameStarted.Value)
        {
            OnGameAlreadyStarted();
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        // Unsubscribe from events
        readyPlayerCount.OnValueChanged -= OnReadyCountChanged;
        totalPlayerCount.OnValueChanged -= OnTotalCountChanged;
        hasGameStarted.OnValueChanged -= OnGameStartedChanged;

        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    private void OnDestroy()
    {
        // Cleanup button listener
        if (readyButton != null)
        {
            readyButton.onClick.RemoveListener(OnReadyButtonClicked);
        }
        
        // Unsubscribe from map spawning event
        MapSpawner.OnMapsSpawned -= OnMapsLoaded;
    }

    #endregion

    #region Setup Methods

    /// <summary>
    /// Sets up the objective panel content based on disaster type
    /// </summary>
    private void SetupObjectiveContent()
    {
        // Get disaster type from PlayerPrefs
        string disasterType = PlayerPrefs.GetString("DisasterType", "TestKen");
        int taskCount = PlayerPrefs.GetInt("TaskCount", 5);
        int durationSeconds = PlayerPrefs.GetInt("GameDuration", 300);
        int durationMinutes = durationSeconds / 60;

        // Set title
        if (titleText != null)
        {
            titleText.text = defaultTitle;
        }

        // Customize description based on disaster type
        if (descriptionText != null)
        {
            string customDescription = disasterType switch
            {
                "Flood" => $"A severe flood has struck the area. Victims are trapped and need immediate rescue.\n\nYour team has {durationMinutes} minutes to save as many lives as possible.",
                "Earthquake" => $"A major earthquake has devastated the region. Victims are trapped under rubble and debris.\n\nYour team has {durationMinutes} minutes to rescue survivors.",
                _ => $"{defaultDescription}\n\nTime Limit: {durationMinutes} minutes"
            };
            descriptionText.text = customDescription;
        }

        // Set objectives list
        if (objectivesListText != null)
        {
            string customObjectives = disasterType switch
            {
                "Flood" => $"• Rescue {taskCount} trapped victims from flood waters\n• Use boats to navigate flooded areas\n• Provide first aid with medkits\n• Bring victims to the safe zone\n• Complete before time runs out",
                "Earthquake" => $"• Locate {taskCount} victims trapped under rubble\n• Clear debris to access victims\n• Provide medical assistance\n• Escort victims to safety\n• Work as a team to maximize rescues",
                _ => $"• Rescue {taskCount} victims\n{defaultObjectives}"
            };
            objectivesListText.text = customObjectives;
        }

        // Set initial button text
        if (readyButtonText != null)
        {
            readyButtonText.text = "Ready";
        }
    }

    #endregion

    #region Ready System

    /// <summary>
    /// Called when the local player clicks the Ready button
    /// </summary>
    private void OnReadyButtonClicked()
    {
        if (localPlayerIsReady)
        {
            if (showDebugLogs)
                Debug.Log("[ObjectiveManager] Player already ready");
            return;
        }

        localPlayerIsReady = true;

        // Hide title, description, and objectives text
        if (titleText != null)
        {
            titleText.gameObject.SetActive(false);
        }
        
        if (descriptionText != null)
        {
            descriptionText.gameObject.SetActive(false);
        }
        
        if (objectivesListText != null)
        {
            objectivesListText.gameObject.SetActive(false);
        }

        // Disable ready button (don't change its text)
        if (readyButton != null)
        {
            readyButton.interactable = false;
        }
        
        // Show "Waiting for others..." text
        if (waitingForOthersText != null)
        {
            waitingForOthersText.gameObject.SetActive(true);
            waitingForOthersText.text = "WAITING FOR OTHERS...";
        }

        // Notify server that this player is ready
        PlayerReadyServerRpc();

        if (showDebugLogs)
            Debug.Log("[ObjectiveManager] Local player marked as ready - hiding objective content");
    }

    /// <summary>
    /// ServerRpc called when a player clicks Ready
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void PlayerReadyServerRpc(ServerRpcParams rpcParams = default)
    {
        readyPlayerCount.Value++;

        if (showDebugLogs)
            Debug.Log($"[ObjectiveManager] Server: Player ready. Count: {readyPlayerCount.Value}/{totalPlayerCount.Value}");

        // Check if all players are ready
        if (readyPlayerCount.Value >= totalPlayerCount.Value && !isCountingDown)
        {
            StartCountdownClientRpc();
        }
    }

    /// <summary>
    /// Called on server when a client connects
    /// </summary>
    private void OnClientConnected(ulong clientId)
    {
        if (!IsServer) return;

        totalPlayerCount.Value = NetworkManager.Singleton.ConnectedClients.Count;

        if (showDebugLogs)
            Debug.Log($"[ObjectiveManager] Client connected. Total players: {totalPlayerCount.Value}");
    }

    /// <summary>
    /// Called on server when a client disconnects
    /// </summary>
    private void OnClientDisconnected(ulong clientId)
    {
        if (!IsServer) return;

        totalPlayerCount.Value = NetworkManager.Singleton.ConnectedClients.Count;

        // If we were waiting and now all remaining players are ready, start countdown
        if (readyPlayerCount.Value >= totalPlayerCount.Value && !hasGameStarted.Value && !isCountingDown)
        {
            StartCountdownClientRpc();
        }

        if (showDebugLogs)
            Debug.Log($"[ObjectiveManager] Client disconnected. Total players: {totalPlayerCount.Value}");
    }

    #endregion

    #region Network Variable Callbacks

    private void OnReadyCountChanged(int previousValue, int newValue)
    {
        if (showDebugLogs)
            Debug.Log($"[ObjectiveManager] Ready count changed: {previousValue} -> {newValue}");

        UpdateReadyStatusUI();
    }

    private void OnTotalCountChanged(int previousValue, int newValue)
    {
        if (showDebugLogs)
            Debug.Log($"[ObjectiveManager] Total player count changed: {previousValue} -> {newValue}");

        UpdateReadyStatusUI();
    }

    private void OnGameStartedChanged(bool previousValue, bool newValue)
    {
        if (newValue && !previousValue)
        {
            OnGameAlreadyStarted();
        }
    }

    #endregion

    #region UI Updates

    /// <summary>
    /// Updates the ready status display
    /// </summary>
    private void UpdateReadyStatusUI()
    {
        if (readyStatusText != null)
        {
            readyStatusText.text = $"{readyPlayerCount.Value}/{totalPlayerCount.Value} Players Ready";
        }
    }
    
    /// <summary>
    /// Called when maps have finished loading
    /// </summary>
    private void OnMapsLoaded()
    {
        mapsLoaded = true;
        UpdateLoadingIndicator();
        
        if (showDebugLogs)
            Debug.Log("[ObjectiveManager] Maps loaded in background");
    }
    
    /// <summary>
    /// Updates the loading indicator visibility
    /// </summary>
    private void UpdateLoadingIndicator()
    {
        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(!mapsLoaded);
        }
        
        if (loadingStatusText != null)
        {
            loadingStatusText.text = mapsLoaded ? "Map Ready" : "Loading Map...";
            loadingStatusText.gameObject.SetActive(!mapsLoaded);
        }
        
        // Optionally disable ready button until maps are loaded
        // Uncomment if you want to prevent ready until map loads:
        // if (readyButton != null && !mapsLoaded)
        // {
        //     readyButton.interactable = false;
        // }
    }

    #endregion

    #region Countdown and Game Start

    /// <summary>
    /// ClientRpc to start the countdown on all clients
    /// </summary>
    [ClientRpc]
    private void StartCountdownClientRpc()
    {
        if (isCountingDown) return;

        isCountingDown = true;
        StartCoroutine(CountdownCoroutine());
    }

    /// <summary>
    /// Countdown coroutine that runs on all clients
    /// </summary>
    private IEnumerator CountdownCoroutine()
    {
        if (showDebugLogs)
            Debug.Log("[ObjectiveManager] Starting countdown...");

        // Show countdown text
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
        }

        // Hide ready button during countdown
        if (readyButton != null)
        {
            readyButton.gameObject.SetActive(false);
        }
        
        // Hide ready status during countdown
        if (readyStatusText != null)
        {
            readyStatusText.gameObject.SetActive(false);
        }
        
        // Hide waiting text during countdown
        if (waitingForOthersText != null)
        {
            waitingForOthersText.gameObject.SetActive(false);
        }

        // Countdown loop (use unscaled time since Time.timeScale is 0)
        for (int i = countdownSeconds; i > 0; i--)
        {
            if (countdownText != null)
            {
                countdownText.text = i.ToString();
            }

            // Play countdown sound
            if (audioSource != null && countdownSound != null)
            {
                audioSource.PlayOneShot(countdownSound);
            }

            if (showDebugLogs)
                Debug.Log($"[ObjectiveManager] Countdown: {i}");

            yield return new WaitForSecondsRealtime(1f);
        }

        // Show "GO!" briefly
        if (countdownText != null)
        {
            countdownText.text = "GO!";
        }

        // Play game start sound
        if (audioSource != null && gameStartSound != null)
        {
            audioSource.PlayOneShot(gameStartSound);
        }

        yield return new WaitForSecondsRealtime(0.5f);

        // Start the game
        StartGame();
    }

    /// <summary>
    /// Starts the actual gameplay
    /// </summary>
    private void StartGame()
    {
        if (showDebugLogs)
            Debug.Log("[ObjectiveManager] Starting game!");

        // Server marks game as started
        if (IsServer)
        {
            hasGameStarted.Value = true;
        }

        // Hide objective panel
        if (objectivePanel != null)
        {
            objectivePanel.SetActive(false);
        }
        
        // Show UserUI now that game is starting
        if (userUIPanel != null)
        {
            userUIPanel.SetActive(true);
            if (showDebugLogs)
                Debug.Log("[ObjectiveManager] UserUI shown - game starting");
        }

        // Resume game time
        Time.timeScale = 1f;

        // Lock cursor for gameplay (only for trainees, not instructors)
        if (PlayerPrefs.GetString("Type_Of_User", "") != "instructor")
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // Start the GameManager timer
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartTimer();
            
            if (showDebugLogs)
                Debug.Log("[ObjectiveManager] GameManager timer started");
        }
        else
        {
            Debug.LogWarning("[ObjectiveManager] GameManager.Instance not found! Timer may not start.");
        }
        
        // Notify SpectatorController to refresh trainees (if instructor is spectating)
        var spectatorController = FindAnyObjectByType<SpectatorController>();
        if (spectatorController != null)
        {
            spectatorController.ForceRefreshTrainees();
            if (showDebugLogs)
                Debug.Log("[ObjectiveManager] Notified SpectatorController to refresh trainees");
        }
    }

    /// <summary>
    /// Called when joining a game that has already started
    /// </summary>
    private void OnGameAlreadyStarted()
    {
        if (showDebugLogs)
            Debug.Log("[ObjectiveManager] Joining game in progress - skipping objectives");

        // Hide objective panel immediately
        if (objectivePanel != null)
        {
            objectivePanel.SetActive(false);
        }
        
        // Show UserUI for late joiners
        if (userUIPanel != null)
        {
            userUIPanel.SetActive(true);
        }

        // Resume game time
        Time.timeScale = 1f;

        // Lock cursor for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    #endregion

    #region Public API

    /// <summary>
    /// Check if all players are ready
    /// </summary>
    public bool AreAllPlayersReady()
    {
        return readyPlayerCount.Value >= totalPlayerCount.Value;
    }

    /// <summary>
    /// Check if the game has started
    /// </summary>
    public bool HasGameStarted()
    {
        return hasGameStarted.Value;
    }

    /// <summary>
    /// Get the current ready player count
    /// </summary>
    public int GetReadyPlayerCount()
    {
        return readyPlayerCount.Value;
    }

    /// <summary>
    /// Get the total player count
    /// </summary>
    public int GetTotalPlayerCount()
    {
        return totalPlayerCount.Value;
    }

    /// <summary>
    /// Force skip the objective screen (for testing/debugging)
    /// Only works on server/host
    /// </summary>
    public void ForceStartGame()
    {
        if (IsServer && !hasGameStarted.Value)
        {
            StartCountdownClientRpc();
        }
    }

    #endregion

    private void Update()
    {
        // Keep cursor visible while objective panel is showing and game hasn't started
        if (!hasGameStarted.Value && objectivePanel != null && objectivePanel.activeSelf)
        {
            if (Cursor.lockState != CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.None;
            }
            if (!Cursor.visible)
            {
                Cursor.visible = true;
            }
        }
    }
}
