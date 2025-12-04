using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("UI References - Instructor")]
    [SerializeField] private Button instructorPlayGameButton;

    [Header("UI References - Trainee")]
    [SerializeField] private Button traineePlayGameButton;

    private void Start()
    {
        // Setup instructor button listener
        if (instructorPlayGameButton != null)
        {
            instructorPlayGameButton.onClick.AddListener(GoToLobby);
        }
        else
        {
            Debug.LogWarning("[MainMenu] Instructor Play Game button not assigned in Inspector!");
        }

        // Setup trainee button listener
        if (traineePlayGameButton != null)
        {
            traineePlayGameButton.onClick.AddListener(GoToLobby);
        }
        else
        {
            Debug.LogWarning("[MainMenu] Trainee Play Game button not assigned in Inspector!");
        }
    }

    /// <summary>
    /// Loads the LobbyMenu scene where users can create or join networked lobbies
    /// </summary>
    public void GoToLobby()
    {
        Debug.Log("Loading Lobby Menu scene...");
        SceneManager.LoadScene("LobbyMenu");
    }

    /// <summary>
    /// Quits the application
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Quit button pressed. Exiting application...");
        Application.Quit();
    }

    private void OnDestroy()
    {
        // Cleanup listeners
        if (instructorPlayGameButton != null)
        {
            instructorPlayGameButton.onClick.RemoveListener(GoToLobby);
        }

        if (traineePlayGameButton != null)
        {
            traineePlayGameButton.onClick.RemoveListener(GoToLobby);
        }
    }
}
