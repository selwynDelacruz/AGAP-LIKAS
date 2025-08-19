using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class LobbyManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject instructorPanel; // "Instructor Disaster Setup"
    public GameObject traineePanel;    // "Trainee Lobby"

    [Header("Instructor Disaster Setup Fields")]
    public TMP_Text instructorNameText;
    public TMP_Text traineeNameText;
    public TMP_Text roomCodeText;

    [Header("Trainee Lobby Fields")]
    public TMP_Text traineePanelInstructorNameText;
    public TMP_Text traineePanelTraineeNameText;
    public TMP_Text traineePanelRoomCodeText;

    [Header("Start Button")]
    public Button startButton; // Only visible to Instructor

    [Header("Disaster Selection")]
    [SerializeField] private TMP_Text modeText;
    [SerializeField] private Image disasterImage;
    [SerializeField] private Sprite floodSprite, earthquakeSprite;
    [SerializeField] private TMP_Dropdown disasterDropdown;

    [Header("Duration Selection")]
    [SerializeField] private TMP_Dropdown durationDropdown;

    private readonly int[] durations = { 60, 180, 300 }; // 1, 3, 5 minutes

    public static string SelectedDisaster { get; private set; } // Stores the selected value

    // Local state for instructor/trainee simulation
    private bool isInstructor = true;
    private string instructorName = "Instructor";
    private string traineeName = "Trainee";
    private string roomCode = "LOCAL123";

    void Start()
    {
        // Panel setup (simulate instructor/trainee locally)
        if (isInstructor)
        {
            instructorPanel.SetActive(true);
            traineePanel.SetActive(false);
            startButton.gameObject.SetActive(true);
        }
        else
        {
            instructorPanel.SetActive(false);
            traineePanel.SetActive(true);
            startButton.gameObject.SetActive(false);
            UpdateTraineePanelFields();
        }

        UpdatePlayerNames();

        // Set the room code text for the main UI (if needed)
        if (roomCodeText != null)
        {
            roomCodeText.text = "Lobby Code: " + roomCode;
        }

        // Disaster dropdown setup
        if (disasterDropdown != null && disasterDropdown.options.Count == 0)
        {
            List<string> options = new List<string> { "Flood", "Earthquake" };
            disasterDropdown.ClearOptions();
            disasterDropdown.AddOptions(options);
        }
        if (disasterDropdown != null)
        {
            UpdateDisaster(disasterDropdown.value);
            disasterDropdown.onValueChanged.AddListener(UpdateDisaster);
        }

        // Duration dropdown setup
        if (durationDropdown != null)
        {
            durationDropdown.onValueChanged.AddListener(OnDurationChanged);
        }

        // Set initial duration
        DurationManager.DurationSeconds = durations[0];
        SetDropdownToDuration(durations[0]);
    }

    void UpdateTraineePanelFields()
    {
        if (traineePanelInstructorNameText != null)
            traineePanelInstructorNameText.text = instructorName;

        if (traineePanelTraineeNameText != null)
            traineePanelTraineeNameText.text = traineeName;

        if (traineePanelRoomCodeText != null)
            traineePanelRoomCodeText.text = "Lobby Code: " + roomCode;
    }

    void UpdatePlayerNames()
    {
        if (instructorNameText != null)
            instructorNameText.text = instructorName;
        if (traineeNameText != null)
            traineeNameText.text = traineeName;
    }

    // Disaster selection logic
    public void UpdateDisaster(int index)
    {
        switch (index)
        {
            case 0:
                if (modeText != null) modeText.text = "Flood";
                if (disasterImage != null) disasterImage.sprite = floodSprite;
                SelectedDisaster = "Flood";
                break;
            case 1:
                if (modeText != null) modeText.text = "Earthquake";
                if (disasterImage != null) disasterImage.sprite = earthquakeSprite;
                SelectedDisaster = "Earthquake";
                break;
            default:
                if (modeText != null) modeText.text = "Unknown";
                if (disasterImage != null) disasterImage.sprite = null;
                SelectedDisaster = "";
                break;
        }
    }

    // Duration selection logic
    void OnDurationChanged(int index)
    {
        if (index >= 0 && index < durations.Length)
        {
            int selectedDuration = durations[index];
            DurationManager.DurationSeconds = selectedDuration;
            SetDropdownToDuration(selectedDuration);
        }
    }

    private void SetDropdownToDuration(int duration)
    {
        if (durationDropdown == null) return;
        for (int i = 0; i < durations.Length; i++)
        {
            if (durations[i] == duration)
            {
                durationDropdown.SetValueWithoutNotify(i);
                break;
            }
        }
    }

    // Called by Instructor's Start Button
    public void OnStartButtonPressed()
    {
        // Ensure duration is set before starting
        if (durationDropdown != null && durationDropdown.value >= 0 && durationDropdown.value < durations.Length)
        {
            DurationManager.DurationSeconds = durations[durationDropdown.value];
        }
        StartDisasterScene();
    }

    void StartDisasterScene()
    {
        // Use the selected disaster from dropdown
        string sceneName = SelectedDisaster;
        if (!string.IsNullOrEmpty(sceneName))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
    }
}
