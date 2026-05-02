using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int nextDrivingLevel = 1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void GoToNextDrivingLevel()
    {
        SceneManager.LoadScene("Drivinglevel" + nextDrivingLevel);
    }

    public void FinishDrivingLevel()
    {
        nextDrivingLevel++;
        SceneManager.LoadScene("BarScene");
    }
}