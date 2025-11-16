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
    private int taskCount = 1;
    private const int MIN_TASKS = 0;
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

    [Header("Networking - Instructor")]
    [SerializeField] private Button generateLobbyCodeButton;
    [SerializeField] private TMP_Text lobbyCodeDisplayText;
    [SerializeField] private Button startHostButton;

    [Header("Networking - Trainee")]
    [SerializeField] private TMP_InputField lobbyCodeInputField;
    [SerializeField] private Button joinLobbyButton;

    [Header("Networking - Status")]
    [SerializeField] private TMP_Text connectionStatusText;
    [SerializeField] private TMP_Text playerCountText;
    [SerializeField] private GameObject instructorPanel;
    [SerializeField] private GameObject traineePanel;

    public static int SelectedTaskCount { get; private set; } = 1;
    public static string SelectedDisaster { get; private set; } = "TestKen";
    public static int SelectedDuration { get; private set; } = 300;

    private bool isInstructor = false;

    void Start()
    {
        // Determine user role
        DetermineUserRole();
        SetupUIForRole();

        // Initialize task count
        taskCount = 1;
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

        // Setup networking buttons
        SetupNetworkingButtons();

        // Subscribe to NetworkLobbyManager events
        if (NetworkLobbyManager.Instance != null)
        {
            NetworkLobbyManager.Instance.OnLobbyCodeGenerated += OnLobbyCodeGenerated;
            NetworkLobbyManager.Instance.OnLobbyJoinAttempt += OnLobbyJoinAttempt;
            NetworkLobbyManager.Instance.OnPlayersCountChanged += OnPlayersCountChanged;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (NetworkLobbyManager.Instance != null)
        {
            NetworkLobbyManager.Instance.OnLobbyCodeGenerated -= OnLobbyCodeGenerated;
            NetworkLobbyManager.Instance.OnLobbyJoinAttempt -= OnLobbyJoinAttempt;
            NetworkLobbyManager.Instance.OnPlayersCountChanged -= OnPlayersCountChanged;
        }
    }

    private void DetermineUserRole()
    {
        // Check from RoomManager first
        if (RoomManager.Instance != null)
        {
            isInstructor = RoomManager.Instance.isInstructor;
        }
        else
        {
            // Fallback to PlayerPrefs
            string userType = PlayerPrefs.GetString("Type_Of_User", "trainee");
            isInstructor = userType == "instructor";
        }

        Debug.Log($"User role determined: {(isInstructor ? "Instructor" : "Trainee")}");
    }

    private void SetupUIForRole()
    {
        // Show/hide panels based on role
        if (instructorPanel != null)
        {
            instructorPanel.SetActive(isInstructor);
        }

        if (traineePanel != null)
        {
            traineePanel.SetActive(!isInstructor);
        }

        // Update connection status
        UpdateConnectionStatus(isInstructor ? "Instructor Mode" : "Trainee Mode");
    }

    private void SetupNetworkingButtons()
    {
        // Instructor buttons
        if (generateLobbyCodeButton != null && isInstructor)
        {
            generateLobbyCodeButton.onClick.AddListener(OnGenerateLobbyCodeClicked);
        }

        if (startHostButton != null && isInstructor)
        {
            startHostButton.onClick.AddListener(OnStartHostClicked);
            startHostButton.interactable = false; // Disabled until code is generated
        }

        // Trainee buttons
        if (joinLobbyButton != null && !isInstructor)
        {
            joinLobbyButton.onClick.AddListener(OnJoinLobbyClicked);
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
        SelectedTaskCount = taskCount;
    }

    private void OnDisasterChanged(int index)
    {
        switch (index)
        {
            case 0: // Flood
                if (disasterImage != null)
                {
                    disasterImage.sprite = floodSprite;
                }
                SelectedDisaster = "Flood";
                break;
            case 1: // Earthquake
                if (disasterImage != null)
                {
                    disasterImage.sprite = earthquakeSprite;
                }
                SelectedDisaster = "Earthquake";
                break;
            case 2: // testing ground ni kenneth
                if (disasterImage != null)
                {
                    disasterImage.sprite = TestSprite;
                }
                SelectedDisaster = "TestKen";
                break;
            default:
                SelectedDisaster = "TestKen";
                break;
        }
    }

    private void OnDurationChanged(int index)
    {
        if (index >= 0 && index < durations.Length)
        {
            SelectedDuration = durations[index];
        }
    }

    private void OnStartButtonClicked()
    {
        // Store the selected disaster and duration for other scenes to access
        PlayerPrefs.SetInt("TaskCount", SelectedTaskCount);
        PlayerPrefs.SetString("DisasterType", SelectedDisaster);
        PlayerPrefs.SetInt("GameDuration", SelectedDuration);
        PlayerPrefs.Save();

        //// Load the appropriate scene based on the selected disaster
        //if (!string.IsNullOrEmpty(SelectedDisaster))
        //{
        //    SceneManager.LoadScene(SelectedDisaster);
        //}
        //else
        //{
        //    Debug.LogError("No disaster selected!");
        //}
        // Always load TestKen scene
        SceneManager.LoadScene("TestKen");
    }

    // Public method to get current task count (can be called from other scripts)
    public int GetTaskCount()
    {
        return taskCount;
    }

    // ========== Networking Methods ==========

    private void OnGenerateLobbyCodeClicked()
    {
        if (NetworkLobbyManager.Instance != null)
        {
            string code = NetworkLobbyManager.Instance.GenerateLobbyCode();
            if (!string.IsNullOrEmpty(code))
            {
                UpdateConnectionStatus($"Lobby code generated: {code}");
            }
        }
        else
        {
            Debug.LogError("NetworkLobbyManager instance not found!");
        }
    }

    private void OnStartHostClicked()
    {
        if (NetworkLobbyManager.Instance != null)
        {
            bool success = NetworkLobbyManager.Instance.StartHostWithLobbyCode();
            if (success)
            {
                UpdateConnectionStatus("Hosting lobby...");
            }
            else
            {
                UpdateConnectionStatus("Failed to start hosting");
            }
        }
    }

    private void OnJoinLobbyClicked()
    {
        if (lobbyCodeInputField != null && NetworkLobbyManager.Instance != null)
        {
            string code = lobbyCodeInputField.text.Trim().ToUpper();
            NetworkLobbyManager.Instance.JoinLobbyWithCode(code);
        }
    }

    // Event callbacks from NetworkLobbyManager
    private void OnLobbyCodeGenerated(string code)
    {
        if (lobbyCodeDisplayText != null)
        {
            lobbyCodeDisplayText.text = $"Lobby Code: {code}";
        }

        // Enable the start host button
        if (startHostButton != null)
        {
            startHostButton.interactable = true;
        }
    }

    private void OnLobbyJoinAttempt(bool success, string message)
    {
        UpdateConnectionStatus(message);
    }

    private void OnPlayersCountChanged(int count)
    {
        if (playerCountText != null)
        {
            playerCountText.text = $"Players: {count}";
        }
    }

    private void UpdateConnectionStatus(string status)
    {
        if (connectionStatusText != null)
        {
            connectionStatusText.text = status;
        }
        Debug.Log($"Connection Status: {status}");
    }
}
