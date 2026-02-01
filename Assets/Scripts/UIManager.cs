using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Game UI")]
    public GameObject gameUI;
    public Text scoreText;

    [Header("Pause UI")]
    public GameObject pauseUI;

    [Header("Game Over UI")]
    public GameObject gameOverUI;
    public Text finalScoreText;
    public Text highScoreText;

    [Header("Main Menu UI")]
    public GameObject mainMenuUI;
    public Text menuHighScoreText;

    void Start()
    {
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        HideAll();

        if (mainMenuUI != null)
        {
            mainMenuUI.SetActive(true);

            if (menuHighScoreText != null)
            {
                int highScore = PlayerPrefs.GetInt("HighScore", 0);
                menuHighScoreText.text = "High Score: " + highScore;
            }
        }
    }

    public void ShowGameUI()
    {
        HideAll();

        if (gameUI != null)
        {
            gameUI.SetActive(true);
        }
    }

    public void ShowPause()
    {
        if (pauseUI != null)
        {
            pauseUI.SetActive(true);
        }
    }

    public void HidePause()
    {
        if (pauseUI != null)
        {
            pauseUI.SetActive(false);
        }
    }

    public void ShowGameOver(int score, int highScore)
    {
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);

            if (finalScoreText != null)
            {
                finalScoreText.text = "Score: " + score;
            }

            if (highScoreText != null)
            {
                highScoreText.text = "Best: " + highScore;
            }
        }
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }

    void HideAll()
    {
        if (mainMenuUI != null) mainMenuUI.SetActive(false);
        if (gameUI != null) gameUI.SetActive(false);
        if (pauseUI != null) pauseUI.SetActive(false);
        if (gameOverUI != null) gameOverUI.SetActive(false);
    }

    // Button callbacks
    public void OnStartButton()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }
    }

    public void OnResumeButton()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TogglePause();
        }
    }

    public void OnRestartButton()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }

    public void OnMainMenuButton()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadMainMenu();
        }
    }
}
