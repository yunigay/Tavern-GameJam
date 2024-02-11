using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
    public GameObject enemyPrefab;
    private int numOfEnemies = 1;
    public GameObject player;
    private float canSpawnDistance = 5;
    private bool hasSpawned = false;
    private Vector2 spawnPos;


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
            SpawnEnemy();

        }
    }

    private void SpawnEnemy()
    {
        GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        hasSpawned = true;
    }

    public Vector2 GetSpawnPos()
    {
        return spawnPos;
    }
}