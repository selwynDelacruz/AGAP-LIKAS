using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class QuizManager : MonoBehaviour
{
    public TMP_InputField questionInput;
    public TMP_InputField rightAnswerInput;
    public TMP_InputField wrongAnswerInput;

    public Button saveButton;
    //public Button startGameButton;

    private bool dataSaved = false;

    //void Start()
    //{
    //    startGameButton.interactable = false;
    //}

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
        Debug.Log("saved");
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
}
