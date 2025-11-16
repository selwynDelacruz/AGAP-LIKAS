using Unity.Netcode;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Enhanced Network UI that displays lobby information and handles connection
/// </summary>
public class EnhancedNetworkUI : MonoBehaviour
{
    [Header("Connection Buttons")]
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private Button disconnectButton;

    [Header("Lobby Info Display")]
    [SerializeField] private GameObject lobbyInfoPanel;
    [SerializeField] private TMP_Text taskCountText;
    [SerializeField] private TMP_Text durationText;
    [SerializeField] private TMP_Text disasterTypeText;
    [SerializeField] private TMP_Text playerCountText;
    [SerializeField] private TMP_Text connectionStatusText;

    [Header("Host Controls")]
    [SerializeField] private GameObject hostControlsPanel;
    [SerializeField] private Button startGameButton;

    [Header("Connection Settings")]
    [SerializeField] private TMP_InputField ipAddressInput;

    private void Awake()
    {
        // Set up button listeners
        if (hostButton != null)
            hostButton.onClick.AddListener(OnHostButtonClicked);
        
        if (clientButton != null)
            clientButton.onClick.AddListener(OnClientButtonClicked);
        
        if (disconnectButton != null)
            disconnectButton.onClick.AddListener(OnDisconnectButtonClicked);
        
        if (startGameButton != null)
            startGameButton.onClick.AddListener(OnStartGameButtonClicked);

        // Initially hide lobby info and host controls
        if (lobbyInfoPanel != null)
            lobbyInfoPanel.SetActive(false);
        
        if (hostControlsPanel != null)
            hostControlsPanel.SetActive(false);
        
        if (disconnectButton != null)
            disconnectButton.gameObject.SetActive(false);
    }

    private void Start()
    {
        // Subscribe to NetworkManager events
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    private void Update()
    {
        // Update lobby info display if connected
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient)
        {
            UpdateLobbyDisplay();
        }
    }

    private void OnHostButtonClicked()
    {
        Debug.Log("[UI] Host button clicked");
        
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager.Singleton is null!");
            UpdateConnectionStatus("Error: NetworkManager not found!", Color.red);
            return;
        }

        // Start as host
        bool success = NetworkManager.Singleton.StartHost();
        
        if (success)
        {
            Debug.Log("[UI] Successfully started host");
            UpdateConnectionStatus("Hosting lobby...", Color.green);
            ShowLobbyUI(true);
        }
        else
        {
            Debug.LogError("[UI] Failed to start host");
            UpdateConnectionStatus("Failed to start host!", Color.red);
        }
    }

    private void OnClientButtonClicked()
    {
        Debug.Log("[UI] Client button clicked");
        
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager.Singleton is null!");
            UpdateConnectionStatus("Error: NetworkManager not found!", Color.red);
            return;
        }

        // Set IP address if provided
        if (ipAddressInput != null && !string.IsNullOrEmpty(ipAddressInput.text))
        {
            var transport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
            if (transport != null)
            {
                transport.SetConnectionData(ipAddressInput.text, 7777);
                Debug.Log($"[UI] Connecting to {ipAddressInput.text}:7777");
            }
        }

        // Start as client
        bool success = NetworkManager.Singleton.StartClient();
        
        if (success)
        {
            Debug.Log("[UI] Successfully started client");
            UpdateConnectionStatus("Connecting to host...", Color.yellow);
        }
        else
        {
            Debug.LogError("[UI] Failed to start client");
            UpdateConnectionStatus("Failed to connect!", Color.red);
        }
    }

    private void OnDisconnectButtonClicked()
    {
        Debug.Log("[UI] Disconnect button clicked");
        
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
            UpdateConnectionStatus("Disconnected", Color.white);
            ShowLobbyUI(false);
        }
    }

    private void OnStartGameButtonClicked()
    {
        Debug.Log("[UI] Start Game button clicked");
        
        if (NetworkLobbyManager.Instance != null && NetworkLobbyManager.Instance.CanStartGame())
        {
            NetworkLobbyManager.Instance.StartGame();
        }
        else
        {
            Debug.LogWarning("Cannot start game - not enough players or not host");
            UpdateConnectionStatus("Cannot start game!", Color.red);
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"[UI] Client {clientId} connected");
        
        // Notify the lobby manager
        if (NetworkLobbyManager.Instance != null)
        {
            NetworkLobbyManager.Instance.OnClientConnected(clientId);
        }

        // If we're the client that just connected
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            UpdateConnectionStatus("Connected to lobby!", Color.green);
            ShowLobbyUI(false); // Clients don't see host controls
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"[UI] Client {clientId} disconnected");
        
        // Notify the lobby manager
        if (NetworkLobbyManager.Instance != null)
        {
            NetworkLobbyManager.Instance.OnClientDisconnected(clientId);
        }

        // If we're the client that disconnected
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.LocalClientId == clientId)
        {
            UpdateConnectionStatus("Disconnected from lobby", Color.white);
            ShowLobbyUI(false);
        }
    }

    private void ShowLobbyUI(bool isHost)
    {
        // Show/hide connection buttons
        if (hostButton != null)
            hostButton.gameObject.SetActive(false);
        
        if (clientButton != null)
            clientButton.gameObject.SetActive(false);
        
        if (disconnectButton != null)
            disconnectButton.gameObject.SetActive(true);

        // Show lobby info panel
        if (lobbyInfoPanel != null)
            lobbyInfoPanel.SetActive(true);

        // Show host controls only if host
        if (hostControlsPanel != null)
            hostControlsPanel.SetActive(isHost);
    }

    private void UpdateLobbyDisplay()
    {
        if (NetworkLobbyManager.Instance == null) return;

        // Update task count
        if (taskCountText != null)
            taskCountText.text = $"Tasks: {NetworkLobbyManager.Instance.GetTaskCount()}";

        // Update duration
        if (durationText != null)
        {
            int duration = NetworkLobbyManager.Instance.GetDuration();
            int minutes = duration / 60;
            durationText.text = $"Duration: {minutes} minutes";
        }

        // Update disaster type
        if (disasterTypeText != null)
            disasterTypeText.text = $"Scenario: {NetworkLobbyManager.Instance.GetDisasterType()}";

        // Update player count
        if (playerCountText != null)
        {
            int current = NetworkLobbyManager.Instance.GetConnectedPlayerCount();
            int max = NetworkLobbyManager.Instance.GetMaxPlayers();
            playerCountText.text = $"Players: {current}/{max}";
        }

        // Update start button interactability
        if (startGameButton != null && NetworkLobbyManager.Instance.IsServer)
        {
            startGameButton.interactable = NetworkLobbyManager.Instance.CanStartGame();
        }
    }

    private void UpdateConnectionStatus(string message, Color color)
    {
        if (connectionStatusText != null)
        {
            connectionStatusText.text = message;
            connectionStatusText.color = color;
        }
        Debug.Log($"[UI Status] {message}");
    }
}
