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
        ClearSavedData();
        tmpDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        OnDropdownValueChanged(tmpDropdown.value);
        questionCount = 0;
        UpdateQuestionTotalText();
    }

    public void SaveData()
    {
        if (questionCount >= maxQuestions)
        {
            errorText.text = "Maximum of 3 questions reached.";
            return;
        }

        if (string.IsNullOrWhiteSpace(questionInput.text) ||
            string.IsNullOrWhiteSpace(rightAnswerInput.text) ||
            string.IsNullOrWhiteSpace(wrongAnswerInput.text))
        {
            errorText.text = "All fields must be filled before saving.";
            return;
        }

        // Save question data
        PlayerPrefs.SetString($"Question_{questionCount}", questionInput.text);
        PlayerPrefs.SetString($"RightAnswer_{questionCount}", rightAnswerInput.text);
        PlayerPrefs.SetString($"WrongAnswer_{questionCount}", wrongAnswerInput.text);
        PlayerPrefs.SetInt($"DropdownIndex_{questionCount}", tmpDropdown.value);

        // Save dropdown option name for prefab matching
        string dropdownOptionName = tmpDropdown.options[tmpDropdown.value].text;
        PlayerPrefs.SetString($"DropdownOptionName_{questionCount}", dropdownOptionName);

        PlayerPrefs.Save();
        questionCount++;

        ClearInputs();
        UpdateQuestionTotalText();
    }

    void OnDropdownValueChanged(int index)
    {
        if (index >= 0 && index < optionSprites.Length)
        {
            targetImage.sprite = optionSprites[index];
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
            PlayerPrefs.DeleteKey($"DropdownOptionName_{i}");
        }
        PlayerPrefs.Save();
        questionCount = 0;
        UpdateQuestionTotalText();
    }

    void UpdateQuestionTotalText()
    {
        if (questionTotal != null)
        {
            questionTotal.text = $"Saved Questions: {questionCount}/{maxQuestions}";
        }
    }
}