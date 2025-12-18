using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Allows the instructor (host) to end the game early with a button press.
/// Attach this to a Button GameObject in the Instructor UI Panel.
/// The button will only be visible/functional for the instructor.
/// </summary>
public class InstructorEndGameButton : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The button that ends the game (auto-detects if not assigned)")]
    [SerializeField] private Button endGameButton;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private bool isEnding = false;

    private void Start()
    {
        // Auto-detect button if not assigned
        if (endGameButton == null)
        {
            endGameButton = GetComponent<Button>();
        }

        // Setup button click listener
        if (endGameButton != null)
        {
            endGameButton.onClick.AddListener(OnEndGameButtonClicked);
        }
        else
        {
            Debug.LogError("[InstructorEndGameButton] End Game Button not found!");
        }

        // Check if user is instructor and hide button if not
        CheckInstructorAccess();
    }

    /// <summary>
    /// Checks if the current user is an instructor and shows/hides the button accordingly
    /// </summary>
    private void CheckInstructorAccess()
    {
        bool isInstructor = IsUserInstructor();

        // Show/hide the button based on instructor status
        if (endGameButton != null)
        {
            endGameButton.gameObject.SetActive(isInstructor);
        }

        if (showDebugLogs)
        {
            Debug.Log($"[InstructorEndGameButton] IsInstructor: {isInstructor}, Button visible: {isInstructor}");
        }
    }

    /// <summary>
    /// Called when the End Game button is clicked
    /// </summary>
    private void OnEndGameButtonClicked()
    {
        if (isEnding) return;

        if (showDebugLogs)
        {
            Debug.Log("[InstructorEndGameButton] End Game button clicked");
        }

        // Check if user is actually instructor (security check)
        if (!IsUserInstructor())
        {
            Debug.LogWarning("[InstructorEndGameButton] Non-instructor tried to end game!");
            return;
        }

        // End game immediately (no confirmation)
        EndGameNow();
    }

    /// <summary>
    /// Actually ends the game
    /// </summary>
    private void EndGameNow()
    {
        if (isEnding) return;
        isEnding = true;

        if (showDebugLogs)
        {
            Debug.Log("[InstructorEndGameButton] Instructor ending game now!");
        }

        // Disable button to prevent double-clicks
        if (endGameButton != null)
        {
            endGameButton.interactable = false;
        }

        // Find and use GameManager to end the game
        var gameManagerObj = GameObject.FindFirstObjectByType<MonoBehaviour>();
        
        // Look for GameManager specifically
        foreach (var mb in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
        {
            if (mb.GetType().Name == "GameManager")
            {
                // Use reflection to call InstructorEndGame
                var method = mb.GetType().GetMethod("InstructorEndGame");
                if (method != null)
                {
                    method.Invoke(mb, null);
                    return;
                }
            }
        }

        Debug.LogError("[InstructorEndGameButton] GameManager not found or InstructorEndGame method missing!");
        isEnding = false;
        if (endGameButton != null)
        {
            endGameButton.interactable = true;
        }
    }

    /// <summary>
    /// Security check to verify user is actually instructor
    /// </summary>
    private bool IsUserInstructor()
    {
        // Check via reflection to find GameManager.IsInstructor
        foreach (var mb in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
        {
            if (mb.GetType().Name == "GameManager")
            {
                var method = mb.GetType().GetMethod("IsInstructor");
                if (method != null)
                {
                    return (bool)method.Invoke(mb, null);
                }
            }
        }

        // Fallback: Check PlayerPrefs
        string userType = PlayerPrefs.GetString("Type_Of_User", "");
        return userType == "instructor";
    }

    private void OnDestroy()
    {
        if (endGameButton != null)
        {
            endGameButton.onClick.RemoveListener(OnEndGameButtonClicked);
        }
    }
}
