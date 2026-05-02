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

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Loaded scene: " + scene.name);
        Debug.Log("Next driving level is: Drivinglevel" + nextDrivingLevel);

        if (scene.name == "BarScene")
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        if (scene.name.StartsWith("Drivinglevel"))
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void StartDriving()
    {
        string sceneToLoad = "Drivinglevel" + nextDrivingLevel;

        Debug.Log("Loading: " + sceneToLoad);

        SceneManager.LoadScene(sceneToLoad);
    }

    public void FinishDrivingLevel()
    {
        nextDrivingLevel++;

        Debug.Log("Driving level finished. Next driving level is now: Drivinglevel" + nextDrivingLevel);

        SceneManager.LoadScene("BarScene");
    }
}