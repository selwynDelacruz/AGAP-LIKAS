using TMPro;
using UnityEngine;

public class SimulationResultSummary : MonoBehaviour
{
    [Header("Result UI - TextMeshPro References")]
    [Header("Rescued Victim Section")]
    public TMP_Text rescueNumberTotal;      // Shows "rescued/total"
    public TMP_Text rescueMultiplier;       // Shows points per rescue
    public TMP_Text rescueSCORE;            // Shows total rescue points

    [Header("Medkit Section")]
    public TMP_Text medkitNumberTotal;      // Shows "healed/total"
    public TMP_Text medkitMultiplier;       // Shows points per medkit use
    public TMP_Text medkitSCORE;            // Shows total medkit points

    [Header("Rubble Section")]
    public TMP_Text rubbleNumberTotal;      // Shows "cleared/total"
    public TMP_Text rubbleMultiplier;       // Shows points per rubble
    public TMP_Text rubbleSCORE;            // Shows total rubble points
    public GameObject rubbleDataParent;     // Parent to show/hide for disaster type

    [Header("Total Score")]
    public TMP_Text totalScoreText;         // Shows total score (JatotDATA)

    [Header("Pass/Fail UI")]
    public GameObject passUI;               // PASS (TMP) GameObject
    public GameObject failUI;               // FAIL (TMP) GameObject

    [Header("Point Multipliers")]
    [SerializeField] private int pointsPerRescue = 20;
    [SerializeField] private int pointsPerMedkit = 10;
    [SerializeField] private int pointsPerRubble = 10;

    [Header("Pass/Fail Settings")]
    [SerializeField] [Range(0f, 1f)] private float passingPercentage = 0.75f; // 75%

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    void Start()
    {
        DisplayPointResults();
    }

    private void DisplayPointResults()
    {
        // Get the selected disaster type from PlayerPrefs
        string disasterType = PlayerPrefs.GetString("DisasterType", "TestKen");

        // Try to get points from PlayerPrefs first (synced by GameManager)
        int totalPoints = PlayerPrefs.GetInt("FinalPoints_Total", -1);
        
        if (totalPoints >= 0)
        {
            // Use synced data from PlayerPrefs
            DisplayFromPlayerPrefs(disasterType);
        }
        else if (PointManager.Instance != null)
        {
            // Fallback to PointManager
            DisplayFromPointManager(disasterType);
        }
        else
        {
            // No data available
            Debug.LogError("[SimulationResultSummary] No point data found!");
            SetDefaultValues(disasterType);
        }
    }

    private void DisplayFromPlayerPrefs(string disasterType)
    {
        int totalPoints = PlayerPrefs.GetInt("FinalPoints_Total", 0);
        int rescuedPoints = PlayerPrefs.GetInt("FinalPoints_Rescued", 0);
        int healedPoints = PlayerPrefs.GetInt("FinalPoints_Healed", 0);
        int rubblePoints = PlayerPrefs.GetInt("FinalPoints_Rubble", 0);

        // Calculate counts based on points and multipliers
        int rescuedCount = rescuedPoints / pointsPerRescue;
        int medkitCount = healedPoints / pointsPerMedkit;
        int rubbleCount = rubblePoints / pointsPerRubble;

        // Get total counts from PlayerPrefs (set by LobbyManager/GameManager)
        int totalVictims = PlayerPrefs.GetInt("TaskCount", 0);
        int totalMedkitVictims = PlayerPrefs.GetInt("MedkitVictimCount", medkitCount); // Use healed count as fallback
        int totalRubble = PlayerPrefs.GetInt("RubbleCount", rubbleCount); // Use cleared count as fallback

        if (showDebugLogs)
        {
            Debug.Log($"[SimulationResultSummary] PlayerPrefs - Rescued: {rescuedCount}/{totalVictims}, Medkits: {medkitCount}/{totalMedkitVictims}, Rubble: {rubbleCount}/{totalRubble}, Total: {totalPoints}");
        }

        DisplayResults(disasterType, rescuedCount, totalVictims, medkitCount, totalMedkitVictims, rubbleCount, totalRubble, rescuedPoints, healedPoints, rubblePoints, totalPoints);
    }

    private void DisplayFromPointManager(string disasterType)
    {
        var pointLog = PointManager.Instance.GetPointLog();
        int totalPoints = PointManager.Instance.GetTotalPoints();

        // Get points for each action
        int rescuedPoints = pointLog.ContainsKey("Rescued Victim") ? pointLog["Rescued Victim"] : 0;
        int healedPoints = pointLog.ContainsKey("Healed Victim") ? pointLog["Healed Victim"] : 0;
        int rubblePoints = pointLog.ContainsKey("Cleared Rubble") ? pointLog["Cleared Rubble"] : 0;

        // Calculate counts
        int rescuedCount = rescuedPoints / pointsPerRescue;
        int medkitCount = healedPoints / pointsPerMedkit;
        int rubbleCount = rubblePoints / pointsPerRubble;

        // Get total counts from PlayerPrefs (set by LobbyManager/GameManager)
        int totalVictims = PlayerPrefs.GetInt("TaskCount", 0);
        int totalMedkitVictims = PlayerPrefs.GetInt("MedkitVictimCount", medkitCount);
        int totalRubble = PlayerPrefs.GetInt("RubbleCount", rubbleCount);

        if (showDebugLogs)
        {
            Debug.Log($"[SimulationResultSummary] PointManager - Rescued: {rescuedCount}/{totalVictims}, Medkits: {medkitCount}/{totalMedkitVictims}, Rubble: {rubbleCount}/{totalRubble}, Total: {totalPoints}");
        }

        DisplayResults(disasterType, rescuedCount, totalVictims, medkitCount, totalMedkitVictims, rubbleCount, totalRubble, rescuedPoints, healedPoints, rubblePoints, totalPoints);
    }

    private void DisplayResults(string disasterType, int rescuedCount, int totalVictims, int medkitCount, int totalMedkitVictims, int rubbleCount, int totalRubble, int rescuedPoints, int healedPoints, int rubblePoints, int totalPoints)
    {
        // Display Rescued Victims
        if (rescueNumberTotal != null)
            rescueNumberTotal.text = $"{rescuedCount}/{totalVictims}";
        if (rescueMultiplier != null)
            rescueMultiplier.text = pointsPerRescue.ToString();
        if (rescueSCORE != null)
            rescueSCORE.text = rescuedPoints.ToString();

        // Display Medkit Usage
        if (medkitNumberTotal != null)
            medkitNumberTotal.text = $"{medkitCount}/{totalMedkitVictims}";
        if (medkitMultiplier != null)
            medkitMultiplier.text = pointsPerMedkit.ToString();
        if (medkitSCORE != null)
            medkitSCORE.text = healedPoints.ToString();

        // Display Cleared Rubble (disaster-specific)
        bool showRubble = (disasterType == "Earthquake" || disasterType == "TestKen");
        if (rubbleDataParent != null)
            rubbleDataParent.SetActive(showRubble);

        if (showRubble)
        {
            if (rubbleNumberTotal != null)
                rubbleNumberTotal.text = $"{rubbleCount}/{totalRubble}";
            if (rubbleMultiplier != null)
                rubbleMultiplier.text = pointsPerRubble.ToString();
            if (rubbleSCORE != null)
                rubbleSCORE.text = rubblePoints.ToString();
        }

        // Display Total Score
        if (totalScoreText != null)
            totalScoreText.text = totalPoints.ToString();

        // Calculate Pass/Fail based on rescued victims percentage
        bool passed = CalculatePassFail(rescuedCount, totalVictims);
        UpdatePassFailUI(passed);

        if (showDebugLogs)
        {
            float percentage = totalVictims > 0 ? (float)rescuedCount / totalVictims * 100f : 0f;
            Debug.Log($"[SimulationResultSummary] {disasterType} - Rescued: {rescuedCount}/{totalVictims} ({percentage:F1}%) - {(passed ? "PASS" : "FAIL")}");
        }
    }

    private bool CalculatePassFail(int rescuedCount, int totalVictims)
    {
        if (totalVictims == 0)
        {
            Debug.LogWarning("[SimulationResultSummary] Total victims is 0, defaulting to FAIL");
            return false;
        }

        float percentage = (float)rescuedCount / totalVictims;
        return percentage >= passingPercentage;
    }

    private void UpdatePassFailUI(bool passed)
    {
        if (passUI != null)
            passUI.SetActive(passed);
        
        if (failUI != null)
            failUI.SetActive(!passed);
    }

    private void SetDefaultValues(string disasterType)
    {
        // Get total counts from PlayerPrefs
        int totalVictims = PlayerPrefs.GetInt("TaskCount", 0);
        int totalMedkitVictims = PlayerPrefs.GetInt("MedkitVictimCount", 0);
        int totalRubble = PlayerPrefs.GetInt("RubbleCount", 0);

        // Display zeros for completed actions
        if (rescueNumberTotal != null) rescueNumberTotal.text = $"0/{totalVictims}";
        if (rescueMultiplier != null) rescueMultiplier.text = pointsPerRescue.ToString();
        if (rescueSCORE != null) rescueSCORE.text = "0";

        if (medkitNumberTotal != null) medkitNumberTotal.text = $"0/{totalMedkitVictims}";
        if (medkitMultiplier != null) medkitMultiplier.text = pointsPerMedkit.ToString();
        if (medkitSCORE != null) medkitSCORE.text = "0";

        bool showRubble = (disasterType == "Earthquake" || disasterType == "TestKen");
        if (rubbleDataParent != null)
            rubbleDataParent.SetActive(showRubble);

        if (showRubble)
        {
            if (rubbleNumberTotal != null) rubbleNumberTotal.text = $"0/{totalRubble}";
            if (rubbleMultiplier != null) rubbleMultiplier.text = pointsPerRubble.ToString();
            if (rubbleSCORE != null) rubbleSCORE.text = "0";
        }

        if (totalScoreText != null) totalScoreText.text = "0";

        // No victims rescued = FAIL
        UpdatePassFailUI(false);
    }
}