using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using Unity.Collections;

namespace Lobby
{
    /// <summary>
    /// Manages the Lobby Room scene where host configures game settings
    /// and clients wait. Syncs settings across all connected clients.
    /// </summary>
    public class LobbyRoomManager : NetworkBehaviour
    {
        [Header("Host Configuration UI")]
        [SerializeField] private GameObject hostPanel;
        [SerializeField] private Button minusTaskButton;
        [SerializeField] private Button plusTaskButton;
        [SerializeField] private TMP_Text taskCountText;
        [SerializeField] private TMP_Dropdown disasterDropdown;
        [SerializeField] private TMP_Dropdown durationDropdown;
        [SerializeField] private Button startGameButton;

        [Header("Client Waiting UI")]
        [SerializeField] private GameObject clientPanel;
        [SerializeField] private TMP_Text waitingMessageText;

        [Header("Network Status Display (Shared)")]
        [SerializeField] private TMP_Text lobbyCodeText;
        [SerializeField] private TMP_Text connectedPlayersText;
        [SerializeField] private TMP_Text instructorNameText;
        [SerializeField] private TMP_Text traineeNamesText;
        [SerializeField] private TMP_Text currentSettingsText;

        [Header("Settings")]
        [SerializeField] private int minTasks = 5;
        [SerializeField] private int maxTasks = 8;
        private readonly int[] durations = { 300, 480, 600 }; // 5, 8, 10 minutes

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        // Network Variables (synced across all clients)
        private NetworkVariable<int> taskCount = new NetworkVariable<int>(5, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private NetworkVariable<int> disasterIndex = new NetworkVariable<int>(2, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server); // Default TestKen
        private NetworkVariable<int> durationIndex = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        // Network variables for player names
        private NetworkVariable<FixedString128Bytes> instructorName = new NetworkVariable<FixedString128Bytes>(
            new FixedString128Bytes("Instructor"),
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        private NetworkList<FixedString128Bytes> traineeNames;

        private string lobbyCode;
        private List<string> connectedPlayerNames = new List<string>();

        private void Awake()
        {
            // Initialize NetworkList before NetworkObject spawns
            traineeNames = new NetworkList<FixedString128Bytes>();
        }

        private void OnEnable()
        {
            EnsureCursor();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "LobbyRoom")
            {
                EnsureCursor();
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (showDebugLogs)
                Debug.Log($"[LobbyRoomManager] OnNetworkSpawn - IsHost: {IsHost}, IsClient: {IsClient}, IsServer: {IsServer}");

            // Get lobby code from PlayerPrefs
            lobbyCode = PlayerPrefs.GetString("LobbyCode", "N/A");

            // Server: set instructor name from host's PlayerPrefs - try User_Username first
            if (IsServer)
            {
                string hostName = PlayerPrefs.GetString("Current_Username", 
                                  PlayerPrefs.GetString("User_Username", 
                                  PlayerPrefs.GetString("Current_Name", 
                                  PlayerPrefs.GetString("User_Name", "Instructor"))));
                instructorName.Value = new FixedString128Bytes(hostName);
                if (showDebugLogs)
                    Debug.Log($"[LobbyRoomManager] Server set instructor name: {hostName}");
            }

            // Client: send name to server - try User_Username first
            if (!IsServer)
            {
                string clientName = PlayerPrefs.GetString("Current_Username", 
                                    PlayerPrefs.GetString("User_Username", 
                                    PlayerPrefs.GetString("Current_Name", 
                                    PlayerPrefs.GetString("User_Name", "Trainee"))));
                SendClientNameServerRpc(clientName);
                if (showDebugLogs)
                    Debug.Log($"[LobbyRoomManager] Client sending name: {clientName}");
            }

            // Subscribe to name changes
            instructorName.OnValueChanged += OnInstructorNameChanged;
            traineeNames.OnListChanged += OnTraineeNamesChanged;

            // Setup UI based on role
            SetupUIBasedOnRole();

            // Subscribe to network variable changes (for clients to update UI)
            taskCount.OnValueChanged += OnTaskCountChanged;
            disasterIndex.OnValueChanged += OnDisasterIndexChanged;
            durationIndex.OnValueChanged += OnDurationIndexChanged;

            // Initial UI update
            UpdateNetworkStatusDisplay();
            UpdateSettingsDisplay();

            // Start listening for player connections/disconnections
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            // Unsubscribe from events
            taskCount.OnValueChanged -= OnTaskCountChanged;
            disasterIndex.OnValueChanged -= OnDisasterIndexChanged;
            durationIndex.OnValueChanged -= OnDurationIndexChanged;
            instructorName.OnValueChanged -= OnInstructorNameChanged;
            traineeNames.OnListChanged -= OnTraineeNamesChanged;

            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            }
        }

        private void SetupUIBasedOnRole()
        {
            if (IsHost || IsServer)
            {
                // Host/Server: Show configuration panel
                if (hostPanel != null)
                    hostPanel.SetActive(true);

                if (clientPanel != null)
                    clientPanel.SetActive(false);

                // Setup host button listeners
                if (minusTaskButton != null)
                    minusTaskButton.onClick.AddListener(OnDecreaseTaskCount);

                if (plusTaskButton != null)
                    plusTaskButton.onClick.AddListener(OnIncreaseTaskCount);

                if (disasterDropdown != null)
                    disasterDropdown.onValueChanged.AddListener(OnDisasterChanged);

                if (durationDropdown != null)
                    durationDropdown.onValueChanged.AddListener(OnDurationChanged);

                if (startGameButton != null)
                    startGameButton.onClick.AddListener(OnStartGameClicked);

                // Initialize dropdowns to match network variables
                if (disasterDropdown != null)
                    disasterDropdown.value = disasterIndex.Value;

                if (durationDropdown != null)
                    durationDropdown.value = durationIndex.Value;

                UpdateTaskCountDisplay();

                if (showDebugLogs)
                    Debug.Log("[LobbyRoomManager] Host UI configured");
            }
            else
            {
                // Client: Show waiting panel
                if (hostPanel != null)
                    hostPanel.SetActive(false);

                if (clientPanel != null)
                    clientPanel.SetActive(true);

                if (waitingMessageText != null)
                    waitingMessageText.text = "Waiting for host to start the game...";

                if (showDebugLogs)
                    Debug.Log("[LobbyRoomManager] Client UI configured");
            }
        }

        #region Host Controls

        private void OnDecreaseTaskCount()
        {
            if (!IsHost && !IsServer) return;

            if (taskCount.Value > minTasks)
            {
                taskCount.Value--;
                UpdateTaskCountDisplay();
            }
        }

        private void OnIncreaseTaskCount()
        {
            if (!IsHost && !IsServer) return;

            if (taskCount.Value < maxTasks)
            {
                taskCount.Value++;
                UpdateTaskCountDisplay();
            }
        }

        private void OnDisasterChanged(int value)
        {
            if (!IsHost && !IsServer) return;

            disasterIndex.Value = value;
        }

        private void OnDurationChanged(int value)
        {
            if (!IsHost && !IsServer) return;

            durationIndex.Value = value;
        }

        private void OnStartGameClicked()
        {
            if (!IsHost && !IsServer)
            {
                Debug.LogWarning("[LobbyRoomManager] Only host can start the game!");
                return;
            }

            if (showDebugLogs)
                Debug.Log("[LobbyRoomManager] Start Game button clicked");

            // Reset PointManager
            if (PointManager.Instance != null)
            {
                PointManager.Instance.ResetPoints();
                if (showDebugLogs)
                    Debug.Log("[LobbyRoomManager] Points reset");
            }

            // Save game settings to PlayerPrefs
            SaveGameSettingsServerRpc();

            // Load game scene
            string sceneName = GetSceneNameFromDisasterIndex(disasterIndex.Value);
            
            if (showDebugLogs)
                Debug.Log($"[LobbyRoomManager] Loading game scene: {sceneName}");

            NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        private void SaveGameSettingsServerRpc()
        {
            // Save settings on server
            PlayerPrefs.SetInt("TaskCount", taskCount.Value);
            PlayerPrefs.SetString("DisasterType", GetSceneNameFromDisasterIndex(disasterIndex.Value));
            PlayerPrefs.SetInt("GameDuration", durations[durationIndex.Value]);
            PlayerPrefs.SetInt("CameFromLobby", 1);
            PlayerPrefs.Save();

            if (showDebugLogs)
                Debug.Log($"[LobbyRoomManager] Game settings saved: Tasks={taskCount.Value}, Disaster={GetSceneNameFromDisasterIndex(disasterIndex.Value)}, Duration={durations[durationIndex.Value]}");

            // Tell all clients to save settings too
            SaveGameSettingsClientRpc();
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void SaveGameSettingsClientRpc()
        {
            if (IsHost || IsServer) return; // Server already saved

            PlayerPrefs.SetInt("TaskCount", taskCount.Value);
            PlayerPrefs.SetString("DisasterType", GetSceneNameFromDisasterIndex(disasterIndex.Value));
            PlayerPrefs.SetInt("GameDuration", durations[durationIndex.Value]);
            PlayerPrefs.SetInt("CameFromLobby", 1);
            PlayerPrefs.Save();

            if (showDebugLogs)
                Debug.Log($"[LobbyRoomManager] Client: Game settings saved");
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        private void SendClientNameServerRpc(string clientName)
        {
            var fixedName = new FixedString128Bytes(clientName);
            if (!traineeNames.Contains(fixedName))
            {
                traineeNames.Add(fixedName);
                if (showDebugLogs)
                    Debug.Log($"[LobbyRoomManager] Server added trainee: {clientName}");
            }
        }

        #endregion

        #region Network Variable Callbacks

        private void OnTaskCountChanged(int previousValue, int newValue)
        {
            UpdateTaskCountDisplay();
            UpdateSettingsDisplay();
        }

        private void OnDisasterIndexChanged(int previousValue, int newValue)
        {
            // Update dropdown if this is a client
            if (!IsHost && !IsServer && disasterDropdown != null)
            {
                disasterDropdown.value = newValue;
            }
            UpdateSettingsDisplay();
        }

        private void OnDurationIndexChanged(int previousValue, int newValue)
        {
            // Update dropdown if this is a client
            if (!IsHost && !IsServer && durationDropdown != null)
            {
                durationDropdown.value = newValue;
            }
            UpdateSettingsDisplay();
        }

        private void OnInstructorNameChanged(FixedString128Bytes prev, FixedString128Bytes current)
        {
            UpdateNetworkStatusDisplay();
        }

        private void OnTraineeNamesChanged(NetworkListEvent<FixedString128Bytes> changeEvent)
        {
            UpdateNetworkStatusDisplay();
        }

        #endregion

        #region UI Updates

        private void UpdateTaskCountDisplay()
        {
            if (taskCountText != null)
            {
                taskCountText.text = taskCount.Value.ToString();
            }
        }

        private void UpdateNetworkStatusDisplay()
        {
            // Lobby Code
            if (lobbyCodeText != null)
            {
                lobbyCodeText.text = $"Lobby Code: {lobbyCode}";
            }

            // Connected Players
            if (connectedPlayersText != null)
            {
                int playerCount = NetworkManager.Singleton != null ? NetworkManager.Singleton.ConnectedClients.Count : 0;
                connectedPlayersText.text = $"Connected Players: {playerCount}";
            }

            // Instructor Name (from PlayerPrefs or default)
            if (instructorNameText != null)
            {
                instructorNameText.text = $"Instructor: {instructorName.Value}";
            }

            // Trainee Names (simplified - showing count for now)
            if (traineeNamesText != null)
            {
                if (traineeNames.Count > 0)
                {
                    string names = "";
                    for (int i = 0; i < traineeNames.Count; i++)
                    {
                        names += traineeNames[i].ToString();
                        if (i < traineeNames.Count - 1) names += ", ";
                    }
                    traineeNamesText.text = $"Trainees: {names}";
                }
                else
                {
                    traineeNamesText.text = "Trainees: (waiting...)";
                }
            }
        }

        private void UpdateSettingsDisplay()
        {
            if (currentSettingsText != null)
            {
                string disasterName = GetDisasterName(disasterIndex.Value);
                int durationMinutes = durations[durationIndex.Value] / 60;
                
                currentSettingsText.text = $"Current Settings:\n" +
                    $"Tasks: {taskCount.Value}\n" +
                    $"Disaster: {disasterName}\n" +
                    $"Duration: {durationMinutes} minutes";
            }
        }

        #endregion

        #region Helper Methods

        private string GetSceneNameFromDisasterIndex(int index)
        {
            switch (index)
            {
                case 0: return "Flood";
                case 1: return "Earthquake";
                case 2: return "TestKen";
                default: return "TestKen";
            }
        }

        private string GetDisasterName(int index)
        {
            switch (index)
            {
                case 0: return "Flood";
                case 1: return "Earthquake";
                case 2: return "TestKen";
                default: return "TestKen";
            }
        }

        #endregion

        #region Network Events

        private void OnClientConnected(ulong clientId)
        {
            if (showDebugLogs)
                Debug.Log($"[LobbyRoomManager] Client connected: {clientId}");

            UpdateNetworkStatusDisplay();
        }

        private void OnClientDisconnected(ulong clientId)
        {
            if (showDebugLogs)
                Debug.Log($"[LobbyRoomManager] Client disconnected: {clientId}");

            UpdateNetworkStatusDisplay();
        }

        #endregion

        private void Update()
        {
            // Periodically update network status (every second)
            if (Time.frameCount % 60 == 0)
            {
                UpdateNetworkStatusDisplay();
            }

            // Keep cursor visible/unlocked while lobby UI panels are active
            bool anyPanelActive = (hostPanel != null && hostPanel.activeInHierarchy) || (clientPanel != null && clientPanel.activeInHierarchy);
            if (anyPanelActive && (Cursor.lockState != CursorLockMode.None || !Cursor.visible))
            {
                EnsureCursor();
            }
        }

        private static void EnsureCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
