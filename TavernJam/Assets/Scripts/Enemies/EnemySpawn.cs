using System.Collections;
using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
    public GameObject enemyPrefab;
    private int numOfEnemies = 1;
    public GameObject player;
    private float canSpawnDistance = 5;
    private bool hasSpawned = false;
    private Vector2 spawnPos;
    public float spawnCooldown = 2f; // Set the cooldown time between spawns
    private int enemiesSpawned = 0;

    private void Awake()
    {
        spawnPos = transform.position;
        enemyPrefab.GetComponent<Enemy>().player = player;
    }

    private void Update()
    {
        float distanceToSpawn = Vector2.Distance(transform.position, player.transform.position);
        if (distanceToSpawn <= canSpawnDistance && !hasSpawned)
        {
            StartCoroutine(SpawnEnemiesWithCooldown());
        }
    }

    private IEnumerator SpawnEnemiesWithCooldown()
    {
        hasSpawned = true;

        for (int i = 0; i < numOfEnemies; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(spawnCooldown);
        }
    }

    private void SpawnEnemy()
    {
        GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        enemiesSpawned++;
     
    }

    public Vector2 GetSpawnPos()
    {
        return spawnPos;
    }
}