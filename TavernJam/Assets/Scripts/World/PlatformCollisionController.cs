using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformCollisionController : MonoBehaviour
{
    public GameObject enemy;  // Reference to the enemy object
    public GameObject player; 

    private void Update()
    {
        if (enemy != null)
        {
            // Check if the enemy is below the platform
            bool isEnemyBelow = enemy.transform.position.y < transform.position.y;

            // Toggle platform collision based on the enemy's position
            ToggleCollision(!isEnemyBelow);
        }

        if (player != null)
        {
            // Check if the enemy is below the platform
            bool isPlayerBelow = enemy.transform.position.y < transform.position.y;

            // Toggle platform collision based on the enemy's position
            ToggleCollision(!isPlayerBelow);
        }
    }

    private void ToggleCollision(bool enableCollision)
    {
        Collider2D platformCollider = GetComponent<Collider2D>();

        if (platformCollider != null)
        {
            // Enable or disable the platform's collider
            platformCollider.enabled = enableCollision;
        }
    }
}