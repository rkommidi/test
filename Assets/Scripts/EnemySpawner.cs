using UnityEngine;
using System.Collections;

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
        // Initial delay
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
