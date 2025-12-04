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

    [Header("Username Input Fields")]
    [SerializeField] private TMP_InputField InstructorInputField;
    [SerializeField] private TMP_InputField trainee1InputField;
    [SerializeField] private TMP_InputField trainee2InputField;

    private int selectedDisasterIndex = 1; // Default to Earthquake
    private int selectedDuration = 300;

    // Track trainees that have joined
    private int traineeCount = 0;

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

        // Populate username based on logged-in role
        PopulateUsernameFields();
    }

    private void PopulateUsernameFields()
    {
        // Get the user type from PlayerPrefs
        string userType = PlayerPrefs.GetString("Type_Of_User", "");
        
        // Get username from AuthManager if available
        string username = "";
        if (AuthManager.Instance != null)
        {
            username = AuthManager.Instance.Current_Username;
        }

        // If username is empty, try to get from PlayerPrefs as fallback
        if (string.IsNullOrEmpty(username))
        {
            username = PlayerPrefs.GetString("Current_Username", "");
        }

        Debug.Log($"[LobbyManager] User Type: {userType}, Username: {username}");

        // Populate the appropriate input field based on user type
        switch (userType)
        {
            case "instructor":
                if (InstructorInputField != null)
                {
                    InstructorInputField.text = username;
                    InstructorInputField.interactable = false; // Make it read-only
                    Debug.Log($"[LobbyManager] Populated Instructor field with: {username}");
                }
                break;

            case "trainee":
                // Assign to next available trainee slot
                if (trainee1InputField != null && string.IsNullOrEmpty(trainee1InputField.text))
                {
                    trainee1InputField.text = username;
                    trainee1InputField.interactable = false;
                    traineeCount = 1;
                    Debug.Log($"[LobbyManager] Populated Trainee1 field with: {username}");
                }
                else if (trainee2InputField != null && string.IsNullOrEmpty(trainee2InputField.text))
                {
                    trainee2InputField.text = username;
                    trainee2InputField.interactable = false;
                    traineeCount = 2;
                    Debug.Log($"[LobbyManager] Populated Trainee2 field with: {username}");
                }
                else
                {
                    Debug.LogWarning("[LobbyManager] All trainee slots are full!");
                }
                break;

            default:
                Debug.LogWarning($"[LobbyManager] Unknown user type: {userType}");
                break;
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
