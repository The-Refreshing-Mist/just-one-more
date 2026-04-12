using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;  // Add this namespace to work with TextMeshProUGUI

public class LevelEndTrigger : MonoBehaviour
{
    public string nextSceneName;           // The name of the next scene to load
    public TextMeshProUGUI levelCompleteText;  // Reference to the TextMeshProUGUI component for the "Level Complete" text
    public GameObject car;                 // Reference to the car GameObject (or player)

    private void Start()
    {
        // Initially hide the "Level Complete" text
        if (levelCompleteText != null)
        {
            levelCompleteText.enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object entering the trigger is the player (tagged as "Player")
        if (other.CompareTag("Player"))
        {
            // Show the "Level Complete" text
            ShowLevelComplete();
            
            // Pause the game
            Time.timeScale = 0f;
        }
    }

    private void ShowLevelComplete()
    {
        // Ensure the level complete text is shown when the player reaches the endpoint
        if (levelCompleteText != null)
        {
            levelCompleteText.enabled = true;  // Display the "Level Complete" message
        }
    }

    private void Update()
    {
        // If the game is paused (Time.timeScale == 0)
        if (Time.timeScale == 0f)
        {
            // Wait for the player to press the Spacebar to continue to the next scene
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // Load the next level (or scene) specified in the inspector
                SceneManager.LoadScene(nextSceneName);
            }
        }
    }
}