using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using StarterAssets;

public class PauseMenu : NetworkBehaviour
{
    [Header("Pause Menu UI")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button pauseButton; // Button to trigger pause

    [Header("Trainee Pause Display")]
    [SerializeField] private GameObject traineesPausePanel; // Optional: Separate panel for trainees showing "Game Paused by Instructor"

    [Header("Player GUI to Hide")]
    [SerializeField] private GameObject playerGuiParent; // Parent GameObject containing player UI elements

    [Header("Settings")]
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
    [SerializeField] private bool showDebugLogs = true;
    [SerializeField] private bool restrictToInstructorOnly = true;
    [SerializeField] private bool disableCameraRotationWhenPaused = true; // NEW: Disable camera rotation during pause

    // Network variable to sync pause state across all clients
    private NetworkVariable<bool> isPausedNetworked = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private float originalTimeScale = 1f;
    private bool isInstructor = false;
    private bool isReturningToMainMenu = false; // Prevent multiple calls

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Subscribe to network variable changes
        isPausedNetworked.OnValueChanged += OnPauseStateChanged;

        // Apply current pause state if joining mid-game
        if (!IsServer)
        {
            ApplyPauseState(isPausedNetworked.Value);
        }

        if (showDebugLogs)
            Debug.Log($"[PauseMenu] Network spawned. IsServer={IsServer} IsHost={IsHost}");
    }

    private void Start()
    {
        // Check user role from PlayerPrefs
        CheckUserRole();

        // Setup button listeners
        if (resumeButton != null)
            resumeButton.onClick.AddListener(OnResumeClicked);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);

        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(OnPauseButtonClicked);
            
            // Hide pause button if user is not instructor
            if (restrictToInstructorOnly && !isInstructor)
            {
                pauseButton.gameObject.SetActive(false);
                if (showDebugLogs)
                    Debug.Log("[PauseMenu] Pause button hidden - User is not an instructor");
            }
        }

        // Initially hide pause menus
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        if (traineesPausePanel != null)
            traineesPausePanel.SetActive(false);

        // Store original time scale
        originalTimeScale = Time.timeScale;

        if (showDebugLogs)
            Debug.Log($"[PauseMenu] Initialized. User role: {(isInstructor ? "Instructor" : "Trainee")}. Press {pauseKey} to pause.");
    }

    /// <summary>
    /// Check the user role from PlayerPrefs to determine if they are an instructor
    /// </summary>
    private void CheckUserRole()
    {
        string userType = PlayerPrefs.GetString("Type_Of_User", "").ToLower();
        isInstructor = (userType == "instructor");

        if (showDebugLogs)
            Debug.Log($"[PauseMenu] User type detected: '{userType}' | Is Instructor: {isInstructor}");
    }

    private void Update()
    {
        // Only allow pause if user is instructor (or if restriction is disabled)
        if (!restrictToInstructorOnly || isInstructor)
        {
            // Check for pause input
            if (Input.GetKeyDown(pauseKey))
            {
                if (isPausedNetworked.Value)
                    ResumeGame();
                else
                    PauseGame();
            }
        }
    }

    /// <summary>
    /// Pauses the game for all clients
    /// Only works for instructors when restrictToInstructorOnly is enabled
    /// </summary>
    public void PauseGame()
    {
        // Check if user has permission to pause
        if (restrictToInstructorOnly && !isInstructor)
        {
            if (showDebugLogs)
                Debug.LogWarning("[PauseMenu] Pause denied - Only instructors can pause the game");
            return;
        }

        if (isPausedNetworked.Value)
            return;

        if (showDebugLogs)
            Debug.Log("[PauseMenu] Requesting pause...");

        // Request server to pause the game
        if (IsServer)
        {
            SetPauseStateServerRpc(true);
        }
        else
        {
            RequestPauseServerRpc(true);
        }
    }

    /// <summary>
    /// Resumes the game for all clients
    /// </summary>
    public void ResumeGame()
    {
        if (!isPausedNetworked.Value)
            return;

        if (showDebugLogs)
            Debug.Log("[PauseMenu] Requesting resume...");

        // Request server to resume the game
        if (IsServer)
        {
            SetPauseStateServerRpc(false);
        }
        else
        {
            RequestPauseServerRpc(false);
        }
    }

    /// <summary>
    /// ServerRpc called by clients to request pause/resume
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void RequestPauseServerRpc(bool shouldPause, ServerRpcParams rpcParams = default)
    {
        // Server validates the request and updates the network variable
        SetPauseStateServerRpc(shouldPause);
    }

    /// <summary>
    /// Server sets the pause state, which syncs to all clients
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void SetPauseStateServerRpc(bool shouldPause)
    {
        isPausedNetworked.Value = shouldPause;

        if (showDebugLogs)
            Debug.Log($"[PauseMenu] Server set pause state to: {shouldPause}");
    }

    /// <summary>
    /// Called on all clients when the pause state changes
    /// </summary>
    private void OnPauseStateChanged(bool previousValue, bool newValue)
    {
        if (showDebugLogs)
            Debug.Log($"[PauseMenu] Pause state changed: {previousValue} -> {newValue}");

        ApplyPauseState(newValue);
    }

    /// <summary>
    /// Applies the pause/resume state locally on this client
    /// </summary>
    private void ApplyPauseState(bool shouldPause)
    {
        if (shouldPause)
        {
            // Freeze game time
            Time.timeScale = 0f;

            // Disable camera rotation for all players
            if (disableCameraRotationWhenPaused)
            {
                LockAllPlayerCameras(true);
            }

            // Show appropriate pause UI based on user role
            if (isInstructor)
            {
                // Instructor sees full pause menu with buttons
                if (pauseMenuPanel != null)
                {
                    pauseMenuPanel.SetActive(true);
                    
                    // Ensure buttons are visible for instructor
                    if (resumeButton != null)
                        resumeButton.gameObject.SetActive(true);
                    
                    if (mainMenuButton != null)
                        mainMenuButton.gameObject.SetActive(true);
                }

                // Hide trainee panel if it exists
                if (traineesPausePanel != null)
                    traineesPausePanel.SetActive(false);
            }
            else
            {
                // Trainee sees either a separate panel or pause menu with hidden buttons
                if (traineesPausePanel != null)
                {
                    // Option 1: Show separate trainee pause panel (recommended)
                    traineesPausePanel.SetActive(true);
                    
                    // Hide instructor pause menu
                    if (pauseMenuPanel != null)
                        pauseMenuPanel.SetActive(false);
                }
                else
                {
                    // Option 2: Show same panel but hide the buttons
                    if (pauseMenuPanel != null)
                        pauseMenuPanel.SetActive(true);
                    
                    // Hide buttons for trainees
                    if (resumeButton != null)
                        resumeButton.gameObject.SetActive(false);
                    
                    if (mainMenuButton != null)
                        mainMenuButton.gameObject.SetActive(false);
                }
            }

            // Hide player GUI
            if (playerGuiParent != null)
                playerGuiParent.SetActive(false);

            // Unlock and show cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (showDebugLogs)
                Debug.Log($"[PauseMenu] Game PAUSED locally. Time.timeScale = 0 | IsInstructor: {isInstructor} | Camera Locked: {disableCameraRotationWhenPaused}");
        }
        else
        {
            // Restore game time
            Time.timeScale = originalTimeScale;

            // Re-enable camera rotation for all players
            if (disableCameraRotationWhenPaused)
            {
                LockAllPlayerCameras(false);
            }

            // Hide pause menus
            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(false);

            if (traineesPausePanel != null)
                traineesPausePanel.SetActive(false);

            // Show player GUI
            if (playerGuiParent != null)
                playerGuiParent.SetActive(true);

            // Lock and hide cursor (adjust based on your game's needs)
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (showDebugLogs)
                Debug.Log("[PauseMenu] Game RESUMED locally. Time.timeScale = " + originalTimeScale);
        }
    }

    /// <summary>
    /// Locks or unlocks camera rotation for all player controllers in the scene
    /// </summary>
    private void LockAllPlayerCameras(bool shouldLock)
    {
        // Find all AgapThirdPersonController instances in the scene
        AgapThirdPersonController[] playerControllers = FindObjectsOfType<AgapThirdPersonController>();

        foreach (var controller in playerControllers)
        {
            controller.LockCameraPosition = shouldLock;

            if (showDebugLogs)
                Debug.Log($"[PauseMenu] Camera lock set to {shouldLock} for player controller: {controller.gameObject.name}");
        }

        // FIXED: Also lock BoatController cameras
        BoatController[] boatControllers = FindObjectsOfType<BoatController>();
        foreach (var boat in boatControllers)
        {
            boat.LockCameraPosition = shouldLock;

            if (showDebugLogs)
                Debug.Log($"[PauseMenu] Camera lock set to {shouldLock} for boat controller: {boat.gameObject.name}");
        }
    }

    /// <summary>
    /// Called when Pause button is clicked
    /// </summary>
    private void OnPauseButtonClicked()
    {
        if (showDebugLogs)
            Debug.Log("[PauseMenu] Pause button clicked");

        PauseGame();
    }

    /// <summary>
    /// Called when Resume button is clicked
    /// </summary>
    private void OnResumeClicked()
    {
        if (showDebugLogs)
            Debug.Log("[PauseMenu] Resume button clicked");

        ResumeGame();
    }

    /// <summary>
    /// Called when Main Menu button is clicked
    /// Handles multiplayer cleanup and returns ALL PLAYERS to main menu
    /// </summary>
    private void OnMainMenuClicked()
    {
        // Only instructor can trigger return to main menu
        if (restrictToInstructorOnly && !isInstructor)
        {
            if (showDebugLogs)
                Debug.LogWarning("[PauseMenu] Main Menu denied - Only instructors can return to main menu");
            return;
        }

        if (showDebugLogs)
            Debug.Log("[PauseMenu] Main Menu button clicked - Requesting all players return to main menu");

        // Request server to signal all clients to return to main menu
        RequestReturnToMainMenuServerRpc();
    }

    /// <summary>
    /// ServerRpc to request all clients return to main menu
    /// FIXED: Now triggers ClientRpc to ensure all clients receive the signal
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void RequestReturnToMainMenuServerRpc()
    {
        if (showDebugLogs)
            Debug.Log("[PauseMenu] Server received request to return all players to main menu. Notifying all clients...");

        // Immediately call ClientRpc to notify ALL clients (including host)
        ReturnToMainMenuClientRpc();
    }

    /// <summary>
    /// ClientRpc called on ALL clients to return to main menu
    /// This ensures instructor and trainees all return together
    /// </summary>
    [ClientRpc]
    private void ReturnToMainMenuClientRpc()
    {
        if (isReturningToMainMenu)
            return;

        isReturningToMainMenu = true;

        if (showDebugLogs)
            Debug.Log("[PauseMenu] ClientRpc received - Returning to main menu...");

        // Resume time before transitioning
        Time.timeScale = originalTimeScale;

        // Re-enable camera rotation before leaving
        if (disableCameraRotationWhenPaused)
        {
            LockAllPlayerCameras(false);
        }

        // Use SceneLoader to handle multiplayer cleanup and return to main menu
        SceneLoader sceneLoader = FindObjectOfType<SceneLoader>();
        
        if (sceneLoader != null)
        {
            sceneLoader.ResetAndReturnToMainMenu();
        }
        else
        {
            // Fallback: Create temporary SceneLoader if one doesn't exist
            if (showDebugLogs)
                Debug.LogWarning("[PauseMenu] SceneLoader not found. Creating temporary instance.");

            GameObject tempLoader = new GameObject("TempSceneLoader");
            SceneLoader tempSceneLoader = tempLoader.AddComponent<SceneLoader>();
            tempSceneLoader.ResetAndReturnToMainMenu();
        }
    }

    /// <summary>
    /// Public method to check if game is currently paused
    /// </summary>
    public bool IsPaused()
    {
        return isPausedNetworked.Value;
    }

    /// <summary>
    /// Check if the current user is an instructor
    /// </summary>
    public bool IsInstructor()
    {
        return isInstructor;
    }

    /// <summary>
    /// Force resume game (useful for external scripts)
    /// </summary>
    public void ForceResume()
    {
        if (isPausedNetworked.Value)
            ResumeGame();
    }

    /// <summary>
    /// Force pause game (useful for external scripts)
    /// Only works for instructors when restrictToInstructorOnly is enabled
    /// </summary>
    public void ForcePause()
    {
        if (!isPausedNetworked.Value)
            PauseGame();
    }

    public override void OnNetworkDespawn()
    {
        // Unsubscribe from network variable changes
        isPausedNetworked.OnValueChanged -= OnPauseStateChanged;

        base.OnNetworkDespawn();
    }

    private void OnDestroy()
    {
        // Cleanup button listeners
        if (resumeButton != null)
            resumeButton.onClick.RemoveListener(OnResumeClicked);

        if (mainMenuButton != null)
            mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);

        if (pauseButton != null)
            pauseButton.onClick.RemoveListener(OnPauseButtonClicked);

        // Ensure time scale is restored when this object is destroyed
        Time.timeScale = originalTimeScale;

        // Ensure cameras are unlocked
        if (disableCameraRotationWhenPaused)
        {
            LockAllPlayerCameras(false);
        }
    }
}
