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
    /// Supports 1 Instructor (Host) + 2 Trainees (Clients).
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
        [SerializeField] private TMP_Text hostIpText; // NEW: Display host IP for direct connect
        [SerializeField] private TMP_Text connectedPlayersText;
        [SerializeField] private TMP_Text instructorNameText;
        [SerializeField] private TMP_Text traineeNamesText;
        [SerializeField] private TMP_Text currentSettingsText;

        [Header("Individual Trainee Display (Optional)")]
        [Tooltip("Text to display Trainee 1's name individually")]
        [SerializeField] private TMP_Text trainee1NameText;
        [Tooltip("Text to display Trainee 2's name individually")]
        [SerializeField] private TMP_Text trainee2NameText;

        [Header("Player Limits")]
        [Tooltip("Maximum number of trainees allowed (default: 2)")]
        [SerializeField] private int maxTrainees = 2;

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

        // Synced lobby code - so all clients see the same code
        private NetworkVariable<FixedString32Bytes> syncedLobbyCode = new NetworkVariable<FixedString32Bytes>(
            new FixedString32Bytes("N/A"),
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        // Synced host IP - so trainees can use direct connect if needed
        private NetworkVariable<FixedString64Bytes> syncedHostIp = new NetworkVariable<FixedString64Bytes>(
            new FixedString64Bytes(""),
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        // Network variables for player names
        private NetworkVariable<FixedString128Bytes> instructorName = new NetworkVariable<FixedString128Bytes>(
            new FixedString128Bytes("Instructor"),
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        // Individual trainee names for better control
        private NetworkVariable<FixedString128Bytes> trainee1Name = new NetworkVariable<FixedString128Bytes>(
            new FixedString128Bytes(""),
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        private NetworkVariable<FixedString128Bytes> trainee2Name = new NetworkVariable<FixedString128Bytes>(
            new FixedString128Bytes(""),
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        // Track client IDs for each trainee slot
        private NetworkVariable<ulong> trainee1ClientId = new NetworkVariable<ulong>(
            ulong.MaxValue, // MaxValue means slot is empty
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        private NetworkVariable<ulong> trainee2ClientId = new NetworkVariable<ulong>(
            ulong.MaxValue,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        private NetworkList<FixedString128Bytes> traineeNames; // Keep for backwards compatibility

        private List<string> connectedPlayerNames = new List<string>();

        // Public properties
        public int MaxTrainees => maxTrainees;
        public int CurrentTraineeCount => (trainee1ClientId.Value != ulong.MaxValue ? 1 : 0) + (trainee2ClientId.Value != ulong.MaxValue ? 1 : 0);
        public bool IsLobbyFull => CurrentTraineeCount >= maxTrainees;

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

            // Server: set lobby code and host IP from host's machine
            if (IsServer)
            {
                string hostLobbyCode = PlayerPrefs.GetString("LobbyCode", "N/A");
                syncedLobbyCode.Value = new FixedString32Bytes(hostLobbyCode);
                
                // Get and sync host's IP address
                string hostIp = GetLocalIPAddress();
                ushort hostPort = 7777; // Default port
                var transport = NetworkManager.Singleton?.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
                if (transport != null)
                {
                    hostPort = transport.ConnectionData.Port;
                }
                syncedHostIp.Value = new FixedString64Bytes($"{hostIp}:{hostPort}");
                
                string hostName = PlayerPrefs.GetString("Current_Username", 
                                  PlayerPrefs.GetString("User_Username", 
                                  PlayerPrefs.GetString("Current_Name", 
                                  PlayerPrefs.GetString("User_Name", "Instructor"))));
                instructorName.Value = new FixedString128Bytes(hostName);
                
                if (showDebugLogs)
                {
                    Debug.Log($"[LobbyRoomManager] Server set lobby code: {hostLobbyCode}");
                    Debug.Log($"[LobbyRoomManager] Server set host IP: {hostIp}:{hostPort}");
                    Debug.Log($"[LobbyRoomManager] Server set instructor name: {hostName}");
                }
            }

            // Client: send name to server
            if (!IsServer)
            {
                string clientName = PlayerPrefs.GetString("Current_Username", 
                                    PlayerPrefs.GetString("User_Username", 
                                    PlayerPrefs.GetString("Current_Name", 
                                    PlayerPrefs.GetString("User_Name", "Trainee"))));
                SendClientNameServerRpc(clientName, NetworkManager.Singleton.LocalClientId);
                if (showDebugLogs)
                    Debug.Log($"[LobbyRoomManager] Client sending name: {clientName}");
            }

            // Subscribe to changes
            syncedLobbyCode.OnValueChanged += OnLobbyCodeChanged;
            syncedHostIp.OnValueChanged += OnHostIpChanged;
            instructorName.OnValueChanged += OnInstructorNameChanged;
            trainee1Name.OnValueChanged += OnTraineeNameChanged;
            trainee2Name.OnValueChanged += OnTraineeNameChanged;
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
            syncedLobbyCode.OnValueChanged -= OnLobbyCodeChanged;
            syncedHostIp.OnValueChanged -= OnHostIpChanged;
            taskCount.OnValueChanged -= OnTaskCountChanged;
            disasterIndex.OnValueChanged -= OnDisasterIndexChanged;
            durationIndex.OnValueChanged -= OnDurationIndexChanged;
            instructorName.OnValueChanged -= OnInstructorNameChanged;
            trainee1Name.OnValueChanged -= OnTraineeNameChanged;
            trainee2Name.OnValueChanged -= OnTraineeNameChanged;
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

            // Always load TestKen scene (both game modes are in one scene)
            // GameManager will enable/disable Flood or Earthquake GameObjects based on DisasterType
            string sceneName = "TestKen";
            
            if (showDebugLogs)
                Debug.Log($"[LobbyRoomManager] Loading game scene: {sceneName} with DisasterType: {GetDisasterName(disasterIndex.Value)}");

            NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        private void SaveGameSettingsServerRpc()
        {
            // Save settings on server
            PlayerPrefs.SetInt("TaskCount", taskCount.Value);
            PlayerPrefs.SetString("DisasterType", GetDisasterName(disasterIndex.Value));
            PlayerPrefs.SetInt("DisasterModeIndex", disasterIndex.Value); // Store index for GameManager
            PlayerPrefs.SetInt("GameDuration", durations[durationIndex.Value]);
            PlayerPrefs.SetInt("CameFromLobby", 1);
            PlayerPrefs.Save();

            if (showDebugLogs)
                Debug.Log($"[LobbyRoomManager] Game settings saved: Tasks={taskCount.Value}, Disaster={GetDisasterName(disasterIndex.Value)}, DisasterIndex={disasterIndex.Value}, Duration={durations[durationIndex.Value]}");

            // Tell all clients to save settings too
            SaveGameSettingsClientRpc();
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void SaveGameSettingsClientRpc()
        {
            if (IsHost || IsServer) return; // Server already saved

            PlayerPrefs.SetInt("TaskCount", taskCount.Value);
            PlayerPrefs.SetString("DisasterType", GetDisasterName(disasterIndex.Value));
            PlayerPrefs.SetInt("DisasterModeIndex", disasterIndex.Value); // Store index for GameManager
            PlayerPrefs.SetInt("GameDuration", durations[durationIndex.Value]);
            PlayerPrefs.SetInt("CameFromLobby", 1);
            PlayerPrefs.Save();

            if (showDebugLogs)
                Debug.Log($"[LobbyRoomManager] Client: Game settings saved - DisasterType={GetDisasterName(disasterIndex.Value)}, DisasterIndex={disasterIndex.Value}");
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        private void SendClientNameServerRpc(string clientName, ulong clientId)
        {
            // Check if lobby is full
            if (IsLobbyFull)
            {
                Debug.LogWarning($"[LobbyRoomManager] Lobby is full! Cannot add trainee: {clientName}");
                return;
            }

            // Assign to first available slot
            if (trainee1ClientId.Value == ulong.MaxValue)
            {
                trainee1ClientId.Value = clientId;
                trainee1Name.Value = new FixedString128Bytes(clientName);
                if (showDebugLogs)
                    Debug.Log($"[LobbyRoomManager] Server assigned Trainee 1: {clientName} (ClientId: {clientId})");
            }
            else if (trainee2ClientId.Value == ulong.MaxValue)
            {
                trainee2ClientId.Value = clientId;
                trainee2Name.Value = new FixedString128Bytes(clientName);
                if (showDebugLogs)
                    Debug.Log($"[LobbyRoomManager] Server assigned Trainee 2: {clientName} (ClientId: {clientId})");
            }

            // Also add to legacy list for backwards compatibility
            var fixedName = new FixedString128Bytes(clientName);
            if (!traineeNames.Contains(fixedName))
            {
                traineeNames.Add(fixedName);
            }
        }

        /// <summary>
        /// Removes a trainee from their slot when they disconnect
        /// </summary>
        private void RemoveTraineeByClientId(ulong clientId)
        {
            if (!IsServer) return;

            if (trainee1ClientId.Value == clientId)
            {
                string removedName = trainee1Name.Value.ToString();
                trainee1ClientId.Value = ulong.MaxValue;
                trainee1Name.Value = new FixedString128Bytes("");
                
                // Remove from legacy list
                var fixedName = new FixedString128Bytes(removedName);
                if (traineeNames.Contains(fixedName))
                {
                    traineeNames.Remove(fixedName);
                }
                
                if (showDebugLogs)
                    Debug.Log($"[LobbyRoomManager] Removed Trainee 1: {removedName}");
            }
            else if (trainee2ClientId.Value == clientId)
            {
                string removedName = trainee2Name.Value.ToString();
                trainee2ClientId.Value = ulong.MaxValue;
                trainee2Name.Value = new FixedString128Bytes("");
                
                // Remove from legacy list
                var fixedName = new FixedString128Bytes(removedName);
                if (traineeNames.Contains(fixedName))
                {
                    traineeNames.Remove(fixedName);
                }
                
                if (showDebugLogs)
                    Debug.Log($"[LobbyRoomManager] Removed Trainee 2: {removedName}");
            }
        }

        #endregion

        #region Network Variable Callbacks

        private void OnLobbyCodeChanged(FixedString32Bytes previousValue, FixedString32Bytes newValue)
        {
            if (showDebugLogs)
                Debug.Log($"[LobbyRoomManager] Lobby code synced: {newValue}");
            UpdateNetworkStatusDisplay();
        }

        private void OnHostIpChanged(FixedString64Bytes previousValue, FixedString64Bytes newValue)
        {
            if (showDebugLogs)
                Debug.Log($"[LobbyRoomManager] Host IP synced: {newValue}");
            UpdateNetworkStatusDisplay();
        }

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

        private void OnTraineeNameChanged(FixedString128Bytes prev, FixedString128Bytes current)
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
            // Lobby Code - now uses synced NetworkVariable
            if (lobbyCodeText != null)
            {
                lobbyCodeText.text = $"Lobby Code: {syncedLobbyCode.Value}";
            }

            // Host IP - for direct connect if lobby code doesn't work
            if (hostIpText != null)
            {
                string ipValue = syncedHostIp.Value.ToString();
                if (!string.IsNullOrEmpty(ipValue))
                {
                    hostIpText.text = $"Host IP: {ipValue}\n(Use this for Direct Connect)";
                }
                else
                {
                    hostIpText.text = "Host IP: (loading...)";
                }
            }

            // Connected Players (1 Instructor + up to 2 Trainees = 3 max)
            if (connectedPlayersText != null)
            {
                int playerCount = NetworkManager.Singleton != null ? NetworkManager.Singleton.ConnectedClients.Count : 0;
                // Use rich text: "Connected Players:" in orange, "X/3" in white
                connectedPlayersText.text = $"<color=#FFA500>Connected Players:</color> <color=#FFFFFF>{playerCount}/3</color>";
            }

            // Instructor Name
            if (instructorNameText != null)
            {
                instructorNameText.text = $"Instructor: {instructorName.Value}";
            }

            // Individual Trainee Names
            if (trainee1NameText != null)
            {
                string name1 = trainee1Name.Value.ToString();
                trainee1NameText.text = string.IsNullOrEmpty(name1) ? "Trainee 1: (waiting...)" : $"Trainee 1: {name1}";
            }

            if (trainee2NameText != null)
            {
                string name2 = trainee2Name.Value.ToString();
                trainee2NameText.text = string.IsNullOrEmpty(name2) ? "Trainee 2: (waiting...)" : $"Trainee 2: {name2}";
            }

            // Combined Trainee Names (legacy support)
            if (traineeNamesText != null)
            {
                string name1 = trainee1Name.Value.ToString();
                string name2 = trainee2Name.Value.ToString();

                List<string> names = new List<string>();
                if (!string.IsNullOrEmpty(name1)) names.Add(name1);
                if (!string.IsNullOrEmpty(name2)) names.Add(name2);

                if (names.Count > 0)
                {
                    traineeNamesText.text = $"Trainees: {string.Join(", ", names)} ({names.Count}/{maxTrainees})";
                }
                else
                {
                    traineeNamesText.text = $"Trainees: (waiting... 0/{maxTrainees})";
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

        /// <summary>
        /// Gets the local IP address of this machine
        /// </summary>
        private string GetLocalIPAddress()
        {
            try
            {
                var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }
            }
            catch (System.Exception e)
            {
                if (showDebugLogs)
                    Debug.LogWarning($"[LobbyRoomManager] Failed to get local IP: {e.Message}");
            }
            return "127.0.0.1";
        }

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

        /// <summary>
        /// Gets the trainee slot number for a given client ID (1 or 2), returns 0 if not found
        /// </summary>
        public int GetTraineeSlot(ulong clientId)
        {
            if (trainee1ClientId.Value == clientId) return 1;
            if (trainee2ClientId.Value == clientId) return 2;
            return 0;
        }

        /// <summary>
        /// Gets the name of a trainee by slot number (1 or 2)
        /// </summary>
        public string GetTraineeName(int slot)
        {
            switch (slot)
            {
                case 1: return trainee1Name.Value.ToString();
                case 2: return trainee2Name.Value.ToString();
                default: return "";
            }
        }

        /// <summary>
        /// Gets the synced lobby code (use this instead of PlayerPrefs)
        /// </summary>
        public string GetLobbyCode()
        {
            return syncedLobbyCode.Value.ToString();
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

            // Remove trainee from their slot if they disconnect
            if (IsServer)
            {
                RemoveTraineeByClientId(clientId);
            }

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
