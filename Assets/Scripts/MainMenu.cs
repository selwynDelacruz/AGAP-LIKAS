using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void Flood()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        //Debug.Log(SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1));
        Debug.Log("Player choosed the Flood disaster mode");
    }

    public void Earthquake()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 2);
    }
}
