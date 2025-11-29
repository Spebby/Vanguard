using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    [SerializeField] PlayerHealth playerHealth;
    [SerializeField] GameObject gameOverPanel;

    // Reference to the text component for HP display
    [SerializeField] TextMeshProUGUI healthText;

    void Start()
    {
        // Reset time scale to normal when game starts
        Time.timeScale = 1f;

        // Ensure the Game Over UI is hidden
        if (gameOverPanel) gameOverPanel.SetActive(false);

        // Listen for player death
        if (playerHealth)
        {
            playerHealth.OnDeath += EndGame;
        }
    }

    void Update()
    {
        // Update the health text every frame
        if (healthText != null)
        {
            if (playerHealth != null)
            {
                // Display current health
                healthText.text = $"HP: {playerHealth.Health}";
            }
            else
            {
                // If player is destroyed (null), show 0
                healthText.text = "HP: 0";
            }
        }
    }

    void EndGame()
    {
        if (Time.timeScale == 0) return;

        // Pause the game
        Time.timeScale = 0f;

        // Show the Game Over UI
        if (gameOverPanel) gameOverPanel.SetActive(true);
    }

    public void RestartGame()
    {
        // Unpause and reload the scene
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void OnDestroy()
    {
        if (playerHealth)
        {
            playerHealth.OnDeath -= EndGame;
        }
    }
}