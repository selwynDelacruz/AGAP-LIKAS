using UnityEngine;

/// <summary>
/// Singleton that persists game configuration across scenes
/// No PlayerPrefs needed - data only lives during game session
/// </summary>
public class GameConfig : MonoBehaviour
{
    public static GameConfig Instance { get; private set; }

    [Header("Game Settings")]
    public int TaskCount { get; set; } = 5;
    public int DisasterModeIndex { get; set; } = 1; // 0=Flood, 1=Earthquake, 2=Both
    public int GameDurationInSeconds { get; set; } = 300;

    private void Awake()
    {
        // Singleton pattern with DontDestroyOnLoad
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[GameConfig] Instance created and will persist across scenes");
        }
        else
        {
            Destroy(gameObject);
            Debug.Log("[GameConfig] Duplicate instance destroyed");
        }
    }

    /// <summary>
    /// Reset to default values
    /// </summary>
    public void ResetToDefaults()
    {
        TaskCount = 5;
        DisasterModeIndex = 1;
        GameDurationInSeconds = 300;
        Debug.Log("[GameConfig] Reset to default values");
    }

    /// <summary>
    /// Get disaster mode name for logging
    /// </summary>
    public string GetDisasterModeName()
    {
        return DisasterModeIndex switch
        {
            0 => "Flood",
            1 => "Earthquake",
            2 => "Both",
            _ => "Unknown"
        };
    }
}
