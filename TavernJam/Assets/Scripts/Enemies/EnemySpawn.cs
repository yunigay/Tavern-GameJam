using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
    public GameObject enemyPrefab;
    public int numOfEnemies = 1;
    public GameObject player;
    private float canSpawnDistance = 5;
    private bool hasSpawned = false;

    private void Update()
    {
        float distanceToSpawn = Vector2.Distance(transform.position, player.transform.position);
        if (distanceToSpawn <= canSpawnDistance && !hasSpawned)
        {
            SpawnEnemy(enemyPrefab);

        }

    }
    private void SpawnEnemy(GameObject enemyPrefab)
    {
      
        GameObject enemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
            hasSpawned = true;
       


    }

}