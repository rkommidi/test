using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

// ============================================================================
// SPACE RUNNER - Complete Unity Mobile Game
// Copy this entire file into your Unity project as SpaceRunnerGame.cs
// ============================================================================

// ============================================================================
// GAME MANAGER - Main game controller
// ============================================================================
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

// ============================================================================
// PLAYER CONTROLLER - Movement and shooting
// ============================================================================
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float smoothing = 0.1f;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 0.25f;

    [Header("Boundaries")]
    public float minX = -2.5f;
    public float maxX = 2.5f;

    [Header("Effects")]
    public ParticleSystem deathEffect;
    public AudioClip shootSound;
    public AudioClip deathSound;

    private float nextFireTime = 0f;
    private Vector3 targetPosition;
    private Vector3 velocity = Vector3.zero;
    private AudioSource audioSource;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        targetPosition = transform.position;
        audioSource = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver)
            return;

        HandleInput();
        Move();
    }

    void HandleInput()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");

        if (horizontalInput != 0)
        {
            targetPosition.x = Mathf.Clamp(
                transform.position.x + horizontalInput * moveSpeed * Time.deltaTime,
                minX, maxX
            );
        }

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            float screenThird = Screen.width / 3f;

            if (touch.position.x < screenThird)
            {
                targetPosition.x = Mathf.Clamp(transform.position.x - moveSpeed * Time.deltaTime, minX, maxX);
            }
            else if (touch.position.x > screenThird * 2)
            {
                targetPosition.x = Mathf.Clamp(transform.position.x + moveSpeed * Time.deltaTime, minX, maxX);
            }
            else if (touch.phase == TouchPhase.Began)
            {
                Shoot();
            }
        }

        if (Input.GetMouseButton(0))
        {
            float screenThird = Screen.width / 3f;
            Vector3 mousePos = Input.mousePosition;

            if (mousePos.x < screenThird)
            {
                targetPosition.x = Mathf.Clamp(transform.position.x - moveSpeed * Time.deltaTime, minX, maxX);
            }
            else if (mousePos.x > screenThird * 2)
            {
                targetPosition.x = Mathf.Clamp(transform.position.x + moveSpeed * Time.deltaTime, minX, maxX);
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            float screenThird = Screen.width / 3f;
            if (Input.mousePosition.x > screenThird && Input.mousePosition.x < screenThird * 2)
            {
                Shoot();
            }
        }

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKey(KeyCode.Space))
        {
            Shoot();
        }
    }

    void Move()
    {
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            smoothing
        );
    }

    void Shoot()
    {
        if (Time.time < nextFireTime) return;

        nextFireTime = Time.time + fireRate;

        if (bulletPrefab != null && firePoint != null)
        {
            Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

            if (audioSource != null && shootSound != null)
            {
                audioSource.PlayOneShot(shootSound);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Die();
        }
    }

    void Die()
    {
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        if (audioSource != null && deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position);
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
    }
}

// ============================================================================
// ENEMY - Enemy behavior
// ============================================================================
public class Enemy : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float horizontalSpeed = 0f;
    public float amplitude = 0f;
    public float frequency = 1f;

    [Header("Stats")]
    public int scoreValue = 25;
    public int health = 1;

    [Header("Effects")]
    public ParticleSystem destroyEffect;
    public AudioClip destroySound;

    private Vector3 startPosition;
    private float timeOffset;

    void Start()
    {
        startPosition = transform.position;
        timeOffset = Random.Range(0f, 2f * Mathf.PI);

        if (Random.value > 0.5f)
        {
            horizontalSpeed = Random.Range(-1f, 1f);
        }

        if (Random.value > 0.7f)
        {
            amplitude = Random.Range(0.5f, 1.5f);
        }
    }

    void Update()
    {
        transform.Translate(Vector3.down * moveSpeed * Time.deltaTime, Space.World);

        if (horizontalSpeed != 0)
        {
            transform.Translate(Vector3.right * horizontalSpeed * Time.deltaTime, Space.World);
        }

        if (amplitude > 0)
        {
            float xOffset = Mathf.Sin((Time.time + timeOffset) * frequency) * amplitude * Time.deltaTime;
            transform.Translate(Vector3.right * xOffset, Space.World);
        }

        if (transform.position.y < -6f)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(5);
            }
            Destroy(gameObject);
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (destroyEffect != null)
        {
            Instantiate(destroyEffect, transform.position, Quaternion.identity);
        }

        if (destroySound != null)
        {
            AudioSource.PlayClipAtPoint(destroySound, transform.position);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scoreValue);
        }

        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bullet"))
        {
            TakeDamage(1);
            Destroy(other.gameObject);
        }
    }
}

// ============================================================================
// BULLET - Projectile behavior
// ============================================================================
public class Bullet : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 12f;
    public int damage = 1;

    void Update()
    {
        transform.Translate(Vector3.up * speed * Time.deltaTime);

        if (transform.position.y > 6f)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
    }
}

// ============================================================================
// STAR - Collectible bonus
// ============================================================================
public class Star : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float rotationSpeed = 90f;

    [Header("Stats")]
    public int scoreValue = 100;

    [Header("Effects")]
    public ParticleSystem collectEffect;
    public AudioClip collectSound;

    void Update()
    {
        transform.Translate(Vector3.down * moveSpeed * Time.deltaTime, Space.World);
        transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);

        if (transform.position.y < -6f)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Collect();
        }
    }

    void Collect()
    {
        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }

        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scoreValue);
        }

        Destroy(gameObject);
    }
}

// ============================================================================
// ENEMY SPAWNER - Spawns enemies and stars
// ============================================================================
public class EnemySpawner : MonoBehaviour
{
    [Header("Spawning")]
    public GameObject[] enemyPrefabs;
    public GameObject starPrefab;
    public float spawnRate = 1.5f;
    public float starSpawnRate = 3f;
    public float minSpawnRate = 0.5f;

    [Header("Spawn Area")]
    public float minX = -2.5f;
    public float maxX = 2.5f;
    public float spawnY = 6f;

    private Coroutine spawnCoroutine;
    private Coroutine starCoroutine;
    private bool isSpawning = false;

    public void StartSpawning()
    {
        if (isSpawning) return;

        isSpawning = true;
        spawnCoroutine = StartCoroutine(SpawnEnemies());
        starCoroutine = StartCoroutine(SpawnStars());
    }

    public void StopSpawning()
    {
        isSpawning = false;

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }

        if (starCoroutine != null)
        {
            StopCoroutine(starCoroutine);
        }
    }

    public void IncreaseDifficulty()
    {
        spawnRate = Mathf.Max(minSpawnRate, spawnRate - 0.1f);
    }

    IEnumerator SpawnEnemies()
    {
        yield return new WaitForSeconds(1f);

        while (isSpawning)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(spawnRate);
        }
    }

    IEnumerator SpawnStars()
    {
        yield return new WaitForSeconds(2f);

        while (isSpawning)
        {
            SpawnStar();
            yield return new WaitForSeconds(starSpawnRate);
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;

        float xPos = Random.Range(minX, maxX);
        Vector3 spawnPos = new Vector3(xPos, spawnY, 0);

        int index = Random.Range(0, enemyPrefabs.Length);
        Instantiate(enemyPrefabs[index], spawnPos, Quaternion.identity);
    }

    void SpawnStar()
    {
        if (starPrefab == null) return;

        float xPos = Random.Range(minX, maxX);
        Vector3 spawnPos = new Vector3(xPos, spawnY, 0);

        Instantiate(starPrefab, spawnPos, Quaternion.identity);
    }
}

// ============================================================================
// UI MANAGER - Handles all UI
// ============================================================================
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

// ============================================================================
// BACKGROUND SCROLLER - Scrolling star background
// ============================================================================
public class BackgroundScroller : MonoBehaviour
{
    [Header("Scrolling")]
    public float scrollSpeed = 0.5f;
    public float resetPosition = -20f;
    public float startPosition = 20f;

    [Header("Stars")]
    public int starCount = 50;
    public float minStarSize = 0.02f;
    public float maxStarSize = 0.08f;
    public float minBrightness = 0.3f;
    public float maxBrightness = 1f;

    private Transform[] stars;

    void Start()
    {
        CreateStars();
    }

    void CreateStars()
    {
        stars = new Transform[starCount];

        for (int i = 0; i < starCount; i++)
        {
            GameObject star = new GameObject("Star");
            star.transform.parent = transform;

            SpriteRenderer sr = star.AddComponent<SpriteRenderer>();
            sr.sprite = CreateCircleSprite();

            float brightness = Random.Range(minBrightness, maxBrightness);
            sr.color = new Color(brightness, brightness, brightness, brightness);

            float x = Random.Range(-4f, 4f);
            float y = Random.Range(-6f, 6f);
            star.transform.position = new Vector3(x, y, 1f);

            float size = Random.Range(minStarSize, maxStarSize);
            star.transform.localScale = Vector3.one * size;

            stars[i] = star.transform;
        }
    }

    Sprite CreateCircleSprite()
    {
        int size = 32;
        Texture2D texture = new Texture2D(size, size);

        Color[] pixels = new Color[size * size];
        float center = size / 2f;
        float radius = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                if (dist < radius)
                {
                    float alpha = 1f - (dist / radius);
                    pixels[y * size + x] = new Color(1, 1, 1, alpha);
                }
                else
                {
                    pixels[y * size + x] = Color.clear;
                }
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    void Update()
    {
        if (stars == null) return;

        for (int i = 0; i < stars.Length; i++)
        {
            if (stars[i] == null) continue;

            stars[i].Translate(Vector3.down * scrollSpeed * Time.deltaTime, Space.World);

            if (stars[i].position.y < resetPosition)
            {
                float x = Random.Range(-4f, 4f);
                stars[i].position = new Vector3(x, startPosition, 1f);
            }
        }
    }
}
