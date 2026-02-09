using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles host/server disconnection events and returns clients to lobby menu
/// Attach this to the NetworkManager GameObject or a persistent manager
/// </summary>
public class HostDisconnectHandler : MonoBehaviour
{
    [Header("Disconnect Settings")]
    [Tooltip("Scene name to return to when host disconnects")]
    [SerializeField] private string lobbySceneName = "LobbyMenu";

    [Tooltip("Time in seconds to wait before returning to lobby")]
    [SerializeField] private float disconnectDelay = 1f;

    [Tooltip("Enable debug logging")]
    [SerializeField] private bool showDebugLogs = true;

    private bool isHandlingDisconnect = false;

    private void OnEnable()
    {
        // Subscribe to network events when this component is enabled
        if (NetworkManager.Singleton != null)
        {
            SubscribeToNetworkEvents();
        }
    }

    private void OnDisable()
    {
        // Unsubscribe from network events to prevent memory leaks
        UnsubscribeFromNetworkEvents();
    }

    private void Start()
    {
        // If NetworkManager wasn't ready in OnEnable, try again in Start
        if (NetworkManager.Singleton != null && !HasSubscribedToEvents())
        {
            SubscribeToNetworkEvents();
        }
    }

    /// <summary>
    /// Subscribe to network disconnect events
    /// </summary>
    private void SubscribeToNetworkEvents()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogWarning("[HostDisconnectHandler] NetworkManager.Singleton is null. Cannot subscribe to events.");
            return;
        }

        // Subscribe to server stopped event (fires on clients when server shuts down)
        NetworkManager.Singleton.OnServerStopped += OnServerStopped;

        // Subscribe to client disconnect event (fires when THIS client disconnects)
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

        if (showDebugLogs)
        {
            Debug.Log("[HostDisconnectHandler] Subscribed to network disconnect events");
        }
    }

    /// <summary>
    /// Unsubscribe from network events
    /// </summary>
    private void UnsubscribeFromNetworkEvents()
    {
        if (NetworkManager.Singleton == null)
            return;

        NetworkManager.Singleton.OnServerStopped -= OnServerStopped;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;

        if (showDebugLogs)
        {
            Debug.Log("[HostDisconnectHandler] Unsubscribed from network disconnect events");
        }
    }

    /// <summary>
    /// Check if already subscribed to events
    /// </summary>
    private bool HasSubscribedToEvents()
    {
        // Simple check - you can enhance this if needed
        return NetworkManager.Singleton != null;
    }

    /// <summary>
    /// Called when the server stops (host leaves/disconnects)
    /// This fires on ALL clients when the server shuts down
    /// </summary>
    /// <param name="isHost">True if this was the host that stopped</param>
    private void OnServerStopped(bool isHost)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[HostDisconnectHandler] Server stopped! IsHost: {isHost}");
        }

        // Only handle on clients (not on the host itself)
        // When host stops, isHost will be true, but we want clients to return to lobby
        if (!NetworkManager.Singleton.IsHost && !isHandlingDisconnect)
        {
            HandleHostDisconnect("Server/Host has stopped");
        }
    }

    /// <summary>
    /// Called when a client disconnects from the server
    /// This includes when THIS client disconnects
    /// </summary>
    /// <param name="clientId">The ID of the client that disconnected</param>
    private void OnClientDisconnected(ulong clientId)
    {
        // Check if this client (us) disconnected
        if (NetworkManager.Singleton != null && clientId == NetworkManager.Singleton.LocalClientId)
        {
            if (showDebugLogs)
            {
                Debug.Log($"[HostDisconnectHandler] Local client ({clientId}) disconnected from server");
            }

            // Only handle if we're not the host and not already handling
            if (!NetworkManager.Singleton.IsHost && !isHandlingDisconnect)
            {
                HandleHostDisconnect("Disconnected from host");
            }
        }
        else if (showDebugLogs)
        {
            Debug.Log($"[HostDisconnectHandler] Client {clientId} disconnected (not us)");
        }
    }

    /// <summary>
    /// Handles the host disconnect by returning to lobby menu
    /// </summary>
    /// <param name="reason">Reason for disconnect</param>
    private void HandleHostDisconnect(string reason)
    {
        if (isHandlingDisconnect)
        {
            if (showDebugLogs)
            {
                Debug.LogWarning("[HostDisconnectHandler] Already handling disconnect. Skipping.");
            }
            return;
        }

        isHandlingDisconnect = true;

        if (showDebugLogs)
        {
            Debug.Log($"[HostDisconnectHandler] Handling disconnect: {reason}. Returning to {lobbySceneName} in {disconnectDelay}s");
        }

        // Optional: Show a message to the player
        // You can create a UI message here if needed
        // Example: UIManager.ShowMessage("Host disconnected. Returning to lobby...");

        // Start coroutine to delay scene transition
        StartCoroutine(ReturnToLobbyAfterDelay(reason));
    }

    /// <summary>
    /// Coroutine to wait before returning to lobby
    /// </summary>
    private System.Collections.IEnumerator ReturnToLobbyAfterDelay(string reason)
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(disconnectDelay);

        if (showDebugLogs)
        {
            Debug.Log($"[HostDisconnectHandler] Returning to lobby: {reason}");
        }

        // Shutdown networking
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
            if (showDebugLogs)
            {
                Debug.Log("[HostDisconnectHandler] NetworkManager shutdown complete");
            }
        }

        // Enable cursor for menu
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Load lobby scene
        SceneManager.LoadScene(lobbySceneName);

        if (showDebugLogs)
        {
            Debug.Log($"[HostDisconnectHandler] Loading scene: {lobbySceneName}");
        }
    }

    /// <summary>
    /// Public method to manually trigger return to lobby
    /// Can be called from UI buttons or other scripts
    /// </summary>
    public void ManuallyReturnToLobby()
    {
        if (showDebugLogs)
        {
            Debug.Log("[HostDisconnectHandler] Manual return to lobby triggered");
        }

        if (!isHandlingDisconnect)
        {
            HandleHostDisconnect("Manual return to lobby");
        }
    }
}
