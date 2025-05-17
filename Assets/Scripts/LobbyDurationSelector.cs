using UnityEngine;
using TMPro; // Use this for TMP_Dropdown
using UnityEngine.SceneManagement;

public class LobbyDurationSelector : MonoBehaviour
{
    public TMP_Dropdown durationDropdown; // Change to TMP_Dropdown

    // Set these to match your dropdown options
    private readonly int[] durations = { 60, 180, 300 }; // 1, 3, 5 minutes

    void Start()
    {
        durationDropdown.onValueChanged.AddListener(OnDurationChanged);
    }

    void OnDurationChanged(int index)
    {
        DurationManager.DurationSeconds = durations[index];
    }

    public void OnStartButton()
    {
        // Ensure duration is set before loading scene
        DurationManager.DurationSeconds = durations[durationDropdown.value];
        SceneManager.LoadScene("Flood");
    }
}
