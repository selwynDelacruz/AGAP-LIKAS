using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class DropdownList : MonoBehaviour
{
    [Header("Disaster Selection")]
    [SerializeField] private TMP_Text modeText;
    [SerializeField] private Image disasterImage;
    [SerializeField] private Sprite floodSprite, earthquakeSprite;
    [SerializeField] private TMP_Dropdown disasterDropdown;

    [Header("Duration Selection")]
    [SerializeField] private TMP_Dropdown durationDropdown;

    private readonly int[] durations = { 60, 180, 300 }; // 1, 3, 5 minutes

    public static string SelectedDisaster { get; private set; } // Stores the selected value

    void Start()
    {
        // Disaster dropdown setup
        if (disasterDropdown.options.Count == 0)
        {
            List<string> options = new List<string> { "Flood", "Earthquake" };
            disasterDropdown.ClearOptions();
            disasterDropdown.AddOptions(options);
        }
        UpdateDisaster(disasterDropdown.value);
        disasterDropdown.onValueChanged.AddListener(UpdateDisaster);

        // Duration dropdown setup
        if (durationDropdown != null)
        {
            durationDropdown.onValueChanged.AddListener(OnDurationChanged);
        }

        // Set initial duration
        DurationManager.DurationSeconds = durations[0];
        SetDropdownToDuration(durations[0]);
    }

    public void UpdateDisaster(int index)
    {
        switch (index)
        {
            case 0:
                modeText.text = "Flood";
                disasterImage.sprite = floodSprite;
                SelectedDisaster = "Flood";
                break;
            case 1:
                modeText.text = "Earthquake";
                disasterImage.sprite = earthquakeSprite;
                SelectedDisaster = "Earthquake";
                break;
            default:
                modeText.text = "Unknown";
                disasterImage.sprite = null;
                SelectedDisaster = "";
                break;
        }
    }

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

    public void OnStartButton()
    {
        // Ensure duration is set before loading scene
        if (durationDropdown != null && durationDropdown.value >= 0 && durationDropdown.value < durations.Length)
        {
            DurationManager.DurationSeconds = durations[durationDropdown.value];
        }
        // Load the selected disaster scene
        if (!string.IsNullOrEmpty(SelectedDisaster))
        {
            SceneManager.LoadScene(SelectedDisaster);
        }
    }
}

