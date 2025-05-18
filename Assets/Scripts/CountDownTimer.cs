using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class CountDownTimer : MonoBehaviourPun
{
    public TMP_Text durationText;
    private float timeLeft;
    private bool isRunning = false;

    void Start()
    {
        timeLeft = DurationManager.DurationSeconds;
        isRunning = true;
        UpdateDurationText();
    }

    void Update()
    {
        if (isRunning && !DurationManager.IsPaused)
        {
            timeLeft -= Time.deltaTime;
            if (timeLeft < 0) timeLeft = 0;
            UpdateDurationText();

            if (timeLeft <= 0)
            {
                isRunning = false;
                if (PhotonNetwork.IsMasterClient)
                {
                    photonView.RPC("GoToResultScene", RpcTarget.All);
                }
            }
        }
    }

    [PunRPC]
    void GoToResultScene()
    {
        SceneManager.LoadScene("ResultScene"); // Use your actual result scene name
    }

    public void PauseTimer()
    {
        DurationManager.IsPaused = true;
    }

    public void ResumeTimer()
    {
        DurationManager.IsPaused = false;
    }

    void UpdateDurationText()
    {
        int minutes = Mathf.FloorToInt(timeLeft / 60);
        int seconds = Mathf.FloorToInt(timeLeft % 60);
        durationText.text = $"{minutes:00}:{seconds:00}";
    }
}
