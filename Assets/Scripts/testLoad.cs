using UnityEngine;
using UnityEngine.SceneManagement;

public class testLoad : MonoBehaviour
{
    //Call this from a UI Button, passing the scene name
    public void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
