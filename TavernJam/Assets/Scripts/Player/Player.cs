using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Player : MonoBehaviour
{
    public HealthComponent health;
    public BaseStatsContainer baseStats;
    public BaseStatsContainer bigFormStats;
    public BaseStatsContainer mediumFormStats;
    public BaseStatsContainer smallFormStats;

    public Animator bigFormAnimator;
    public Animator mediumFormAnimator;
    public Animator smallFormAnimator;
    private enum PlayerForm
    {
        Big,
        Medium,
        Small
    }

    private PlayerForm currentForm;

    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;
    public float attackRadius = 2f;


    public float dashDuration = 1.0f;

    private Animator animator;
    AnimatorStateInfo stateInfo;

    private bool isSliding = false;
    private float slideDuration = 2.0f;
    private float slideTimer = 0.0f;


    private Rigidbody2D rb;
    private bool isDashing = false;
    private bool isJumping = false;
    private bool canDash = true;
    private bool isGrounded;
    private LayerMask groundLayer;
    private Vector2 movement;

    private float groundCheckDistance = 0.2f;

    private Enemy enemy;


    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        groundLayer = LayerMask.GetMask("Ground"); // Make sure to set the ground layer in Unity
        SwitchForm(PlayerForm.Big);
    }


    private void Update()
    {
        stateInfo = animator.GetCurrentAnimatorStateInfo(0);

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

            if (isGrounded)
            {
                isJumping = false;
            }
            else if(!isGrounded && !isDashing && !isSliding)
            {
                if(!stateInfo.IsName("Dash"))
                    animator.Play("Jump");
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
            if (isSliding)
            {
                Slide();
            }
        }
        if (Input.GetButtonDown("Fire1"))
        {
            MeleeAttack();
        }
    }

    private void Move(float horizontalInput)
    {
        if (IsGroundedOnSides())
        {
            movement = new Vector2(horizontalInput * baseStats.Speed, rb.velocity.y);
            rb.velocity = movement;
            if (isGrounded && !isJumping && !isSliding)
            {
                if (rb.velocity.magnitude == 0 || horizontalInput == 0)
                {
                    animator.Play("Idle");
                }
                else
                {
                    animator.Play("Run");
                }
            }
        }
    }

    private void Jump()
    {
        isJumping = true;
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
        float verticalInput = Input.GetAxis("Vertical");
        isDashing = true;
        canDash = false;

        // Normalize the input to get the direction
        Vector2 dashDirection = new Vector2(horizontalInput, 0f).normalized;

        // Set velocity for the dash
        rb.velocity = new Vector2(dashDirection.x * baseStats.dashSpeed, verticalInput * 300f / baseStats.dashSpeed);
        animator.Play("Dash");

        // Wait for the dash duration
        yield return new WaitForSeconds(dashDuration);



        // Stop the dash and prevent upward movement
        isDashing = false;
        if (isGrounded)
        {
            isSliding = true;
        }


        // Set cooldown
        yield return new WaitForSeconds(baseStats.dashCooldown);
        canDash = true;

    }

    private void Slide()
    {
        if (isGrounded)
        {
            // Update the sliding timer
            slideTimer += Time.deltaTime;

            // Calculate the percentage of slide completion
            float slideProgress = slideTimer / slideDuration;

            // Gradually reduce velocity during the sliding state for the second half of the duration
            if (slideProgress > 0.5f)
            {
                float t = (slideProgress - 0.5f) / 0.5f;
                rb.velocity = new Vector2(Mathf.Lerp(rb.velocity.x, 0f, t), rb.velocity.y);
            }
            else
            {
                // During the first half of the duration, increase the speed
                float newSpeed = Mathf.Lerp(0f, baseStats.Speed * 2f, slideProgress * 2f);
                rb.velocity = new Vector2(newSpeed, rb.velocity.y);
            }

            // Play the appropriate animation based on slide progress
            if (slideProgress < 0.5f)
            {
                animator.Play("SlideStart");
            }
            else
            {
                animator.Play("SlideEnd");

                // Delay for the "getting up" period
                float gettingUpDelay = animator.GetCurrentAnimatorClipInfo(0)[0].clip.length;
                StartCoroutine(GettingUp(gettingUpDelay));
            }
        }
        else
        {
            isSliding = false;
            slideTimer = 0.0f;
        }
    }

    private IEnumerator GettingUp(float delay)
    {
        yield return new WaitForSeconds(delay);

        // After the delay, allow other movement inputs
        isSliding = false;
        slideTimer = 0.0f;
    }


    private bool IsGroundedOnSides()
    {
        // Get the bounds of the player's collider
        Bounds bounds = GetComponent<Collider2D>().bounds;

        // Define the positions for the raycasts
        Vector2 topLeft = new Vector2(bounds.min.x, bounds.max.y);
        Vector2 topRight = new Vector2(bounds.max.x, bounds.max.y);
        Vector2 bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        Vector2 bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        Vector2 middleLeft = new Vector2(bounds.min.x, bounds.center.y);
        Vector2 middleRight = new Vector2(bounds.max.x, bounds.center.y);

        // Check if the player is not in contact with the ground on the sides
        RaycastHit2D hitTopLeft = Physics2D.Raycast(topLeft, Vector2.left, groundCheckDistance, groundLayer);
        RaycastHit2D hitTopRight = Physics2D.Raycast(topRight, Vector2.right, groundCheckDistance, groundLayer);
        RaycastHit2D hitBottomLeft = Physics2D.Raycast(bottomLeft, Vector2.left, groundCheckDistance, groundLayer);
        RaycastHit2D hitBottomRight = Physics2D.Raycast(bottomRight, Vector2.right, groundCheckDistance, groundLayer);
        RaycastHit2D hitMiddleLeft = Physics2D.Raycast(middleLeft, Vector2.left, groundCheckDistance, groundLayer);
        RaycastHit2D hitMiddleRight = Physics2D.Raycast(middleRight, Vector2.right, groundCheckDistance, groundLayer);

        // Visualize the raycasts
        Debug.DrawLine(topLeft, topLeft + Vector2.left * groundCheckDistance, Color.red);
        Debug.DrawLine(topRight, topRight + Vector2.right * groundCheckDistance, Color.blue);
        Debug.DrawLine(bottomLeft, bottomLeft + Vector2.left * groundCheckDistance, Color.green);
        Debug.DrawLine(bottomRight, bottomRight + Vector2.right * groundCheckDistance, Color.yellow);
        Debug.DrawLine(middleLeft, middleLeft + Vector2.left * groundCheckDistance, Color.cyan);
        Debug.DrawLine(middleRight, middleRight + Vector2.right * groundCheckDistance, Color.magenta);

        return (hitTopLeft.collider == null && hitTopRight.collider == null) &&
               (hitBottomLeft.collider == null && hitBottomRight.collider == null) &&
               (hitMiddleLeft.collider == null && hitMiddleRight.collider == null);
    }

  


    public void TakeDamage(float damage) 
    {
        health.ReceiveDamage(damage);

        // Check for form switch based on health
        if (health.GetCurrentHealth() <= 0f)
        {
            OnDeath(gameObject);
        }
        else if (health.GetCurrentHealth() <= mediumFormStats.MaxHealth && currentForm != PlayerForm.Small)
        {
            SwitchForm(PlayerForm.Small);
        }
        else if (health.GetCurrentHealth() <= bigFormStats.MaxHealth && currentForm != PlayerForm.Medium)
        {
            SwitchForm(PlayerForm.Medium);
        }
    }

    private void OnDeath(GameObject death) 
    {
        Destroy(death);
    }

    private void MeleeAttack()
    {
        // Cast a ray from the mouse position
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

        RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

        if (hit.collider != null)
        {
            // Check if the hit collider is an enemy
            if (hit.collider.CompareTag("Enemy"))
            {
                Enemy enemy = hit.collider.GetComponent<Enemy>();

                // Check if the enemy is within the attack radius
                float distanceToEnemy = Vector2.Distance(transform.position, enemy.transform.position);

                if (distanceToEnemy <= attackRadius)
                {
                    // Deal damage to the enemy
                    enemy.TakeDamage(baseStats.Attack);
                    Debug.Log(enemy.stats.CurrentHealth);
                }
            }
        }
    }


    private void SwitchForm(PlayerForm newForm)
    {
        // Update the current form
        currentForm = newForm;

        // Adjust properties based on the new form
        switch (currentForm)
        {
            case PlayerForm.Big:
                baseStats = bigFormStats;
                animator.runtimeAnimatorController = GetComponent<Animator>().runtimeAnimatorController;
                break;
            case PlayerForm.Medium:
                baseStats = mediumFormStats;
                animator.runtimeAnimatorController = mediumFormAnimator.runtimeAnimatorController;
                break;
            case PlayerForm.Small:
                baseStats = smallFormStats;
                animator.runtimeAnimatorController = smallFormAnimator.runtimeAnimatorController;
                break;
        }
    }

}
