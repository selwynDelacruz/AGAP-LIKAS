using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    //Call this from a UI Button, passing the scene name
    public void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // Call this from "Return to Main Menu" button in Result Scene
    public void ResetAndReturnToMainMenu()
    {
        // Reset points before returning to main menu
        if (PointManager.Instance != null)
        {
            PointManager.Instance.ResetPoints();
            Debug.Log("Points reset. Returning to Main Menu.");
        }

        // Load the main menu scene (adjust scene name if different)
        SceneManager.LoadScene("Main Menu");
    }
}
