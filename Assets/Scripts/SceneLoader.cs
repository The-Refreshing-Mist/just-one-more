using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadTitleScreen()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Titlescreen");
    }

    public void LoadBarMiniGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("barscene");
    }

    public void LoadDrivingScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Drunk_driving");
    }

    public void RestartCurrentScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}