using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Net;
using System.Linq;

namespace Lobby
{
    /// <summary>
    /// Manages the Lobby Menu scene where users can create or join lobbies
    /// Shows different UI based on user role (Instructor/Trainee)
    /// NetworkManager must exist in this scene and will persist via DontDestroyOnLoad
    /// </summary>
    public class LobbyMenuManager : MonoBehaviour
    {
        [Header("UI Panels")]
        [SerializeField] private GameObject createLobbyPanel;
        [SerializeField] private GameObject joinLobbyPanel;

        [Header("Create Lobby UI (Instructor)")]
        [SerializeField] private Button createLobbyButton;
        [SerializeField] private TMP_Text lobbyCodeDisplayText;
        [SerializeField] private TMP_Text hostIpDisplayText; // NEW: Show host IP for direct connect

        [Header("Join Lobby UI (Trainee)")]
        [SerializeField] private TMP_InputField lobbyCodeInputField;
        [SerializeField] private Button joinLobbyButton;
        [SerializeField] private TMP_Text statusText;

        [Header("Common UI")]
        [SerializeField] private Button backButton;

        [Header("Settings")]
        [SerializeField] private string lobbyRoomSceneName = "LobbyRoom";
        [SerializeField] private ushort port = 7777;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        [Header("Direct Connect (Optional)")]
        [SerializeField] private TMP_InputField directIpInputField; // optional: set in Inspector to enable manual IP connect
        [SerializeField] private Button directConnectButton;        // optional: set in Inspector

        // Status Colors
        private static readonly Color COLOR_SUCCESS = new Color(0.2f, 0.8f, 0.2f);      // Green - success
        private static readonly Color COLOR_ERROR = new Color(0.9f, 0.2f, 0.2f);        // Red - error
        private static readonly Color COLOR_WARNING = new Color(1f, 0.65f, 0f);         // Orange - warning/info needed
        private static readonly Color COLOR_SEARCHING = new Color(0.3f, 0.7f, 1f);      // Light Blue - searching/connecting
        private static readonly Color COLOR_INFO = Color.white;                          // White - neutral info

        private string userRole;
        private LobbyBroadcaster broadcaster;
        private LobbyScanner scanner;
        private string currentLobbyCode;

        private void Awake()
        {
            // Ensure UnityMainThreadDispatcher exists
            Lobby.UnityMainThreadDispatcher.Instance();

            // Make NetworkManager persistent across scenes (if it exists in this scene)
            EnsureNetworkManagerPersists();
        }

        private void Start()
        {
            // Get user role from PlayerPrefs
            userRole = PlayerPrefs.GetString("Type_Of_User", "");

            if (showDebugLogs)
                Debug.Log($"[LobbyMenuManager] User role: {userRole}");

            // Verify NetworkManager exists
            if (NetworkManager.Singleton == null)
            {
                Debug.LogError("[LobbyMenuManager] NetworkManager not found in scene! Please add NetworkManager to LobbyMenu scene.");
                SetStatus("Error: Network not initialized", StatusType.Error);
                return;
            }

            // Extra diagnostics about transport
            var transport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
            if (transport != null && showDebugLogs)
            {
                Debug.Log($"[LobbyMenuManager] Transport initial address: {transport.ConnectionData.Address}:{transport.ConnectionData.Port}");
            }

            // Setup UI based on role
            SetupUIBasedOnRole();

            // Setup button listeners
            SetupButtons();

            // Hook network events for debug
            NetworkManager.Singleton.OnClientConnectedCallback += OnAnyClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnAnyClientDisconnected;

            SetupDirectConnectUI();
        }

        private void OnAnyClientConnected(ulong clientId)
        {
            if (showDebugLogs)
                Debug.Log($"[LobbyMenuManager] OnClientConnectedCallback fired. LocalClientId={NetworkManager.Singleton.LocalClientId} JoinedClientId={clientId} IsHost={NetworkManager.Singleton.IsHost} IsServer={NetworkManager.Singleton.IsServer} IsClient={NetworkManager.Singleton.IsClient}");
        }

        private void OnAnyClientDisconnected(ulong clientId)
        {
            if (showDebugLogs)
                Debug.Log($"[LobbyMenuManager] OnClientDisconnectCallback fired. DisconnectedClientId={clientId}");
        }

        /// <summary>
        /// Ensure NetworkManager persists across scenes
        /// </summary>
        private void EnsureNetworkManagerPersists()
        {
            if (NetworkManager.Singleton != null)
            {
                // Make NetworkManager persist across scenes
                DontDestroyOnLoad(NetworkManager.Singleton.gameObject);

                if (showDebugLogs)
                    Debug.Log("[LobbyMenuManager] NetworkManager set to DontDestroyOnLoad");
            }
        }

        private void SetupUIBasedOnRole()
        {
            if (userRole == "instructor")
            {
                // Show Create Lobby panel for Instructors
                if (createLobbyPanel != null)
                    createLobbyPanel.SetActive(true);

                if (joinLobbyPanel != null)
                    joinLobbyPanel.SetActive(false);

                if (showDebugLogs)
                    Debug.Log("[LobbyMenuManager] Instructor UI enabled - Create Lobby panel shown");
            }
            else if (userRole == "trainee")
            {
                // Show Join Lobby panel for Trainees
                if (createLobbyPanel != null)
                    createLobbyPanel.SetActive(false);

                if (joinLobbyPanel != null)
                    joinLobbyPanel.SetActive(true);

                if (showDebugLogs)
                    Debug.Log("[LobbyMenuManager] Trainee UI enabled - Join Lobby panel shown");
            }
            else
            {
                Debug.LogError($"[LobbyMenuManager] Unknown user role: '{userRole}'");
                SetStatus("Error: User role not set. Please log in again.", StatusType.Error);
            }
        }

        private void SetupButtons()
        {
            if (createLobbyButton != null)
                createLobbyButton.onClick.AddListener(OnCreateLobbyClicked);

            if (joinLobbyButton != null)
                joinLobbyButton.onClick.AddListener(OnJoinLobbyClicked);

            if (backButton != null)
                backButton.onClick.AddListener(OnBackClicked);
        }

        private void SetupDirectConnectUI()
        {
            // Wire optional direct-connect controls if assigned
            if (directConnectButton != null)
            {
                directConnectButton.onClick.RemoveAllListeners();
                directConnectButton.onClick.AddListener(OnDirectConnectClicked);
                if (showDebugLogs) Debug.Log("[LobbyMenuManager] DirectConnect UI wired");
            }
        }

        #region Create Lobby (Instructor)

        private void OnCreateLobbyClicked()
        {
            if (showDebugLogs)
                Debug.Log("[LobbyMenuManager] Create Lobby button clicked");

            // Verify NetworkManager is ready
            if (NetworkManager.Singleton == null)
            {
                Debug.LogError("[LobbyMenuManager] NetworkManager not found!");
                SetStatus("Error: Network not initialized", StatusType.Error);
                return;
            }

            // Generate lobby code
            currentLobbyCode = LobbyCodeGenerator.GenerateCode();
            
            // Store lobby code in PlayerPrefs
            PlayerPrefs.SetString("LobbyCode", currentLobbyCode);
            PlayerPrefs.Save();

            if (showDebugLogs)
                Debug.Log($"[LobbyMenuManager] Generated lobby code: {currentLobbyCode}");

            // Display lobby code to instructor IMMEDIATELY
            if (lobbyCodeDisplayText != null)
            {
                lobbyCodeDisplayText.text = $"Lobby Code: {currentLobbyCode}";
                lobbyCodeDisplayText.gameObject.SetActive(true);
            }

            // Get local IP for broadcasting
            string localIP = GetLocalIPAddressVerbose();

            // Display host IP (optional - main display is in LobbyRoom)
            if (hostIpDisplayText != null)
            {
                hostIpDisplayText.text = $"Host IP: {localIP}:{port}";
                hostIpDisplayText.gameObject.SetActive(true);
            }

            // Disable the create button to prevent double-clicks
            if (createLobbyButton != null)
                createLobbyButton.interactable = false;

            // Configure NetworkManager for hosting
            ConfigureTransport("0.0.0.0", port);

            if (showDebugLogs) Debug.Log("[LobbyMenuManager] Calling StartHost()");
            // Start as Host
            bool success = NetworkManager.Singleton.StartHost();

            if (success)
            {
                var transport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
                if (transport != null && showDebugLogs)
                {
                    Debug.Log($"[LobbyMenuManager] Host started. ServerListenAddress={transport.ConnectionData.ServerListenAddress} Port={transport.ConnectionData.Port}");
                }

                // Start broadcasting lobby code on LAN
                broadcaster = gameObject.AddComponent<LobbyBroadcaster>();
                broadcaster.StartBroadcasting(currentLobbyCode, localIP, port);

                // Show status message
                SetStatus($"Lobby Created! Code: {currentLobbyCode}\nLoading lobby room...", StatusType.Success);

                // Wait 2 seconds before loading lobby room so user can see the code
                Invoke(nameof(LoadLobbyRoom), 2f);
            }
            else
            {
                Debug.LogError("[LobbyMenuManager] Failed to start as Host!");
                SetStatus("Failed to create lobby. Try again.", StatusType.Error);
                
                // Re-enable button on failure
                if (createLobbyButton != null)
                    createLobbyButton.interactable = true;
            }
        }

        #endregion

        #region Join Lobby (Trainee)

        private void OnJoinLobbyClicked()
        {
            if (showDebugLogs)
                Debug.Log("[LobbyMenuManager] Join Lobby button clicked");

            // Verify NetworkManager is ready
            if (NetworkManager.Singleton == null)
            {
                Debug.LogError("[LobbyMenuManager] NetworkManager not found!");
                SetStatus("Error: Network not initialized", StatusType.Error);
                return;
            }

            // Get input from field (can be lobby code OR IP address)
            string input = lobbyCodeInputField.text.Trim();

            if (string.IsNullOrEmpty(input))
            {
                SetStatus("Please enter a lobby code or IP address", StatusType.Warning);
                return;
            }

            // Check if input is an IP address (direct connect)
            if (IsIPAddress(input))
            {
                if (showDebugLogs)
                    Debug.Log($"[LobbyMenuManager] Detected IP address: {input}");
                
                DirectConnectToIP(input);
                return;
            }

            // Otherwise, treat as lobby code
            string enteredCode = input.ToUpper();

            // Validate code format
            if (!LobbyCodeGenerator.ValidateCode(enteredCode))
            {
                SetStatus("Invalid format!\nUse code (ABC123) or IP (192.168.1.10)", StatusType.Warning);
                return;
            }

            if (showDebugLogs)
                Debug.Log($"[LobbyMenuManager] Searching for lobby: {enteredCode}");

            // Show searching status
            SetStatus($"Searching for lobby {enteredCode}...", StatusType.Searching);

            // Disable join button while searching
            if (joinLobbyButton != null)
                joinLobbyButton.interactable = false;

            // Start scanning for lobby
            scanner = gameObject.AddComponent<LobbyScanner>();
            scanner.StartScanning(
                enteredCode,
                onLobbyFound: OnLobbyFound,
                onScanTimeout: OnScanTimeout
            );
        }

        /// <summary>
        /// Check if the input string is an IP address
        /// Supports: 192.168.1.10, 192.168.1.10:7777, 127.0.0.1, localhost
        /// </summary>
        private bool IsIPAddress(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            // Handle "localhost" specially
            if (input.ToLower().StartsWith("localhost"))
                return true;

            // Remove port if present (e.g., "192.168.1.10:7777" -> "192.168.1.10")
            string ipPart = input.Contains(":") ? input.Split(':')[0] : input;

            // Check if it looks like an IP address (contains dots and numbers)
            if (ipPart.Contains("."))
            {
                // Try to parse as IP address
                return System.Net.IPAddress.TryParse(ipPart, out _);
            }

            return false;
        }

        /// <summary>
        /// Direct connect to an IP address
        /// </summary>
        private void DirectConnectToIP(string input)
        {
            string ip;
            ushort connectPort = port; // Default port

            // Handle localhost
            if (input.ToLower().StartsWith("localhost"))
            {
                ip = "127.0.0.1";
                // Check for port in localhost:7777 format
                if (input.Contains(":"))
                {
                    string portStr = input.Split(':')[1];
                    if (ushort.TryParse(portStr, out ushort parsedPort))
                        connectPort = parsedPort;
                }
            }
            // Handle IP:Port format (e.g., 192.168.1.10:7777)
            else if (input.Contains(":"))
            {
                string[] parts = input.Split(':');
                ip = parts[0];
                if (parts.Length > 1 && ushort.TryParse(parts[1], out ushort parsedPort))
                    connectPort = parsedPort;
            }
            else
            {
                ip = input;
            }

            if (showDebugLogs)
                Debug.Log($"[LobbyMenuManager] Direct connecting to {ip}:{connectPort}");

            SetStatus($"Connecting to {ip}:{connectPort}...", StatusType.Searching);

            // Disable join button
            if (joinLobbyButton != null)
                joinLobbyButton.interactable = false;

            // Configure and connect
            ConfigureTransport(ip, connectPort);
            bool success = NetworkManager.Singleton.StartClient();

            if (success)
            {
                SetStatus("Connected! Waiting for host...", StatusType.Success);
                if (showDebugLogs)
                    Debug.Log($"[LobbyMenuManager] Direct connect to {ip}:{connectPort} succeeded");
            }
            else
            {
                SetStatus("Connection failed!\nCheck IP address and try again.", StatusType.Error);
                if (showDebugLogs)
                    Debug.LogError($"[LobbyMenuManager] Direct connect to {ip}:{connectPort} failed");
                
                // Re-enable join button
                if (joinLobbyButton != null)
                    joinLobbyButton.interactable = true;
            }
        }

        private void OnLobbyFound(string ip, int lobbyPort)
        {
            if (showDebugLogs)
                Debug.Log($"[LobbyMenuManager] Lobby found at {ip}:{lobbyPort}. Configuring transport and starting client.");

            SetStatus("Lobby found! Connecting...", StatusType.Success);

            // Configure NetworkManager to connect to host
            ConfigureTransport(ip, (ushort)lobbyPort);
            var transport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
            if (transport != null && showDebugLogs)
            {
                Debug.Log($"[LobbyMenuManager] Client transport after config Address={transport.ConnectionData.Address} Port={transport.ConnectionData.Port}");
            }

            // Start as Client
            bool success = NetworkManager.Singleton.StartClient();

            if (success)
            {
                if (showDebugLogs)
                    Debug.Log("[LobbyMenuManager] StartClient succeeded. Waiting for host scene load.");

                // Store lobby code
                currentLobbyCode = lobbyCodeInputField.text.Trim().ToUpper();
                PlayerPrefs.SetString("LobbyCode", currentLobbyCode);
                PlayerPrefs.Save();

                // Client will auto-follow when host loads LobbyRoom scene
                SetStatus("Connected! Waiting for host...", StatusType.Success);
            }
            else
            {
                Debug.LogError("[LobbyMenuManager] Failed to start as Client!");
                SetStatus("Failed to connect to lobby", StatusType.Error);
                
                // Re-enable join button
                if (joinLobbyButton != null)
                    joinLobbyButton.interactable = true;
            }
        }

        private void OnScanTimeout()
        {
            if (showDebugLogs)
                Debug.LogWarning("[LobbyMenuManager] Lobby scan timed out");

            SetStatus("Lobby not found!\nCheck code or try IP address directly.", StatusType.Error);

            // Re-enable join button
            if (joinLobbyButton != null)
                joinLobbyButton.interactable = true;
        }

        #endregion

        #region Direct Connect

        private void OnDirectConnectClicked()
        {
            if (NetworkManager.Singleton == null)
            {
                SetStatus("Error: Network not initialized", StatusType.Error);
                return;
            }
            string ip = directIpInputField != null ? directIpInputField.text.Trim() : string.Empty;
            if (string.IsNullOrEmpty(ip))
            {
                SetStatus("Enter host IP (e.g., 192.168.1.10)", StatusType.Warning);
                return;
            }
            // Use configured game port
            ushort connectPort = port;
            SetStatus($"Connecting to {ip}:{connectPort}...", StatusType.Searching);
            ConfigureTransport(ip, connectPort);
            bool success = NetworkManager.Singleton.StartClient();
            if (success)
            {
                SetStatus("Connected! Waiting for host...", StatusType.Success);
                if (showDebugLogs) Debug.Log($"[LobbyMenuManager] Direct StartClient to {ip}:{connectPort} succeeded");
            }
            else
            {
                SetStatus("Connection failed! Check IP.", StatusType.Error);
                if (showDebugLogs) Debug.LogError("[LobbyMenuManager] Direct StartClient failed");
            }
        }

        #endregion

        #region Network Helper Methods

        /// <summary>
        /// Configure the UnityTransport with connection data
        /// </summary>
        private void ConfigureTransport(string address, ushort transportPort)
        {
            if (NetworkManager.Singleton == null)
            {
                Debug.LogError("[LobbyMenuManager] Cannot configure transport - NetworkManager not ready!");
                return;
            }

            Unity.Netcode.Transports.UTP.UnityTransport transport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
            if (transport == null)
            {
                Debug.LogError("[LobbyMenuManager] UnityTransport component not found!");
                return;
            }

            transport.SetConnectionData(address, transportPort);

            if (showDebugLogs)
                Debug.Log($"[LobbyMenuManager] Transport configured: {address}:{transportPort}");
        }

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
                Debug.LogWarning($"[LobbyMenuManager] Failed to get local IP: {e.Message}");
            }
            
            return "127.0.0.1";
        }

        private string GetLocalIPAddressVerbose()
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                var ipv4 = host.AddressList.FirstOrDefault(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                if (ipv4 != null)
                {
                    if (showDebugLogs) Debug.Log($"[LobbyMenuManager] Resolved local IPv4: {ipv4}");
                    return ipv4.ToString();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[LobbyMenuManager] GetLocalIPAddressVerbose failed: {e.Message}");
            }
            return "127.0.0.1";
        }

        #endregion

        #region Scene Loading

        private void LoadLobbyRoom()
        {
            if (showDebugLogs)
                Debug.Log($"[LobbyMenuManager] Loading {lobbyRoomSceneName} scene");

            // Host loads the scene using NetworkSceneManager
            // Clients will automatically follow
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                NetworkManager.Singleton.SceneManager.LoadScene(lobbyRoomSceneName, LoadSceneMode.Single);
            }
            else
            {
                Debug.LogWarning("[LobbyMenuManager] Only host can load scenes!");
            }
        }

        #endregion

        #region UI Helpers

        /// <summary>
        /// Status types for color coding
        /// </summary>
        private enum StatusType
        {
            Success,    // Green - operation completed successfully
            Error,      // Red - something went wrong
            Warning,    // Orange - needs attention or input required
            Searching,  // Light Blue - operation in progress
            Info        // White - neutral information
        }

        private void SetStatus(string message, StatusType type)
        {
            Color color = type switch
            {
                StatusType.Success => COLOR_SUCCESS,
                StatusType.Error => COLOR_ERROR,
                StatusType.Warning => COLOR_WARNING,
                StatusType.Searching => COLOR_SEARCHING,
                StatusType.Info => COLOR_INFO,
                _ => COLOR_INFO
            };

            if (statusText != null)
            {
                statusText.text = message;
                statusText.color = color;
            }

            if (showDebugLogs)
                Debug.Log($"[LobbyMenuManager] Status ({type}): {message}");
        }

        // Keep old method for backwards compatibility
        private void SetStatusText(string message, Color color)
        {
            if (statusText != null)
            {
                statusText.text = message;
                statusText.color = color;
            }

            if (showDebugLogs)
                Debug.Log($"[LobbyMenuManager] Status: {message}");
        }

        private void OnBackClicked()
        {
            if (showDebugLogs)
                Debug.Log("[LobbyMenuManager] Back button clicked");

            // Cleanup network if active
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            {
                NetworkManager.Singleton.Shutdown();
            }

            // Stop broadcasting/scanning
            if (broadcaster != null)
                broadcaster.StopBroadcasting();

            if (scanner != null)
                scanner.StopScanning();

            // Return to main menu (adjust scene name as needed)
            SceneManager.LoadScene("MainMenu");
        }

        #endregion

        private void OnDestroy()
        {
            // Cleanup
            if (createLobbyButton != null)
                createLobbyButton.onClick.RemoveListener(OnCreateLobbyClicked);

            if (joinLobbyButton != null)
                joinLobbyButton.onClick.RemoveListener(OnJoinLobbyClicked);

            if (backButton != null)
                backButton.onClick.RemoveListener(OnBackClicked);

            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnAnyClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnAnyClientDisconnected;
            }

            if (directConnectButton != null)
            {
                directConnectButton.onClick.RemoveListener(OnDirectConnectClicked);
            }
        }
    }
}
