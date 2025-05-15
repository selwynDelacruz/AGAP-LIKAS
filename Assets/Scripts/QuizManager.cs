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

    [Header("Buttons")]
    public Button saveButton;
    //public Button startGameButton;

    [Header("Dropdown & Image")]
    public TMP_Dropdown tmpDropdown;      // Reference to your TMP Dropdown
    public Image targetImage;             // Image that will change
    public Sprite[] optionSprites;        // Sprites to switch based on dropdown option

    private bool dataSaved = false;

    void Start()
    {
        // Optional: Set initial image based on default dropdown value
        tmpDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        OnDropdownValueChanged(tmpDropdown.value); // initialize the image
    }

    public void SaveData()
    {
        if (string.IsNullOrWhiteSpace(questionInput.text) ||
            string.IsNullOrWhiteSpace(rightAnswerInput.text) ||
            string.IsNullOrWhiteSpace(wrongAnswerInput.text))
        {
            Debug.LogWarning("All fields must be filled before saving.");
            return;
        }

        PlayerPrefs.SetString("Question", questionInput.text);
        PlayerPrefs.SetString("RightAnswer", rightAnswerInput.text);
        PlayerPrefs.SetString("WrongAnswer", wrongAnswerInput.text);
        PlayerPrefs.Save();

        Debug.Log("Saved");
        DisableInputs();
        dataSaved = true;
        //startGameButton.interactable = true;
    }

    void DisableInputs()
    {
        questionInput.interactable = false;
        rightAnswerInput.interactable = false;
        wrongAnswerInput.interactable = false;
        saveButton.interactable = false;
    }

    public void StartGame()
    {
        if (dataSaved)
        {
            SceneManager.LoadScene("GameScene"); // replace with your actual scene name
        }
    }

    // This is triggered when the dropdown changes
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
}
