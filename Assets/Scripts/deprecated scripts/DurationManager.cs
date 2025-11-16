using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class DurationManager : MonoBehaviour
{
    [Header("UI Text References")]
    [SerializeField] private TMP_Text trainee1DurationText;
    [SerializeField] private TMP_Text trainee2DurationText;
    [SerializeField] private TMP_Text instructorDurationText;

    [Header("Timer Settings")]
    [SerializeField] private bool startOnAwake = true;

    private int totalDurationInSeconds;
    private int remainingTimeInSeconds;
    private bool isTimerRunning = false;

    private void Start()
    {
        // Get the duration from PlayerPrefs (set by LobbyManager)
        totalDurationInSeconds = PlayerPrefs.GetInt("GameDuration", 300); // Default to 5 minutes if not set
        remainingTimeInSeconds = totalDurationInSeconds;

        Debug.Log($"[DurationManager] Loaded duration: {totalDurationInSeconds} seconds ({totalDurationInSeconds / 60} minutes)");

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
            Debug.Log("[DurationManager] Timer started");
        }
    }

    /// <summary>
    /// Pauses the countdown timer
    /// </summary>
    public void PauseTimer()
    {
        isTimerRunning = false;
        Debug.Log("[DurationManager] Timer paused");
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
            Debug.Log("[DurationManager] Timer resumed");
        }
    }

    /// <summary>
    /// Resets the timer to the initial duration
    /// </summary>
    public void ResetTimer()
    {
        remainingTimeInSeconds = totalDurationInSeconds;
        UpdateAllTimerDisplays();
        Debug.Log("[DurationManager] Timer reset");
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
        Cursor.visible = true;
        isTimerRunning = false;
        Debug.Log("[DurationManager] Timer completed!");
        
        // You can add additional logic here, such as:
        // - Show a "Time's Up!" message
        // - End the game
        // - Load a results scene
        SceneManager.LoadScene("Result");
        // - Calculate scores

        UpdateAllTimerDisplays(); // Ensure it shows 00:00
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

    private void OnDestroy()
    {
        // Stop the timer when the object is destroyed
        isTimerRunning = false;
    }
}
