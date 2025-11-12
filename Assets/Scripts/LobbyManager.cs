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

    public static int SelectedTaskCount { get; private set; } = 1;
    public static string SelectedDisaster { get; private set; } = "Flood";
    public static int SelectedDuration { get; private set; } = 300;

    void Start()
    {
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
                SelectedDisaster = "Flood";
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

        // Load the appropriate scene based on the selected disaster
        if (!string.IsNullOrEmpty(SelectedDisaster))
        {
            SceneManager.LoadScene(SelectedDisaster);
        }
        else
        {
            Debug.LogError("No disaster selected!");
        }
    }

    // Public method to get current task count (can be called from other scripts)
    public int GetTaskCount()
    {
        return taskCount;
    }
}
