using UnityEngine;

public class OptionsPopup : MonoBehaviour
{
    public GameObject optionsPanel;
    public bool pauseGameWhenOpen = true;

    void Start()
    {
        CloseOptions();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleOptions();
        }
    }

    public void OpenOptions()
    {
        optionsPanel.SetActive(true);

        if (pauseGameWhenOpen)
        {
            Time.timeScale = 0f;
        }
    }

    public void CloseOptions()
    {
        optionsPanel.SetActive(false);

        if (pauseGameWhenOpen)
        {
            Time.timeScale = 1f;
        }
    }

    public void ToggleOptions()
    {
        if (optionsPanel.activeSelf)
        {
            CloseOptions();
        }
        else
        {
            OpenOptions();
        }
    }
}