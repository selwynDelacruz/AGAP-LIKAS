using UnityEngine;
using TMPro;

public class SimulationResultSummary : MonoBehaviour
{
    [Header("Result UI - TextMeshPro References")]
    public TMP_Text rescuedVictimDataText;  // RescuedDATA
    public TMP_Text clearedRubbleDataText;  // RubbleDATA
    public TMP_Text medkitDataText;         // MedkitDATA
    public TMP_Text totalPointsDataText;    // JatotDATA

    [Header("UI Labels/Parents (Optional)")]
    public GameObject rubbleDataParent;     // Parent GameObject containing rubble UI elements

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    void Start()
    {
        DisplayPointResults();
    }

    private void DisplayPointResults()
    {
        // Get the selected disaster type from PlayerPrefs (set by LobbyManager)
        string disasterType = PlayerPrefs.GetString("DisasterType", "TestKen");

        // Try to get points from PlayerPrefs first (synced by GameManager before scene load)
        int totalPoints = PlayerPrefs.GetInt("FinalPoints_Total", -1);
        
        // If PlayerPrefs has synced data, use it
        if (totalPoints >= 0)
        {
            DisplayFromPlayerPrefs(disasterType);
            return;
        }

        // Fallback to PointManager if PlayerPrefs doesn't have data
        if (PointManager.Instance != null)
        {
            DisplayFromPointManager(disasterType);
            return;
        }

        // No data available
        Debug.LogError("[SimulationResultSummary] No point data found! Neither PlayerPrefs nor PointManager available.");
        SetDefaultValues(disasterType);
    }

    /// <summary>
    /// Display results from PlayerPrefs (synced by server before scene load)
    /// </summary>
    private void DisplayFromPlayerPrefs(string disasterType)
    {
        int totalPoints = PlayerPrefs.GetInt("FinalPoints_Total", 0);
        int rescuedPoints = PlayerPrefs.GetInt("FinalPoints_Rescued", 0);
        int healedPoints = PlayerPrefs.GetInt("FinalPoints_Healed", 0);
        int rubblePoints = PlayerPrefs.GetInt("FinalPoints_Rubble", 0);

        // Calculate counts based on points
        int rescuedCount = rescuedPoints / 20;  // 20 points per rescued victim
        int medkitCount = healedPoints / 10;     // 10 points per healed victim
        int rubbleCount = rubblePoints / 10;     // 10 points per cleared rubble

        if (showDebugLogs)
        {
            Debug.Log($"[SimulationResultSummary] Reading from PlayerPrefs - Total: {totalPoints}, Rescued: {rescuedCount}, Medkits: {medkitCount}, Rubble: {rubbleCount}");
        }

        DisplayResults(disasterType, rescuedCount, rubbleCount, medkitCount, totalPoints);
    }

    /// <summary>
    /// Display results from PointManager (fallback for non-networked or if PlayerPrefs not set)
    /// </summary>
    private void DisplayFromPointManager(string disasterType)
    {
        var pointLog = PointManager.Instance.GetPointLog();
        int totalPoints = PointManager.Instance.GetTotalPoints();

        // Calculate counts based on points
        int rescuedCount = 0;
        if (pointLog.ContainsKey("Rescued Victim"))
        {
            rescuedCount = pointLog["Rescued Victim"] / 20;
        }

        int rubbleCount = 0;
        if (pointLog.ContainsKey("Cleared Rubble"))
        {
            rubbleCount = pointLog["Cleared Rubble"] / 10;
        }

        int medkitCount = 0;
        if (pointLog.ContainsKey("Healed Victim"))
        {
            medkitCount = pointLog["Healed Victim"] / 10;
        }

        if (showDebugLogs)
        {
            Debug.Log($"[SimulationResultSummary] Reading from PointManager - Total: {totalPoints}, Rescued: {rescuedCount}, Medkits: {medkitCount}, Rubble: {rubbleCount}");
        }

        DisplayResults(disasterType, rescuedCount, rubbleCount, medkitCount, totalPoints);
    }

    /// <summary>
    /// Common method to display results based on disaster type
    /// </summary>
    private void DisplayResults(string disasterType, int rescuedCount, int rubbleCount, int medkitCount, int totalPoints)
    {
        if (disasterType == "Earthquake")
        {
            // EARTHQUAKE: Show Rescued, Rubble, Medkit, Total
            if (rescuedVictimDataText != null)
                rescuedVictimDataText.text = rescuedCount.ToString();

            if (clearedRubbleDataText != null)
                clearedRubbleDataText.text = rubbleCount.ToString();

            if (medkitDataText != null)
                medkitDataText.text = medkitCount.ToString();

            if (totalPointsDataText != null)
                totalPointsDataText.text = totalPoints.ToString();

            // Show rubble UI element
            if (rubbleDataParent != null)
                rubbleDataParent.SetActive(true);

            Debug.Log($"Earthquake Results - Rescued: {rescuedCount}, Rubble: {rubbleCount}, Medkits: {medkitCount}, Total: {totalPoints}");
        }
        else if (disasterType == "Flood")
        {
            // FLOOD: Show only Rescued, Medkit, Total (hide Rubble)
            if (rescuedVictimDataText != null)
                rescuedVictimDataText.text = rescuedCount.ToString();

            if (medkitDataText != null)
                medkitDataText.text = medkitCount.ToString();

            if (totalPointsDataText != null)
                totalPointsDataText.text = totalPoints.ToString();

            // Hide rubble UI element
            if (rubbleDataParent != null)
                rubbleDataParent.SetActive(false);

            Debug.Log($"Flood Results - Rescued: {rescuedCount}, Medkits: {medkitCount}, Total: {totalPoints}");
        }
        else // TestKen or other
        {
            // Default: Show all data
            if (rescuedVictimDataText != null)
                rescuedVictimDataText.text = rescuedCount.ToString();

            if (clearedRubbleDataText != null)
                clearedRubbleDataText.text = rubbleCount.ToString();

            if (medkitDataText != null)
                medkitDataText.text = medkitCount.ToString();

            if (totalPointsDataText != null)
                totalPointsDataText.text = totalPoints.ToString();

            if (rubbleDataParent != null)
                rubbleDataParent.SetActive(true);

            Debug.Log($"TestKen Results - Rescued: {rescuedCount}, Rubble: {rubbleCount}, Medkits: {medkitCount}, Total: {totalPoints}");
        }
    }

    private void SetDefaultValues(string disasterType)
    {
        // Set all values to 0 if no data available
        if (rescuedVictimDataText != null) rescuedVictimDataText.text = "0";
        if (medkitDataText != null) medkitDataText.text = "0";
        if (totalPointsDataText != null) totalPointsDataText.text = "0";

        // Only show rubble for Earthquake
        if (disasterType == "Earthquake" || disasterType == "TestKen")
        {
            if (clearedRubbleDataText != null) clearedRubbleDataText.text = "0";
            if (rubbleDataParent != null) rubbleDataParent.SetActive(true);
        }
        else if (disasterType == "Flood")
        {
            if (rubbleDataParent != null) rubbleDataParent.SetActive(false);
        }
    }
}