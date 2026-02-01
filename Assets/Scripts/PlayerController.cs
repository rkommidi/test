using UnityEngine;

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
        // Keyboard input
        float horizontalInput = Input.GetAxisRaw("Horizontal");

        if (horizontalInput != 0)
        {
            targetPosition.x = Mathf.Clamp(
                transform.position.x + horizontalInput * moveSpeed * Time.deltaTime,
                minX, maxX
            );
        }

        // Touch input for mobile
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector3 touchPos = Camera.main.ScreenToWorldPoint(touch.position);

            float screenThird = Screen.width / 3f;

            if (touch.position.x < screenThird)
            {
                // Left side - move left
                targetPosition.x = Mathf.Clamp(transform.position.x - moveSpeed * Time.deltaTime, minX, maxX);
            }
            else if (touch.position.x > screenThird * 2)
            {
                // Right side - move right
                targetPosition.x = Mathf.Clamp(transform.position.x + moveSpeed * Time.deltaTime, minX, maxX);
            }
            else if (touch.phase == TouchPhase.Began)
            {
                // Center - shoot
                Shoot();
            }
        }

        // Mouse input for testing
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

        // Keyboard shooting
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
