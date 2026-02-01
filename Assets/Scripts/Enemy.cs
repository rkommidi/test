using UnityEngine;

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

        // Random horizontal movement
        if (Random.value > 0.5f)
        {
            horizontalSpeed = Random.Range(-1f, 1f);
        }

        // Random wave movement
        if (Random.value > 0.7f)
        {
            amplitude = Random.Range(0.5f, 1.5f);
        }
    }

    void Update()
    {
        // Move down
        transform.Translate(Vector3.down * moveSpeed * Time.deltaTime, Space.World);

        // Horizontal movement
        if (horizontalSpeed != 0)
        {
            transform.Translate(Vector3.right * horizontalSpeed * Time.deltaTime, Space.World);
        }

        // Wave movement
        if (amplitude > 0)
        {
            float xOffset = Mathf.Sin((Time.time + timeOffset) * frequency) * amplitude * Time.deltaTime;
            transform.Translate(Vector3.right * xOffset, Space.World);
        }

        // Destroy if off screen
        if (transform.position.y < -6f)
        {
            // Points for dodging
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
        // Spawn effect
        if (destroyEffect != null)
        {
            Instantiate(destroyEffect, transform.position, Quaternion.identity);
        }

        // Play sound
        if (destroySound != null)
        {
            AudioSource.PlayClipAtPoint(destroySound, transform.position);
        }

        // Add score
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
