using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class QuestionTriggerPopup : MonoBehaviour
{
    [Header("Question UI")]
    public GameObject questionDialogUI;
    public TMP_Text questionText;
    public Button answerButton1;
    public Button answerButton2;
    public TMP_Text resultText;

    private string rightAnswer;
    private string wrongAnswer;

    // Store the current victim GameObject
    private GameObject currentVictim;
    private void Awake()
    {
        // Auto-assign UI references if not set in Inspector
        if (questionDialogUI == null)
            questionDialogUI = transform.Find("QuestionDialogUI")?.gameObject;

        if (questionText == null)
            questionText = questionDialogUI?.transform.Find("QuestionPanel/QuestionText")?.GetComponent<TMP_Text>();

        if (answerButton1 == null)
            answerButton1 = questionDialogUI?.transform.Find("QuestionPanel/QuestionButton1")?.GetComponent<Button>();

        if (answerButton2 == null)
            answerButton2 = questionDialogUI?.transform.Find("QuestionPanel/QuestionButton2")?.GetComponent<Button>();

        if (resultText == null)
            resultText = questionDialogUI?.transform.Find("QuestionPanel/QuestionResultText")?.GetComponent<TMP_Text>();
    }
    private void Start()
    {
        if (questionDialogUI != null) questionDialogUI.SetActive(false);
        if (resultText != null) resultText.gameObject.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Victim"))
        {
            VictimQuestion victim = other.GetComponent<VictimQuestion>();
            if (victim != null)
            {
                PauseGame();
                LoadQuestionData(victim.questionIndex);
                ShowQuestionPanel();
                currentVictim = other.gameObject; // Store reference to victim
            }
        }
    }

    void LoadQuestionData(int index)
    {
        string question = PlayerPrefs.GetString($"Question_{index}");
        rightAnswer = PlayerPrefs.GetString($"RightAnswer_{index}");
        wrongAnswer = PlayerPrefs.GetString($"WrongAnswer_{index}");

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
        yield return new WaitForSecondsRealtime(delay);
        questionDialogUI.SetActive(false);
        resultText.gameObject.SetActive(false);
        ResumeGame();

        // Destroy the victim after interaction
        if (currentVictim != null)
        {
            Destroy(currentVictim);
            currentVictim = null;
        }
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
