using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public BaseStatsContainer baseStats;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;
    public float attackRadius = 2f;


    public float dashDistance = 2.0f;
    public float dashDuration = 1.0f;
    public float dashCooldown = 3f;

    private Rigidbody2D rb;
    private bool isDashing = false;
    private bool canDash = true;
    private bool isGrounded;
    private LayerMask groundLayer;
    private Vector2 movement;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        groundLayer = LayerMask.GetMask("Ground"); // Make sure to set the ground layer in Unity
        baseStats.CurrentHealth = baseStats.MaxHealth;
    }

    private void Update()
    {
        // Check if the player is grounded
        isGrounded = Physics2D.OverlapCircle(transform.position, 0.8f, groundLayer);

        // Handle player input
        float horizontalInput = Input.GetAxis("Horizontal");

        // Move the player

        if (!isDashing)
        {
            Move(horizontalInput);

            // Dashing
            // Check for dash input and cooldown
            if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
            {
                StartCoroutine(Dash(horizontalInput));
            }
            // Jumping
            if (isGrounded && Input.GetButtonDown("Jump"))
            {
                Jump();
            }

            // Apply gravity modifications for faster falling
            if (rb.velocity.y < 0)
            {
                ApplyFallMultiplier();
            }

            // Allows you to choose how far/high you jump by holding/releasing space
            else if (rb.velocity.y > 0 && !Input.GetButton("Jump"))
            {
                ApplyLowJumpMultiplier();
            }
        }
        if (Input.GetButtonDown("Fire1"))
        {
            MeleeAttack();
        }
    }

    private void Move(float horizontalInput)
    {
            movement = new Vector2(horizontalInput * baseStats.Speed, rb.velocity.y);
            rb.velocity = movement;
    }

    private void Jump()
    {
        Debug.Log("Jump");
        rb.velocity = new Vector2(rb.velocity.x, baseStats.jumpForce);
    }

    private void ApplyFallMultiplier()
    {
        rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
    }

    private void ApplyLowJumpMultiplier()
    {
        rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
    }


    private IEnumerator Dash(float horizontalInput)
    {
        isDashing = true;
        canDash = false;

        // Normalize the input to get the direction
        Vector2 dashDirection = new Vector2(horizontalInput, 0f).normalized;

        // Set velocity for the dash
        rb.velocity = dashDirection * dashDistance;

        // Wait for the dash duration
        yield return new WaitForSeconds(dashDuration);

        // Stop the dash
        isDashing = false;
        rb.velocity = new Vector2(0f, rb.velocity.y);

        // Set cooldown
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;

    }

    public void TakeDamage(float damage) 
    {
        baseStats.CurrentHealth -= damage;
        if (baseStats.CurrentHealth <= 0f)
        {
            OnDeath(gameObject);
        }
    }

    private void OnDeath(GameObject death) 
    {
        Destroy(death);
    }

    private void MeleeAttack() 
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, attackRadius);

        // Deal damage to enemies found
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                Enemy enemy = collider.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(baseStats.Attack);
                    Debug.Log(enemy.stats.CurrentHealth);
                }
            }
        }
    }
}