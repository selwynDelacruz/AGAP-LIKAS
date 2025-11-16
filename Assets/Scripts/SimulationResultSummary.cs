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

    void Start()
    {
        DisplayPointResults();
    }

    private void DisplayPointResults()
    {
        // Get the selected disaster type from PlayerPrefs (set by LobbyManager)
        string disasterType = PlayerPrefs.GetString("DisasterType", "TestKen");

        // Check if PointManager exists
        if (PointManager.Instance == null)
        {
            Debug.LogError("PointManager not found! Make sure it persists across scenes.");
            SetDefaultValues(disasterType);
            return;
        }

        // Get the point log from PointManager
        var pointLog = PointManager.Instance.GetPointLog();
        int totalPoints = PointManager.Instance.GetTotalPoints();

        // Calculate counts based on points
        // Rescued Victim = 20 points each
        int rescuedCount = 0;
        if (pointLog.ContainsKey("Rescued Victim"))
        {
            rescuedCount = pointLog["Rescued Victim"] / 20;
        }

        // Cleared Rubble = 10 points each
        int rubbleCount = 0;
        if (pointLog.ContainsKey("Cleared Rubble"))
        {
            rubbleCount = pointLog["Cleared Rubble"] / 10;
        }

        // Healed Victim (Give Medkit to Victim) = 10 points each
        int medkitCount = 0;
        if (pointLog.ContainsKey("Healed Victim"))
        {
            medkitCount = pointLog["Healed Victim"] / 10;
        }

        // Display data based on disaster type
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
        // Set all values to 0 if PointManager is missing
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