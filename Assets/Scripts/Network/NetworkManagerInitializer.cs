using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

/// <summary>
/// Initializes and manages the NetworkManager singleton
/// Ensures it persists across scenes and only exists once
/// </summary>
public class NetworkManagerInitializer : MonoBehaviour
{
    [Header("NetworkManager Prefab")]
    [Tooltip("Reference to the NetworkManager prefab")]
    public GameObject networkManagerPrefab;

    [Header("Debug Settings")]
    [Tooltip("Enable detailed logging")]
    public bool debugMode = true;

    private static NetworkManagerInitializer _instance;
    private static bool _isNetworkManagerInitialized = false;

    private void Awake()
    {
        // Singleton pattern - only one initializer should exist
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            if (debugMode)
            {
                Debug.Log("[NetworkManagerInitializer] Initializer created and set to DontDestroyOnLoad");
            }
        }
        else
        {
            if (debugMode)
            {
                Debug.Log("[NetworkManagerInitializer] Duplicate initializer destroyed");
            }
            Destroy(gameObject);
            return;
        }

        // Initialize NetworkManager if not already done
        if (!_isNetworkManagerInitialized)
        {
            InitializeNetworkManager();
        }
    }

    /// <summary>
    /// Creates and initializes the NetworkManager
    /// </summary>
    private void InitializeNetworkManager()
    {
        // Check if NetworkManager already exists in the scene
        if (NetworkManager.Singleton != null)
        {
            if (debugMode)
            {
                Debug.Log("[NetworkManagerInitializer] NetworkManager already exists");
            }
            _isNetworkManagerInitialized = true;
            return;
        }

        // Instantiate NetworkManager from prefab
        if (networkManagerPrefab == null)
        {
            Debug.LogError("[NetworkManagerInitializer] NetworkManager prefab is not assigned!");
            return;
        }

        GameObject networkManagerObject = Instantiate(networkManagerPrefab);
        networkManagerObject.name = "NetworkManager (Persistent)";
        
        // Ensure it persists across scenes
        DontDestroyOnLoad(networkManagerObject);

        // Verify components
        NetworkManager networkManager = networkManagerObject.GetComponent<NetworkManager>();
        UnityTransport transport = networkManagerObject.GetComponent<UnityTransport>();

        if (networkManager == null)
        {
            Debug.LogError("[NetworkManagerInitializer] NetworkManager component not found on prefab!");
            return;
        }

        if (transport == null)
        {
            Debug.LogError("[NetworkManagerInitializer] UnityTransport component not found on prefab!");
            return;
        }

        _isNetworkManagerInitialized = true;

        if (debugMode)
        {
            Debug.Log($"[NetworkManagerInitializer] NetworkManager initialized successfully");
            Debug.Log($"[NetworkManagerInitializer] Transport: {transport.GetType().Name}");
            Debug.Log($"[NetworkManagerInitializer] Default Port: {transport.ConnectionData.Port}");
        }
    }

    /// <summary>
    /// Public method to ensure NetworkManager is initialized
    /// Can be called from other scripts before using NetworkManager
    /// </summary>
    public static void EnsureNetworkManagerExists()
    {
        if (_instance == null)
        {
            Debug.LogWarning("[NetworkManagerInitializer] Creating initializer on demand");
            GameObject initializerObject = new GameObject("NetworkManagerInitializer");
            initializerObject.AddComponent<NetworkManagerInitializer>();
        }

        if (!_isNetworkManagerInitialized && _instance != null)
        {
            _instance.InitializeNetworkManager();
        }
    }

    /// <summary>
    /// Check if NetworkManager is ready to use
    /// </summary>
    /// <returns>True if NetworkManager is initialized and ready</returns>
    public static bool IsNetworkManagerReady()
    {
        return NetworkManager.Singleton != null;
    }

    /// <summary>
    /// Gets the UnityTransport component safely
    /// </summary>
    /// <returns>UnityTransport or null if not found</returns>
    public static UnityTransport GetTransport()
    {
        if (!IsNetworkManagerReady())
        {
            Debug.LogError("[NetworkManagerInitializer] NetworkManager not ready!");
            return null;
        }

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (transport == null)
        {
            Debug.LogError("[NetworkManagerInitializer] UnityTransport component not found!");
        }

        return transport;
    }

    /// <summary>
    /// Configures the transport with connection data
    /// </summary>
    /// <param name="address">IP address to connect to (use "0.0.0.0" for host)</param>
    /// <param name="port">Port number (default 7777)</param>
    public static void ConfigureTransport(string address, ushort port = 7777)
    {
        UnityTransport transport = GetTransport();
        if (transport == null)
        {
            Debug.LogError("[NetworkManagerInitializer] Cannot configure transport - not available");
            return;
        }

        transport.SetConnectionData(address, port);

        if (_instance != null && _instance.debugMode)
        {
            Debug.Log($"[NetworkManagerInitializer] Transport configured: {address}:{port}");
        }
    }

    /// <summary>
    /// Gets the local IP address of this machine
    /// </summary>
    /// <returns>Local IP address or 127.0.0.1 if not found</returns>
    public static string GetLocalIPAddress()
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
            Debug.LogWarning($"[NetworkManagerInitializer] Failed to get local IP: {e.Message}");
        }
        
        return "127.0.0.1";
    }

    private void OnApplicationQuit()
    {
        // Cleanup when application quits
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            if (debugMode)
            {
                Debug.Log("[NetworkManagerInitializer] Shutting down NetworkManager");
            }
            NetworkManager.Singleton.Shutdown();
        }
    }
}
