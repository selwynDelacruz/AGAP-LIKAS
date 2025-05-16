using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
public class SituationalQuestion
{
    public string question;
    public string rightAnswer;
    public string wrongAnswer;
}

public class SituationalQuestionManager : MonoBehaviour
{
    public TMP_InputField questionInput;
    public TMP_InputField rightAnswerInput;
    public TMP_InputField wrongAnswerInput;

    public List<SituationalQuestion> savedQuestions = new List<SituationalQuestion>();

    public void SaveQuestion()
    {
        SituationalQuestion newQuestion = new SituationalQuestion
        {
            question = questionInput.text,
            rightAnswer = rightAnswerInput.text,
            wrongAnswer = wrongAnswerInput.text
        };

        savedQuestions.Add(newQuestion);
        Debug.Log("Question saved: " + newQuestion.question);
    }
}
