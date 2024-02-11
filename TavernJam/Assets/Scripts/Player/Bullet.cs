using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private float damage;
    private Rigidbody2D rb;

    // Set a threshold speed for the bullet to deal damage
    public float damageThresholdSpeed = 10f;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetDamage(float damageValue)
    {
        damage = damageValue;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Enemy"))
        {
            Enemy enemy = collision.collider.GetComponent<Enemy>();
            if (rb.velocity.magnitude >= damageThresholdSpeed)
            {
               
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }
                rb.AddForce(Vector2.up * 15f, ForceMode2D.Impulse);
            }
            else 
            {
                if (enemy.runAway == false)
                {
                    enemy.runAway = true;
                    Destroy(gameObject);
                }

            }
        }
    }
   
}
