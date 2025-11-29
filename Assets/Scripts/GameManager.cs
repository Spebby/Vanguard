using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;


public class GameManager : MonoBehaviour {
    [SerializeField] PlayerHealth playerHealth;
    [SerializeField] GameObject gameOverPanel;
    [SerializeField] TextMeshProUGUI healthText;

    void Start() {
        Time.timeScale = 1f;
        gameOverPanel.SetActive(false);

        if (!playerHealth) {
            playerHealth = FindFirstObjectByType<PlayerHealth>();
            if (!playerHealth) throw new Exception("Expected to find PlayerHealth component in scene.");
        }
        
        playerHealth.OnDeath += EndGame;
    }

    void Update() {
        if (!healthText) return;
        healthText.text = playerHealth ? $"HP: {playerHealth.Health}" : "HP: 0";
    }

    void EndGame() {
        Time.timeScale = 0f;
        if (gameOverPanel) gameOverPanel.SetActive(true);
    }

    public void RestartGame() {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void OnDestroy() {
        if (!playerHealth) return;
        playerHealth.OnDeath -= EndGame;
    }
}