using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LobbyManager : MonoBehaviour
{
    [Header("Number of Tasks")]
    [SerializeField] private Button minusButton;
    [SerializeField] private Button addButton;
    [SerializeField] private TMP_Text taskNumberText;
    private int taskCount = 5;
    private const int MIN_TASKS = 5;
    private const int MAX_TASKS = 8;

    [Header("Main Disaster")]
    [SerializeField] private TMP_Dropdown disasterDropdown;
    [SerializeField] private Image disasterImage;
    [SerializeField] private Sprite floodSprite;
    [SerializeField] private Sprite earthquakeSprite;
    [SerializeField] private Sprite TestSprite;

    [Header("Duration")]
    [SerializeField] private TMP_Dropdown durationDropdown;
    private readonly int[] durations = { 300, 480, 600 }; // 5, 8, 10 minutes in seconds

    [Header("Start Button")]
    [SerializeField] private Button startButton;

    private int selectedDisasterIndex = 1; // Default to Earthquake
    private int selectedDuration = 300;

    void Start()
    {
        // Ensure GameConfig exists
        EnsureGameConfigExists();

        // Initialize task count
        taskCount = 5;
        UpdateTaskCountDisplay();

        // Setup minus button
        if (minusButton != null)
        {
            minusButton.onClick.AddListener(DecreaseTaskCount);
        }

        // Setup add button
        if (addButton != null)
        {
            addButton.onClick.AddListener(IncreaseTaskCount);
        }

        // Setup disaster dropdown
        if (disasterDropdown != null)
        {
            disasterDropdown.onValueChanged.AddListener(OnDisasterChanged);
            OnDisasterChanged(disasterDropdown.value); // Initialize
        }

        // Setup duration dropdown
        if (durationDropdown != null)
        {
            durationDropdown.onValueChanged.AddListener(OnDurationChanged);
            OnDurationChanged(durationDropdown.value); // Initialize
        }

        // Setup start button
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
        }
    }

    private void EnsureGameConfigExists()
    {
        if (GameConfig.Instance == null)
        {
            GameObject configObject = new GameObject("GameConfig");
            configObject.AddComponent<GameConfig>();
            Debug.Log("[LobbyManager] GameConfig instance created");
        }
    }

    private void DecreaseTaskCount()
    {
        if (taskCount > MIN_TASKS)
        {
            taskCount--;
            UpdateTaskCountDisplay();
        }
    }

    private void IncreaseTaskCount()
    {
        if (taskCount < MAX_TASKS)
        {
            taskCount++;
            UpdateTaskCountDisplay();
        }
    }

    private void UpdateTaskCountDisplay()
    {
        if (taskNumberText != null)
        {
            taskNumberText.text = taskCount.ToString();
        }
    }

    private void OnDisasterChanged(int index)
    {
        selectedDisasterIndex = index;

        // Update UI sprite based on selection
        switch (index)
        {
            case 0: // Flood
                if (disasterImage != null)
                {
                    disasterImage.sprite = floodSprite;
                }
                Debug.Log("[LobbyManager] Flood mode selected (will load TestKen scene with Flood mode)");
                break;
            case 1: // Earthquake
                if (disasterImage != null)
                {
                    disasterImage.sprite = earthquakeSprite;
                }
                Debug.Log("[LobbyManager] Earthquake mode selected (will load TestKen scene with Earthquake mode)");
                break;
            case 2: // TestKen (both modes)
                if (disasterImage != null)
                {
                    disasterImage.sprite = TestSprite;
                }
                Debug.Log("[LobbyManager] TestKen mode selected");
                break;
            default:
                selectedDisasterIndex = 1;
                Debug.LogWarning("[LobbyManager] Unknown disaster index, defaulting to Earthquake");
                break;
        }
    }

    private void OnDurationChanged(int index)
    {
        if (index >= 0 && index < durations.Length)
        {
            selectedDuration = durations[index];
        }
    }

    private void OnStartButtonClicked()
    {
        // Reset points before starting new game
        if (PointManager.Instance != null)
        {
            PointManager.Instance.ResetPoints();
            Debug.Log("[LobbyManager] Points reset for new simulation");
        }

        // Store settings in GameConfig (persists across scenes via DontDestroyOnLoad)
        if (GameConfig.Instance != null)
        {
            GameConfig.Instance.TaskCount = taskCount;
            GameConfig.Instance.DisasterModeIndex = selectedDisasterIndex;
            GameConfig.Instance.GameDurationInSeconds = selectedDuration;

            Debug.Log($"[LobbyManager] Game configured - Tasks: {taskCount}, Mode: {GameConfig.Instance.GetDisasterModeName()}, Duration: {selectedDuration}s");
        }
        else
        {
            Debug.LogError("[LobbyManager] GameConfig instance not found!");
        }

        // Always load TestKen scene
        Debug.Log("[LobbyManager] Loading TestKen scene");
        SceneManager.LoadScene("TestKen");
    }

    // Public method to get current task count (can be called from other scripts)
    public int GetTaskCount()
    {
        return taskCount;
    }
}
