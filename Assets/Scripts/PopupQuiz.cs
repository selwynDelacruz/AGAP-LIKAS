using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class QuestionTriggerPopup : MonoBehaviour
{
    [Header("Question UI")]
    public GameObject questionDialogUI; // UI for question dialog
    public TMP_Text questionText; // Text for the dialog/question
    public Button answerButton1; //button for right answer
    public Button answerButton2; // button for wrong answer
    public TMP_Text resultText; // text for the result

    private string rightAnswer;
    private string wrongAnswer;

    private void Start()
    {
        if (questionDialogUI != null) questionDialogUI.SetActive(false);
        if (resultText != null) resultText.gameObject.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Victim"))
        {
            PauseGame();
            LoadQuestionData();
            ShowQuestionPanel();
        }
    }

    void LoadQuestionData()
    {
        string question = PlayerPrefs.GetString("Question");
        rightAnswer = PlayerPrefs.GetString("RightAnswer");
        wrongAnswer = PlayerPrefs.GetString("WrongAnswer");

        questionText.text = question;

        if (Random.value > 0.5f)
        {
            SetAnswerButton(answerButton1, rightAnswer, true);
            SetAnswerButton(answerButton2, wrongAnswer, false);
        }
        else
        {
            SetAnswerButton(answerButton1, wrongAnswer, false);
            SetAnswerButton(answerButton2, rightAnswer, true);
        }
    }

    void SetAnswerButton(Button button, string text, bool isCorrect)
    {
        button.GetComponentInChildren<TMP_Text>().text = text;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => OnAnswerSelected(isCorrect));
    }

    void OnAnswerSelected(bool isCorrect)
    {
        resultText.gameObject.SetActive(true);
        resultText.text = isCorrect ? "Correct Answer!" : "Wrong Answer!";
        StartCoroutine(HideQuestionPanelAfterDelay(2f));
    }

    IEnumerator HideQuestionPanelAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay); // Use realtime because game is paused
        questionDialogUI.SetActive(false);
        resultText.gameObject.SetActive(false);
        ResumeGame();
    }

    void ShowQuestionPanel()
    {
        questionDialogUI.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void PauseGame()
    {
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void ResumeGame()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
