using UnityEngine;
using TMPro;

public class SimulationResultSummary : MonoBehaviour
{
    [Header("Result UI")]
    public TMP_Text disasterTypeText;
    public TMP_Text timeDurationText;
    public TMP_Text correctAnswersText;
    public TMP_Text victimsSavedText;

    void Start()
    {
        // Disaster Type
        string disasterType = DropdownList.SelectedDisaster;
        if (string.IsNullOrEmpty(disasterType))
            disasterType = PlayerPrefs.GetString("LastDisasterType", "Unknown");

        // Time Duration (in seconds)
        int durationSeconds = DurationManager.DurationSeconds;
        int minutes = durationSeconds / 60;
        int seconds = durationSeconds % 60;
        string formattedDuration = $"{minutes:00}:{seconds:00}";

        // Correct Answers
        int correctAnswers = 0;
        
        // Victims Saved
        int victimsSaved = 0;
        var rescue = FindObjectOfType<RescueBoatInteraction>();
        if (rescue != null)
            victimsSaved = rescue.GetRescuedVictimCount();

        // Set UI
        if (disasterTypeText != null)
            disasterTypeText.text = $"Disaster Type: {disasterType}";
        if (timeDurationText != null)
            timeDurationText.text = $"Time Duration: {formattedDuration}";
        if (correctAnswersText != null)
            correctAnswersText.text = $"Correct Answers: {correctAnswers}";
        if (victimsSavedText != null)
            victimsSavedText.text = $"Victims Saved: {victimsSaved}";
    }
}