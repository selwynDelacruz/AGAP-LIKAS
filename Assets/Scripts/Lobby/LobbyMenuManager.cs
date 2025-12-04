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
                SetStatusText("Error: Network not initialized", Color.red);
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
                SetStatusText("Error: User role not set. Please log in again.", Color.red);
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
                SetStatusText("Error: Network not initialized", Color.red);
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
                lobbyCodeDisplayText.gameObject.SetActive(true); // Make sure it's visible
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

                // Get local IP for broadcasting
                string localIP = GetLocalIPAddressVerbose();

                // Start broadcasting lobby code on LAN
                broadcaster = gameObject.AddComponent<LobbyBroadcaster>();
                broadcaster.StartBroadcasting(currentLobbyCode, localIP, port);

                // Show status message
                SetStatusText($"Lobby Created! Code: {currentLobbyCode}\nLoading lobby room...", Color.green);

                // Wait 2 seconds before loading lobby room so user can see the code
                Invoke(nameof(LoadLobbyRoom), 2f);
            }
            else
            {
                Debug.LogError("[LobbyMenuManager] Failed to start as Host!");
                SetStatusText("Failed to create lobby", Color.red);
                
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
                SetStatusText("Error: Network not initialized", Color.red);
                return;
            }

            // Get lobby code from input field
            string enteredCode = lobbyCodeInputField.text.Trim().ToUpper();

            // Validate code format
            if (!LobbyCodeGenerator.ValidateCode(enteredCode))
            {
                SetStatusText("Invalid lobby code format!", Color.red);
                return;
            }

            if (showDebugLogs)
                Debug.Log($"[LobbyMenuManager] Searching for lobby: {enteredCode}");

            // Show searching status
            SetStatusText($"Searching for lobby {enteredCode}...", Color.yellow);

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

        private void OnLobbyFound(string ip, int lobbyPort)
        {
            if (showDebugLogs)
                Debug.Log($"[LobbyMenuManager] Lobby found at {ip}:{lobbyPort}. Configuring transport and starting client.");

            SetStatusText("Lobby found! Connecting...", Color.green);

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
                SetStatusText("Connected! Waiting for host...", Color.green);

                // Note: Don't load scene here - client will follow host automatically
            }
            else
            {
                Debug.LogError("[LobbyMenuManager] Failed to start as Client!");
                SetStatusText("Failed to connect to lobby", Color.red);
                
                // Re-enable join button
                if (joinLobbyButton != null)
                    joinLobbyButton.interactable = true;
            }
        }

        private void OnScanTimeout()
        {
            if (showDebugLogs)
                Debug.LogWarning("[LobbyMenuManager] Lobby scan timed out");

            SetStatusText("Lobby not found! Check code and try again.", Color.red);

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
                SetStatusText("Error: Network not initialized", Color.red);
                return;
            }
            string ip = directIpInputField != null ? directIpInputField.text.Trim() : string.Empty;
            if (string.IsNullOrEmpty(ip))
            {
                SetStatusText("Enter host LAN IP (e.g., 192.168.1.10)", Color.yellow);
                return;
            }
            // Use configured game port
            ushort connectPort = port;
            SetStatusText($"Direct connect to {ip}:{connectPort}...", Color.yellow);
            ConfigureTransport(ip, connectPort);
            bool success = NetworkManager.Singleton.StartClient();
            if (success)
            {
                SetStatusText("Connected! Waiting for host...", Color.green);
                if (showDebugLogs) Debug.Log($"[LobbyMenuManager] Direct StartClient to {ip}:{connectPort} succeeded");
            }
            else
            {
                SetStatusText("Direct connect failed", Color.red);
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
