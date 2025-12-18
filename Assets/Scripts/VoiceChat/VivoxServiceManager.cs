using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Vivox;
using System;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
#if AUTH_PACKAGE_PRESENT
using Unity.Services.Authentication;
#endif

/// <summary>
/// Manages Vivox service initialization and authentication for AGAP-LIKAS voice chat system.
/// This is a persistent singleton that handles the global Vivox setup.
/// Scene-specific voice chat logic should use GameVoiceChatManager instead.
/// </summary>
public class VivoxServiceManager : MonoBehaviour
{
    public const string GameChannelName = "gameChannel";

    static object m_Lock = new object();
    static VivoxServiceManager m_Instance;

    [Header("Vivox Credentials (Optional - use Authentication if empty)")]
    [Tooltip("Leave empty to automatically use credentials from Project Settings > Services > Vivox")]
    [SerializeField] private string _key = "";
    [SerializeField] private string _issuer = "";
    [SerializeField] private string _domain = "";
    [SerializeField] private string _server = "";

    [Header("Debug Settings")]
    [SerializeField] private bool showDebugLogs = true;

    private bool isInitialized = false;
    private bool isLoggingIn = false;
    private string currentPlayerName = "";
    private InitializationOptions storedInitOptions = null;

    /// <summary>
    /// Access singleton instance through this property.
    /// </summary>
    public static VivoxServiceManager Instance
    {
        get
        {
            lock (m_Lock)
            {
                if (m_Instance == null)
                {
                    m_Instance = (VivoxServiceManager)FindFirstObjectByType(typeof(VivoxServiceManager));

                    if (m_Instance == null)
                    {
                        var singletonObject = new GameObject("VivoxServiceManager (Singleton)");
                        m_Instance = singletonObject.AddComponent<VivoxServiceManager>();
                    }
                }
                DontDestroyOnLoad(m_Instance.gameObject);
                return m_Instance;
            }
        }
    }

    /// <summary>
    /// Check if Vivox service is initialized and ready
    /// </summary>
    public bool IsInitialized => isInitialized && VivoxService.Instance != null;

    async void Awake()
    {
        if (m_Instance != null && m_Instance != this)
        {
            if (showDebugLogs)
                Debug.LogWarning("[VivoxServiceManager] Multiple instances detected. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        m_Instance = this;
        DontDestroyOnLoad(gameObject);

        // ADD THIS LINE
        CheckVivoxConfiguration();

        SceneManager.sceneLoaded += OnSceneLoaded;
        await InitializeVivoxService();
    }

    /// <summary>
    /// Called when a scene is loaded - ensures Vivox stays initialized
    /// </summary>
    private async void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (showDebugLogs)
            Debug.Log($"[VivoxServiceManager] Scene loaded: {scene.name}");

        // Give Unity a moment to fully load the scene
        await Task.Delay(200);

        if (storedInitOptions != null && UnityServices.State == ServicesInitializationState.Initialized)
        {
            try
            {
                if (showDebugLogs)
                    Debug.Log("[VivoxServiceManager] Re-applying Vivox credentials after scene load...");

                // Force re-initialization of VivoxService
                if (VivoxService.Instance != null)
                {
                    await VivoxService.Instance.InitializeAsync();
                    
                    // CRITICAL: Extra delay to let Vivox stabilize
                    await Task.Delay(500);
                    
                    if (showDebugLogs)
                        Debug.Log("[VivoxServiceManager] VivoxService re-initialized after scene load");
                }
                
                // Re-login if we had a player name
                if (!string.IsNullOrEmpty(currentPlayerName) && !IsLoggedIn())
                {
                    if (showDebugLogs)
                        Debug.Log($"[VivoxServiceManager] Re-logging in as: {currentPlayerName}");
                    
                    await LoginAsync(currentPlayerName);
                    
                    // CRITICAL: Extra delay after login
                    await Task.Delay(500);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[VivoxServiceManager] Error during scene transition re-initialization: {e.Message}");
            }
        }
    }

    /// <summary>
    /// Initialize Unity Services and Vivox
    /// </summary>
    private async Task InitializeVivoxService()
    {
        try
        {
            if (showDebugLogs)
                Debug.Log("[VivoxServiceManager] Starting Vivox initialization...");

            var options = new InitializationOptions();

            // Check if manual credentials are provided in Inspector
            if (CheckManualCredentials())
            {
                options.SetVivoxCredentials(_server, _domain, _issuer, _key);
                storedInitOptions = options;

                if (showDebugLogs)
                    Debug.Log("[VivoxServiceManager] Using manual Vivox credentials from Inspector");
            }
            else
            {
                // Unity will automatically use credentials from Project Settings
                storedInitOptions = options;

                if (showDebugLogs)
                    Debug.Log("[VivoxServiceManager] Using Vivox credentials from Project Settings (auto-configured)");
            }

            // Initialize Unity Services
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                await UnityServices.InitializeAsync(options);
                if (showDebugLogs)
                    Debug.Log("[VivoxServiceManager] Unity Services initialized");
            }
            else if (showDebugLogs)
            {
                Debug.Log($"[VivoxServiceManager] Unity Services already initialized. State: {UnityServices.State}");
            }

            // Verify VivoxService.Instance is available
            if (VivoxService.Instance == null)
            {
                Debug.LogError("[VivoxServiceManager] VivoxService.Instance is null after Unity Services initialization!");
                Debug.LogError("Make sure Test Mode is enabled in Project Settings > Services > Vivox");
                isInitialized = false;
                return;
            }

            // Initialize Vivox
            await VivoxService.Instance.InitializeAsync();
            
            if (showDebugLogs)
                Debug.Log("[VivoxServiceManager] Vivox service initialized successfully");

            // CRITICAL: Wait a bit for Vivox to fully stabilize after initialization
            await Task.Delay(500);

            isInitialized = true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[VivoxServiceManager] Failed to initialize Vivox: {e.Message}\n{e.StackTrace}");
            isInitialized = false;
        }
    }

    /// <summary>
    /// Initialize authentication and login to Vivox with a specific player name
    /// </summary>
    /// <param name="playerName">Unique player identifier</param>
    public async Task LoginAsync(string playerName)
    {
        if (!isInitialized)
        {
            Debug.LogError("[VivoxServiceManager] Cannot login - Vivox not initialized!");
            return;
        }

        if (VivoxService.Instance == null)
        {
            Debug.LogError("[VivoxServiceManager] Cannot login - VivoxService.Instance is null!");
            await InitializeVivoxService();
            
            if (VivoxService.Instance == null)
            {
                Debug.LogError("[VivoxServiceManager] Re-initialization failed!");
                return;
            }
        }

        if (isLoggingIn)
        {
            if (showDebugLogs)
                Debug.LogWarning("[VivoxServiceManager] Already logging in, skipping duplicate request");
            return;
        }

        try
        {
            isLoggingIn = true;
            currentPlayerName = playerName;

#if AUTH_PACKAGE_PRESENT
            // Use Unity Authentication for token generation
            if (showDebugLogs)
                Debug.Log($"[VivoxServiceManager] Switching authentication profile to: {playerName}");

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                AuthenticationService.Instance.SwitchProfile(playerName);
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

                if (showDebugLogs)
                    Debug.Log($"[VivoxServiceManager] Signed in anonymously as: {playerName}");
            }
            else if (showDebugLogs)
            {
                Debug.Log("[VivoxServiceManager] Already signed in to Authentication Service");
            }
#endif

            // Login to Vivox
            if (!VivoxService.Instance.IsLoggedIn)
            {
                await VivoxService.Instance.LoginAsync();

                if (showDebugLogs)
                    Debug.Log($"[VivoxServiceManager] Logged in to Vivox as: {playerName}");
            }
            else if (showDebugLogs)
            {
                Debug.Log("[VivoxServiceManager] Already logged in to Vivox");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[VivoxServiceManager] Login failed: {e.Message}\n{e.StackTrace}");
            throw;
        }
        finally
        {
            isLoggingIn = false;
        }
    }

    /// <summary>
    /// Logout from Vivox service
    /// </summary>
    public async Task LogoutAsync()
    {
        if (!isInitialized || VivoxService.Instance == null)
            return;

        try
        {
            if (VivoxService.Instance.IsLoggedIn)
            {
                await VivoxService.Instance.LogoutAsync();

                if (showDebugLogs)
                    Debug.Log("[VivoxServiceManager] Logged out from Vivox");
            }
            
            currentPlayerName = "";
        }
        catch (Exception e)
        {
            Debug.LogError($"[VivoxServiceManager] Logout failed: {e.Message}");
        }
    }

    /// <summary>
    /// Check if manual credentials are provided
    /// </summary>
    private bool CheckManualCredentials()
    {
        bool hasCredentials = !string.IsNullOrEmpty(_issuer) && 
                             !string.IsNullOrEmpty(_domain) && 
                             !string.IsNullOrEmpty(_server);
        // Note: _key is optional for production (uses Unity Authentication tokens)

        return hasCredentials;
    }

    /// <summary>
    /// Check if currently logged in to Vivox
    /// </summary>
    public bool IsLoggedIn()
    {
        if (!isInitialized || VivoxService.Instance == null)
            return false;

        return VivoxService.Instance.IsLoggedIn;
    }

    /// <summary>
    /// Force re-initialization of Vivox (for troubleshooting)
    /// </summary>
    public async Task ForceReinitialize()
    {
        if (showDebugLogs)
            Debug.Log("[VivoxServiceManager] Force re-initializing Vivox...");
        
        isInitialized = false;
        
        // Re-initialize Unity Services with stored credentials
        if (storedInitOptions != null && UnityServices.State != ServicesInitializationState.Uninitialized)
        {
            // Unity Services is already initialized, just reinit Vivox
            if (VivoxService.Instance != null)
            {
                await VivoxService.Instance.InitializeAsync();
            }
        }
        else
        {
            await InitializeVivoxService();
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from scene loaded event
        SceneManager.sceneLoaded -= OnSceneLoaded;
        
        if (m_Instance == this)
        {
            m_Instance = null;
        }
    }

    private void OnApplicationQuit()
    {
        if (isInitialized && IsLoggedIn())
        {
            _ = LogoutAsync();
        }
    }

    private void CheckVivoxConfiguration()
    {
        if (showDebugLogs)
        {
            Debug.Log("=== Vivox Configuration Check ===");
            Debug.Log($"Manual Server: {(string.IsNullOrEmpty(_server) ? "NOT SET (will use auto-config)" : _server)}");
            Debug.Log($"Manual Domain: {(string.IsNullOrEmpty(_domain) ? "NOT SET (will use auto-config)" : _domain)}");
            Debug.Log($"Manual Issuer: {(string.IsNullOrEmpty(_issuer) ? "NOT SET (will use auto-config)" : _issuer)}");
            Debug.Log($"Manual Key: {(string.IsNullOrEmpty(_key) ? "NOT SET (will use Unity Auth tokens)" : "SET")}");
            Debug.Log($"Unity Services State: {UnityServices.State}");
            
            if (!string.IsNullOrEmpty(Application.cloudProjectId))
            {
                Debug.Log($"Cloud Project ID: {Application.cloudProjectId}");
            }
            else
            {
                Debug.LogWarning("Cloud Project ID is NULL - Project not linked!");
            }
            
            Debug.Log("================================");
        }
    }
}
