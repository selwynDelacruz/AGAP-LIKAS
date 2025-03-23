using UnityEngine;
using UnityEngine.SceneManagement;
using StarterAssets;

public class PauseMenu : MonoBehaviour

{
    public GameObject pauseMenuUI;
    public GameObject[] screenGUI;
    public bool isPaused;
    private StarterAssetsInputs playerInput;

    void Start()
    {
        Time.timeScale = 1f;
        pauseMenuUI.SetActive(false);
        playerInput = Object.FindFirstObjectByType<StarterAssetsInputs>();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                Resume();
            }
            else {
                Pause();
            }
        }
    }
    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        toggleOnScreenGUI(true);
        Time.timeScale = 1.0f;
        isPaused = false;

        Cursor.lockState = CursorLockMode.Locked; // Lock cursor back to center
        Cursor.visible = false; // Hide cursor

        if (playerInput != null)
            playerInput.cursorInputForLook = true; // Re-enable camera movement
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        toggleOnScreenGUI(false);

        Time.timeScale = 0f; // Pause Game
        isPaused = true;

        Cursor.lockState = CursorLockMode.None; // Unlock cursor for menu
        Cursor.visible = true; // Show cursor

        if (playerInput != null)
            playerInput.cursorInputForLook = false; // Disable camera movement
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene("Main Menu");
    }

    public void Quit()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }
  
    private void toggleOnScreenGUI(bool state)
    {
        foreach (GameObject canvas in screenGUI)
        {
            if (canvas != null)
            {
                canvas.SetActive(state);
            }
        }
    }
}
