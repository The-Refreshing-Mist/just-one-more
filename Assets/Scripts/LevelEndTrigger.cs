using UnityEngine;
using TMPro;

public class LevelEndTrigger : MonoBehaviour
{
    public TextMeshProUGUI levelCompleteText;

    private bool levelCompleted = false;

    private void Start()
    {
        Time.timeScale = 1f;

        if (levelCompleteText != null)
        {
            levelCompleteText.enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (levelCompleted)
            return;

        if (other.CompareTag("Player"))
        {
            levelCompleted = true;
            ShowLevelComplete();
            Time.timeScale = 0f;
        }
    }

    private void ShowLevelComplete()
    {
        if (levelCompleteText != null)
        {
            levelCompleteText.enabled = true;
            levelCompleteText.text = "Level Complete!\nPress Space to continue";
        }
    }

    private void Update()
    {
        if (!levelCompleted)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Time.timeScale = 1f;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.FinishDrivingLevel();
            }
            else
            {
                Debug.LogError("GameManager not found.");
            }
        }
    }
}