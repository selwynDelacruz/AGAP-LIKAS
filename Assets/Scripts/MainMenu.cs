 using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void GoToLobby()
    {
        Debug.Log("Loading Lobby scene...");
        SceneManager.LoadScene("Lobby");
    }

    public void QuitGame()
    {
        Debug.Log("Quit button pressed. Exiting application...");
        Application.Quit();

    }
}
