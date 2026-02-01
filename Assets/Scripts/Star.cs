using UnityEngine;

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
        // Move down
        transform.Translate(Vector3.down * moveSpeed * Time.deltaTime, Space.World);

        // Rotate
        transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);

        // Destroy if off screen
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
        // Spawn effect
        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }

        // Play sound
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }

        // Add score
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scoreValue);
        }

        Destroy(gameObject);
    }
}
