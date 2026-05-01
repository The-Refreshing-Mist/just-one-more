using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int currentDrivingLevel = 1;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void GoToBar()
    {
        SceneManager.LoadScene("BarScene");
    }

    public void StartDrivingLevel()
    {
        SceneManager.LoadScene("DrivingLevel" + currentDrivingLevel);
    }

    public void FinishDrivingLevel()
    {
        currentDrivingLevel++;
        SceneManager.LoadScene("BarScene");
    }
}