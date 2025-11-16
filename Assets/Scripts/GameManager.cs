using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Singleton for easy access
    public static GameManager Instance { get; private set; }

    #region Disaster Management
    [Header("Disaster GameObjects")]
    [Tooltip("The Flood Game GameObject in the scene")]
    [SerializeField] private GameObject floodGameObject;

    [Tooltip("The Earthquake Game GameObject in the scene")]
    [SerializeField] private GameObject earthquakeGameObject;
    #endregion

    #region Victim Tracking
    [Header("Victim Tracking")]
    [Tooltip("Reference to the VictimSpawner to track total victims")]
    [SerializeField] private VictimSpawner victimSpawner;

    private int totalVictims = 0;
    private int savedVictims = 0;

    // Public properties for victim tracking
    public int TotalVictims => totalVictims;
    public int SavedVictims => savedVictims;
    #endregion

    #region Medkit Management
    [Header("Medkit Settings")]
    [SerializeField] private int maxMedkits = 2;
    [SerializeField] private int currentMedkits = 2;

    [Header("Safe Zone")]
    [Tooltip("Safe zone GameObject that replenishes medkits when player enters")]
    [SerializeField] private GameObject safeZone;

    [Header("Medkit UI References")]
    [Tooltip("Reference to the TextMeshProUGUI component that displays medkit count")]
    [SerializeField] private TextMeshProUGUI medkitCountText;

    [Header("Blink Settings")]
    [SerializeField] private float blinkDuration = 1f;
    [SerializeField] private float blinkInterval = 0.2f;
    [SerializeField] private Color blinkColor = Color.red;

    [Header("Replenish Effect")]
    [SerializeField] private Color replenishColor = Color.green;
    [SerializeField] private float replenishBlinkDuration = 0.5f;

    // Public properties
    public int CurrentMedkits => currentMedkits;
    public int MaxMedkits => maxMedkits;

    private Color defaultColor;
    private SafeZoneTrigger safeZoneTrigger;
    #endregion

    #region Duration Management
    [Header("Timer UI Text References")]
    [SerializeField] private TMP_Text trainee1DurationText;
    [SerializeField] private TMP_Text trainee2DurationText;
    [SerializeField] private TMP_Text instructorDurationText;

    [Header("Timer Settings")]
    [SerializeField] private bool startOnAwake = true;

    private int totalDurationInSeconds;
    private int remainingTimeInSeconds;
    private bool isTimerRunning = false;
    #endregion

    void Awake()
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
    }

    void Start()
    {
        // Initialize Disaster Scene Management
        InitializeDisasterScene();

        // Initialize Victim Tracking
        InitializeVictimTracking();

        // Initialize Medkit Management
        InitializeMedkitSystem();

        // Initialize Duration Management
        InitializeDurationSystem();
    }

    #region Disaster Scene Management
    private void InitializeDisasterScene()
    {
        // Get the selected disaster from PlayerPrefs
        string selectedDisaster = PlayerPrefs.GetString("DisasterType", "Flood");

        // Enable/Disable GameObjects based on selection
        switch (selectedDisaster)
        {
            case "Flood":
                EnableFloodMode();
                break;
            case "Earthquake":
                EnableEarthquakeMode();
                break;
            case "TestKen":
                // For TestKen, you can choose to enable both or handle differently
                // Currently enabling both for testing purposes
                EnableBothModes();
                break;
            default:
                Debug.LogWarning($"Unknown disaster type: {selectedDisaster}. Defaulting to Flood.");
                EnableFloodMode();
                break;
        }
    }

    private void EnableFloodMode()
    {
        if (floodGameObject != null)
        {
            floodGameObject.SetActive(true);
            Debug.Log("Flood Game enabled");
        }
        else
        {
            Debug.LogError("Flood Game GameObject is not assigned!");
        }

        if (earthquakeGameObject != null)
        {
            earthquakeGameObject.SetActive(false);
            Debug.Log("Earthquake Game disabled");
        }
    }

    private void EnableEarthquakeMode()
    {
        if (earthquakeGameObject != null)
        {
            earthquakeGameObject.SetActive(true);
            Debug.Log("Earthquake Game enabled");
        }
        else
        {
            Debug.LogError("Earthquake Game GameObject is not assigned!");
        }

        if (floodGameObject != null)
        {
            floodGameObject.SetActive(false);
            Debug.Log("Flood Game disabled");
        }
    }

    private void EnableBothModes()
    {
        // For TestKen mode, enable both for testing
        if (floodGameObject != null)
        {
            floodGameObject.SetActive(true);
        }

        if (earthquakeGameObject != null)
        {
            earthquakeGameObject.SetActive(true);
        }

        Debug.Log("TestKen mode: Both disaster GameObjects enabled");
    }
    #endregion

    #region Victim Tracking
    private void InitializeVictimTracking()
    {
        // Auto-find VictimSpawner if not assigned
        if (victimSpawner == null)
        {
            victimSpawner = Object.FindAnyObjectByType<VictimSpawner>();
        }

        if (victimSpawner != null)
        {
            // Get total victims from PlayerPrefs (set by LobbyManager)
            totalVictims = PlayerPrefs.GetInt("TaskCount", 0);
            Debug.Log($"[GameManager] Total victims to rescue: {totalVictims}");
        }
        else
        {
            Debug.LogWarning("[GameManager] VictimSpawner not found! Victim tracking will not work properly.");
        }
    }

    /// <summary>
    /// Increments the saved victims count
    /// </summary>
    public void IncrementSavedVictims()
    {
        savedVictims++;
        Debug.Log($"[GameManager] Saved victims: {savedVictims}/{totalVictims}");

        // Check if all victims are saved
        if (AreAllVictimsSaved())
        {
            Debug.Log("[GameManager] All victims saved!");
        }
    }

    /// <summary>
    /// Checks if all victims have been saved
    /// </summary>
    public bool AreAllVictimsSaved()
    {
        return savedVictims >= totalVictims && totalVictims > 0;
    }

    /// <summary>
    /// Gets the count of rescued victims
    /// </summary>
    public int GetRescuedVictimCount()
    {
        return savedVictims;
    }
    #endregion

    #region Medkit Management
    private void InitializeMedkitSystem()
    {
        if (medkitCountText != null)
        {
            defaultColor = medkitCountText.color;
        }
        UpdateMedkitUI();

        // Setup safe zone trigger
        SetupSafeZone();
    }

    private void SetupSafeZone()
    {
        if (safeZone != null)
        {
            // Check if SafeZoneTrigger component exists, if not add it
            safeZoneTrigger = safeZone.GetComponent<SafeZoneTrigger>();
            if (safeZoneTrigger == null)
            {
                safeZoneTrigger = safeZone.AddComponent<SafeZoneTrigger>();
            }

            // Subscribe to the safe zone entry event
            safeZoneTrigger.OnPlayerEnterSafeZone += OnPlayerEnterSafeZone;

            Debug.Log("[GameManager] Safe zone configured successfully");
        }
        else
        {
            Debug.LogWarning("[GameManager] No safe zone GameObject assigned!");
        }
    }

    /// <summary>
    /// Called when player enters the safe zone
    /// </summary>
    private void OnPlayerEnterSafeZone()
    {
        // Check if all victims are saved
        if (AreAllVictimsSaved())
        {
            Debug.Log("[GameManager] All victims saved! Loading result scene...");
            LoadResultScene();
        }
        else
        {
            // Not all victims saved, just replenish medkits
            ReplenishMedkits();
        }
    }

    public bool UseMedkit()
    {
        if (currentMedkits <= 0)
            return false;

        currentMedkits--;
        UpdateMedkitUI();
        return true;
    }

    /// <summary>
    /// Replenishes medkits to maximum capacity
    /// </summary>
    public void ReplenishMedkits()
    {
        int medkitsToAdd = maxMedkits - currentMedkits;

        if (medkitsToAdd > 0)
        {
            currentMedkits = maxMedkits;
            UpdateMedkitUI();
            TriggerReplenishEffect();
            Debug.Log($"[GameManager] Replenished {medkitsToAdd} medkit(s). Current: {currentMedkits}/{maxMedkits}");
        }
        else
        {
            Debug.Log("[GameManager] Medkits already at maximum capacity");
        }
    }

    private void UpdateMedkitUI()
    {
        if (medkitCountText != null)
        {
            medkitCountText.text = "Medkit: " + currentMedkits + "/" + maxMedkits;
        }
    }

    /// <summary>
    /// Triggers a blink effect on the medkit count text
    /// </summary>
    public void TriggerBlinkEffect()
    {
        StartCoroutine(BlinkText());
    }

    /// <summary>
    /// Triggers a green blink effect when medkits are replenished
    /// </summary>
    private void TriggerReplenishEffect()
    {
        StartCoroutine(ReplenishBlinkText());
    }

    private IEnumerator BlinkText()
    {
        if (medkitCountText == null)
            yield break;

        float elapsedTime = 0f;

        while (elapsedTime < blinkDuration)
        {
            // Red
            medkitCountText.color = blinkColor;
            yield return new WaitForSeconds(blinkInterval);

            // Default
            medkitCountText.color = defaultColor;
            yield return new WaitForSeconds(blinkInterval);

            elapsedTime += blinkInterval * 2;
        }

        // Ensure it ends on default color
        medkitCountText.color = defaultColor;
    }

    private IEnumerator ReplenishBlinkText()
    {
        if (medkitCountText == null)
            yield break;

        float elapsedTime = 0f;

        while (elapsedTime < replenishBlinkDuration)
        {
            // Green
            medkitCountText.color = replenishColor;
            yield return new WaitForSeconds(blinkInterval);

            // Default
            medkitCountText.color = defaultColor;
            yield return new WaitForSeconds(blinkInterval);

            elapsedTime += blinkInterval * 2;
        }

        // Ensure it ends on default color
        medkitCountText.color = defaultColor;
    }
    #endregion

    #region Duration Management
    private void InitializeDurationSystem()
    {
        // Get the duration from PlayerPrefs (set by LobbyManager)
        totalDurationInSeconds = PlayerPrefs.GetInt("GameDuration", 300); // Default to 5 minutes if not set
        remainingTimeInSeconds = totalDurationInSeconds;

        Debug.Log($"[GameManager] Loaded duration: {totalDurationInSeconds} seconds ({totalDurationInSeconds / 60} minutes)");

        // Update all UI texts with initial time
        UpdateAllTimerDisplays();

        // Start the timer if enabled
        if (startOnAwake)
        {
            StartTimer();
        }
    }

    /// <summary>
    /// Starts the countdown timer
    /// </summary>
    public void StartTimer()
    {
        if (!isTimerRunning)
        {
            isTimerRunning = true;
            StartCoroutine(CountdownCoroutine());
            Debug.Log("[GameManager] Timer started");
        }
    }

    /// <summary>
    /// Pauses the countdown timer
    /// </summary>
    public void PauseTimer()
    {
        isTimerRunning = false;
        Debug.Log("[GameManager] Timer paused");
    }

    /// <summary>
    /// Resumes the countdown timer
    /// </summary>
    public void ResumeTimer()
    {
        if (!isTimerRunning && remainingTimeInSeconds > 0)
        {
            isTimerRunning = true;
            StartCoroutine(CountdownCoroutine());
            Debug.Log("[GameManager] Timer resumed");
        }
    }

    /// <summary>
    /// Resets the timer to the initial duration
    /// </summary>
    public void ResetTimer()
    {
        remainingTimeInSeconds = totalDurationInSeconds;
        UpdateAllTimerDisplays();
        Debug.Log("[GameManager] Timer reset");
    }

    /// <summary>
    /// Coroutine that handles the countdown
    /// </summary>
    private IEnumerator CountdownCoroutine()
    {
        while (isTimerRunning && remainingTimeInSeconds > 0)
        {
            yield return new WaitForSeconds(1f);
            remainingTimeInSeconds--;
            UpdateAllTimerDisplays();
        }

        if (remainingTimeInSeconds <= 0)
        {
            OnTimerComplete();
        }
    }

    /// <summary>
    /// Updates all timer UI displays
    /// </summary>
    private void UpdateAllTimerDisplays()
    {
        string formattedTime = FormatTime(remainingTimeInSeconds);

        if (trainee1DurationText != null)
        {
            trainee1DurationText.text = formattedTime;
        }

        if (trainee2DurationText != null)
        {
            trainee2DurationText.text = formattedTime;
        }

        if (instructorDurationText != null)
        {
            instructorDurationText.text = formattedTime;
        }
    }

    /// <summary>
    /// Formats time in seconds to MM:SS format
    /// </summary>
    private string FormatTime(int timeInSeconds)
    {
        int minutes = timeInSeconds / 60;
        int seconds = timeInSeconds % 60;
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    /// <summary>
    /// Called when the timer reaches zero
    /// </summary>
    private void OnTimerComplete()
    {
        isTimerRunning = false;
        Debug.Log("[GameManager] Timer completed!");
        
        UpdateAllTimerDisplays(); // Ensure it shows 00:00
        
        // Load result scene when time is up
        LoadResultScene();
    }

    /// <summary>
    /// Loads the result scene
    /// </summary>
    private void LoadResultScene()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        Debug.Log("[GameManager] Loading Result scene...");
        SceneManager.LoadScene("Result");
    }

    /// <summary>
    /// Public getter for remaining time
    /// </summary>
    public int GetRemainingTime()
    {
        return remainingTimeInSeconds;
    }

    /// <summary>
    /// Public getter for total duration
    /// </summary>
    public int GetTotalDuration()
    {
        return totalDurationInSeconds;
    }

    /// <summary>
    /// Public getter for timer running state
    /// </summary>
    public bool IsTimerRunning()
    {
        return isTimerRunning;
    }

    /// <summary>
    /// Gets the remaining time as a formatted string
    /// </summary>
    public string GetFormattedRemainingTime()
    {
        return FormatTime(remainingTimeInSeconds);
    }

    /// <summary>
    /// Gets the progress as a percentage (0 to 1)
    /// </summary>
    public float GetTimerProgress()
    {
        if (totalDurationInSeconds <= 0) return 0f;
        return 1f - ((float)remainingTimeInSeconds / (float)totalDurationInSeconds);
    }
    #endregion

    private void OnDestroy()
    {
        // Stop the timer when the object is destroyed
        isTimerRunning = false;

        // Unsubscribe from safe zone events to prevent memory leaks
        if (safeZoneTrigger != null)
        {
            safeZoneTrigger.OnPlayerEnterSafeZone -= OnPlayerEnterSafeZone;
        }
    }
}

/// <summary>
/// Helper component for safe zone trigger detection
/// </summary>
public class SafeZoneTrigger : MonoBehaviour
{
    public event System.Action OnPlayerEnterSafeZone;

    [Header("Safe Zone Settings")]
    [Tooltip("Tag used to identify the player")]
    [SerializeField] private string playerTag = "Player";

    [Tooltip("Cooldown time in seconds before player can trigger again")]
    [SerializeField] private float triggerCooldown = 2f;

    private float lastTriggerTime = -999f;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the colliding object is the player
        if (other.CompareTag(playerTag))
        {
            // Check cooldown to prevent spam
            if (Time.time - lastTriggerTime >= triggerCooldown)
            {
                lastTriggerTime = Time.time;
                OnPlayerEnterSafeZone?.Invoke();
                Debug.Log("[SafeZoneTrigger] Player entered safe zone");
            }
        }
    }

    private void OnValidate()
    {
        // Ensure this GameObject has a trigger collider
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning("[SafeZoneTrigger] Collider on safe zone should be set as Trigger!");
        }
    }
}
