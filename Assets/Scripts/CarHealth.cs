using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CarHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 5;
    private int currentHealth;

    [Header("UI")]
    public Slider healthBar;
    public GameObject gameOverPanel;

    [Header("Damage Settings")]
    public LayerMask damageLayers;
    public float damageCooldown = 0.75f;

    private float lastDamageTime = -999f;
    private bool gameEnded = false;

    private void Start()
    {
        currentHealth = maxHealth;

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        Time.timeScale = 1f;
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryTakeDamage(collision.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        TryTakeDamage(other.gameObject);
    }

    private void TryTakeDamage(GameObject hitObject)
    {
        if (gameEnded)
        {
            return;
        }

        if (Time.time - lastDamageTime < damageCooldown)
        {
            return;
        }

        bool hitDamageLayer = (damageLayers.value & (1 << hitObject.layer)) != 0;

        if (!hitDamageLayer)
        {
            return;
        }

        lastDamageTime = Time.time;

        currentHealth--;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }

        Debug.Log("Car health: " + currentHealth);

        if (currentHealth <= 0)
        {
            EndGame();
        }
    }

    private void EndGame()
{
    gameEnded = true;

    Debug.Log("GAME OVER PANEL SHOULD SHOW NOW");

    if (gameOverPanel != null)
    {
        gameOverPanel.SetActive(true);
    }

    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true;

    Time.timeScale = 0f;
}

   public void RestartLevel()
{
    Debug.Log("Restart button clicked");

    Time.timeScale = 1f;
    UnityEngine.SceneManagement.SceneManager.LoadScene(
        UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
    );
}
}