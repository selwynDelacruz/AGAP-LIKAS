using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class QuizManager : MonoBehaviour
{
    [Header("Input Fields")]
    public TMP_InputField questionInput;
    public TMP_InputField rightAnswerInput;
    public TMP_InputField wrongAnswerInput;

    [Header("Dropdown & Image")]
    public TMP_Dropdown tmpDropdown;
    public Image targetImage;
    public Sprite[] optionSprites;

    [Header("UI Elements")]
    public Button saveButton;
    public TMP_Text questionTotal;
    public TMP_Text errorText;

    private int questionCount = 0;
    private const int maxQuestions = 3;

    void Start()
    {
        // Clear previous saved data on startup
        ClearSavedData();

        // Setup dropdown listener and initial sprite
        tmpDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        OnDropdownValueChanged(tmpDropdown.value); // Set initial image

        // Reset count and update UI
        questionCount = 0;
        UpdateQuestionTotalText();
    }

    public void SaveData()
    {
        if (questionCount >= maxQuestions)
        {
            Debug.LogWarning("Maximum of 3 questions reached.");
            errorText.text = "Maximum of 3 questions reached.";
            return;
        }

        if (string.IsNullOrWhiteSpace(questionInput.text) ||
            string.IsNullOrWhiteSpace(rightAnswerInput.text) ||
            string.IsNullOrWhiteSpace(wrongAnswerInput.text))
        {
            Debug.LogWarning("All fields must be filled before saving.");
            errorText.text = "All fields must be filled before saving.";
            return;
        }

        PlayerPrefs.SetString($"Question_{questionCount}", questionInput.text);
        PlayerPrefs.SetString($"RightAnswer_{questionCount}", rightAnswerInput.text);
        PlayerPrefs.SetString($"WrongAnswer_{questionCount}", wrongAnswerInput.text);
        PlayerPrefs.SetInt($"DropdownIndex_{questionCount}", tmpDropdown.value);

        PlayerPrefs.Save();
        questionCount++;

        Debug.Log($"Saved question {questionCount}");

        ClearInputs();
        UpdateQuestionTotalText();
    }

    void OnDropdownValueChanged(int index)
    {
        if (index >= 0 && index < optionSprites.Length)
        {
            targetImage.sprite = optionSprites[index];
        }
        else
        {
            Debug.LogWarning("No sprite assigned for this index.");
        }
    }

    void ClearInputs()
    {
        questionInput.text = "";
        rightAnswerInput.text = "";
        wrongAnswerInput.text = "";
        tmpDropdown.value = 0;
    }

    public void StartGame()
    {
        if (questionCount > 0)
        {
            SceneManager.LoadScene("GameScene");
        }
        else
        {
            Debug.LogWarning("You must save at least 1 question to start.");
            errorText.text = "You must save at least 1 question to start.";
        }
    }

    public void ClearSavedData()
    {
        for (int i = 0; i < maxQuestions; i++)
        {
            PlayerPrefs.DeleteKey($"Question_{i}");
            PlayerPrefs.DeleteKey($"RightAnswer_{i}");
            PlayerPrefs.DeleteKey($"WrongAnswer_{i}");
            PlayerPrefs.DeleteKey($"DropdownIndex_{i}");
        }
        PlayerPrefs.Save();
        questionCount = 0;
        UpdateQuestionTotalText();
        Debug.Log("All saved questions cleared.");
    }

    int GetSavedQuestionCount()
    {
        int count = 0;
        for (int i = 0; i < maxQuestions; i++)
        {
            if (PlayerPrefs.HasKey($"Question_{i}"))
            {
                count++;
            }
        }
        return count;
    }

    void UpdateQuestionTotalText()
    {
        if (questionTotal != null)
        {
            questionTotal.text = $"Saved Questions: {questionCount}/{maxQuestions}";
        }
    }
}