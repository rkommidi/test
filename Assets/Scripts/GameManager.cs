using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public bool isGameOver = false;
    public bool isPaused = false;
    public int score = 0;
    public int highScore = 0;

    [Header("References")]
    public UIManager uiManager;
    public PlayerController player;
    public EnemySpawner spawner;

    private const string HIGH_SCORE_KEY = "HighScore";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
    }

    void Start()
    {
        StartGame();
    }

    void Update()
    {
        // Pause toggle
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            TogglePause();
        }
    }

    public void StartGame()
    {
        isGameOver = false;
        isPaused = false;
        score = 0;
        Time.timeScale = 1f;

        if (uiManager != null)
        {
            uiManager.UpdateScore(score);
            uiManager.ShowGameUI();
        }

        if (spawner != null)
        {
            spawner.StartSpawning();
        }
    }

    public void AddScore(int points)
    {
        if (isGameOver) return;

        score += points;

        if (uiManager != null)
        {
            uiManager.UpdateScore(score);
        }

        // Increase difficulty based on score
        if (spawner != null && score % 500 == 0)
        {
            spawner.IncreaseDifficulty();
        }
    }

    public void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;
        Time.timeScale = 0f;

        // Update high score
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt(HIGH_SCORE_KEY, highScore);
            PlayerPrefs.Save();
        }

        if (spawner != null)
        {
            spawner.StopSpawning();
        }

        if (uiManager != null)
        {
            uiManager.ShowGameOver(score, highScore);
        }
    }

    public void TogglePause()
    {
        if (isGameOver) return;

        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;

        if (uiManager != null)
        {
            if (isPaused)
                uiManager.ShowPause();
            else
                uiManager.HidePause();
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
