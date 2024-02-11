using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    public float finalPoints;

    public float attackRadius = 3.5f;

    private bool canAttack = true;

    private bool isAttacking = false;

    public GameObject bulletPrefab;
    public Transform bulletSpawnPoint;


    private bool canShoot = true;

    public bool isDying = false;

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
    private bool isTakingDamage = false;
    private bool hasWallJumped = false;
    private bool isGrounded;
    private bool isOnPlatform;
    private bool isWall;
    private LayerMask groundLayer;
    private LayerMask platformLayer;
    private LayerMask wallLayer;
    private Vector2 movement;

    private float groundCheckDistance = 0.8f;

    private Enemy enemy;

    float horizontalInput;
    float meleePointOffset = 1.5f;


    private SoundManger soundManager;
    public AudioClip takeDamageSound; // Assign your sound effect in the Unity Editor or through code
    public AudioClip throwAttackSound; // Assign your sound effect in the Unity Editor or through code
    public AudioClip deathSound; // Assign your sound effect in the Unity Editor or through code
    public AudioClip pickUpSound; // Assign your sound effect in the Unity Editor or through code
    public AudioClip jumpSound; // Assign your sound effect in the Unity Editor or through code
    public AudioClip dashSound; // Assign your sound effect in the Unity Editor or through code

    private float timer = 0f;

    public bool youWon = false;
    private void Start()
    {
        soundManager = GetComponent<SoundManger>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        groundLayer = LayerMask.GetMask("Ground"); // Make sure to set the ground layer in Unity
        platformLayer = LayerMask.GetMask("Platform"); // Make sure to set the ground layer in Unity
        wallLayer = LayerMask.GetMask("Wall"); // Make sure to set the ground layer in Unity
        SwitchForm(PlayerForm.Big);
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        timer = 0f;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Bread"))
        {
            soundManager.PlaySoundEffect(pickUpSound);
            if (health.GetCurrentHealth() <= smallFormStats.MaxHealth)
            {
                SwitchForm(PlayerForm.Medium);
                health.SetMaxHealth(mediumFormStats.MaxHealth);
                health.SetCurrentHealth(mediumFormStats.CurrentHealth);
            }
            else if (health.GetCurrentHealth() <= mediumFormStats.MaxHealth && health.GetCurrentHealth() > smallFormStats.MaxHealth)
            {
                SwitchForm(PlayerForm.Big);
                health.SetMaxHealth(bigFormStats.MaxHealth);
                health.SetCurrentHealth(bigFormStats.CurrentHealth);

            }
            Destroy(collision.collider.gameObject);

        }
        if (collision.collider.CompareTag("End"))
        {
            StartCoroutine(completeGame());
        }
    }

    private IEnumerator completeGame()
    {
        Debug.Log("end");

        float timeBonus = CalculateTimeBonus();
        float formPoints = CalculateFormPoints();

        finalPoints = timeBonus + formPoints;

        // Display the final points or use them as needed
        Debug.Log("Final Points: " + finalPoints);

        Input.ResetInputAxes();
        // Additional logic or UI display if needed
        yield return new WaitForSeconds(5f);
        youWon = true;
    }

    private float CalculateTimeBonus()
    {
        // Assuming you want to give more points for a faster completion time
        float maxTime = 600f;  // Adjust this to your desired maximum time
        float timeBonus = Mathf.Clamp(maxTime - timer, 0f, maxTime) * 0.1f;  // Adjust multiplier as needed

        return timeBonus;
    }

    private float CalculateFormPoints()
    {
        // Get points based on the current form
        float formPoints = 0f;

        switch (currentForm)
        {
            case PlayerForm.Big:
                formPoints = 50f;  // Adjust points for big form
                break;
            case PlayerForm.Medium:
                formPoints = 30f;  // Adjust points for medium form
                break;
            case PlayerForm.Small:
                formPoints = 10f;  // Adjust points for small form
                break;
        }

        return formPoints;
    }
    private void Update()
    {
        stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        timer += Time.deltaTime;
        // Check if the player is grounded
        isGrounded = Physics2D.OverlapCircle(transform.position, 0.8f, groundLayer);
        isOnPlatform = Physics2D.OverlapCircle(transform.position, 0.8f, platformLayer);
        isWall = Physics2D.OverlapCircle(transform.position, 0.8f, wallLayer);
        // Handle player input
        horizontalInput = Input.GetAxis("Horizontal");
        FlipPlayer(horizontalInput);
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
            if (isGrounded && Input.GetButtonDown("Jump") || isOnPlatform && Input.GetButtonDown("Jump"))
            {
                Jump();
            }

            if (isWall && Input.GetButtonDown("Jump") && !hasWallJumped)
            {
                WallJump();
                hasWallJumped = true; // Set to true when wall jumping
            }

            if (isGrounded || isOnPlatform)
            {
                isJumping = false;
                hasWallJumped = false; // Reset wall jump when grounded or on a platform
            }


            else if (!isGrounded && !isDashing && !isSliding && !isAttacking && !isDying && !isOnPlatform && !isTakingDamage)
            {
                if (!stateInfo.IsName("Dash"))
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
                Slide(horizontalInput);
            }
        }
        if (Input.GetButtonDown("Fire1") && canAttack)
        {
            MeleeAttack();
        }

        if (Input.GetButtonDown("Fire2") && canShoot)
        {
            RangedAttack();
        }
    }

    private void WallJump()
    {
        soundManager.PlaySoundEffect(jumpSound, 0.7f);
        isJumping = true;
        rb.velocity = new Vector2(rb.velocity.x, baseStats.jumpForce);
    }
    private void Move(float horizontalInput)
    {
        if (IsGroundedOnSides())
        {
            movement = new Vector2(horizontalInput * baseStats.Speed, rb.velocity.y);
            rb.velocity = movement;
            if ((isGrounded || isOnPlatform) && !isJumping && !isSliding && !isAttacking && !isDying && !isTakingDamage)
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
        soundManager.PlaySoundEffect(jumpSound, 0.7f);
        isJumping = true;
        if (isSliding)
        {
            Debug.Log("big jump");
            rb.velocity = new Vector2(rb.velocity.x, baseStats.jumpForce * 1.5f);
            canDash = true;
        }
        else
        {
            rb.velocity = new Vector2(rb.velocity.x, baseStats.jumpForce);
        }
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
        soundManager.PlaySoundEffect(dashSound, 0.5f);

        // Wait for the dash duration
        yield return new WaitForSeconds(dashDuration);



        // Stop the dash and prevent upward movement
        isDashing = false;
        if (isGrounded || isOnPlatform)
        {
            isSliding = true;
        }


        // Set cooldown
        yield return new WaitForSeconds(baseStats.dashCooldown);
        if (canDash == false)
        {
            canDash = true;
        }

    }

    private void Slide(float horizontalInput)
    {
        if (isGrounded || isOnPlatform)
        {

            // Gradually reduce velocity during the sliding state
            rb.velocity = new Vector2(Mathf.Lerp(horizontalInput * (baseStats.dashSpeed), 0f, slideTimer / slideDuration), rb.velocity.y);

            // Update the sliding timer
            slideTimer += Time.deltaTime;

            if (slideTimer < slideDuration)
            {
                animator.Play("SlideStart");
            }
            else
            {
                // After the slide duration, play "SlideEnd" animation and enter the "getting up" period
                animator.Play("SlideEnd");

                // Delay for the "getting up" period
                float gettingUpDelay = animator.GetCurrentAnimatorClipInfo(0)[0].clip.length;
                Debug.Log(gettingUpDelay);

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
        Vector2 top = new Vector2(bounds.center.x, bounds.max.y);
        Vector2 middle = new Vector2(bounds.center.x, bounds.center.y);
        Vector2 bottom = new Vector2(bounds.center.x, bounds.min.y);

        // Calculate the direction based on the player's scale (facing direction)
        float direction = transform.localScale.x < 0 ? 1f : -1f;

        // Check if the player is not in contact with the ground on the sides
        RaycastHit2D hitTop = Physics2D.Raycast(top, Vector2.right * direction, groundCheckDistance, groundLayer);
        RaycastHit2D hitMiddle = Physics2D.Raycast(middle, Vector2.right * direction, groundCheckDistance, groundLayer);
        RaycastHit2D hitBottom = Physics2D.Raycast(bottom, Vector2.right * direction, groundCheckDistance, groundLayer);

        RaycastHit2D hitTop2 = Physics2D.Raycast(top, Vector2.right * direction, groundCheckDistance, wallLayer);
        RaycastHit2D hitMiddle2 = Physics2D.Raycast(middle, Vector2.right * direction, groundCheckDistance, wallLayer);
        RaycastHit2D hitBottom2 = Physics2D.Raycast(bottom, Vector2.right * direction, groundCheckDistance, wallLayer);

        RaycastHit2D hitTop3 = Physics2D.Raycast(top, Vector2.right * direction, groundCheckDistance, platformLayer);
        RaycastHit2D hitMiddle3 = Physics2D.Raycast(middle, Vector2.right * direction, groundCheckDistance, platformLayer);
        RaycastHit2D hitBottom3 = Physics2D.Raycast(bottom, Vector2.right * direction, groundCheckDistance, platformLayer);

        // Visualize the raycasts
        Debug.DrawLine(top, top + Vector2.right * direction * groundCheckDistance, Color.red);
        Debug.DrawLine(middle, middle + Vector2.right * direction * groundCheckDistance, Color.green);
        Debug.DrawLine(bottom, bottom + Vector2.right * direction * groundCheckDistance, Color.blue);

        return (hitTop.collider == null) && (hitMiddle.collider == null) && (hitBottom.collider == null) &&
            (hitTop2.collider == null) && (hitMiddle2.collider == null) && (hitBottom2.collider == null) &&
            (hitTop3.collider == null) && (hitMiddle3.collider == null) && (hitBottom3.collider == null);
    }



    public void TakeDamage(float damage)
    {
        if (!isTakingDamage)
        {
            health.ReceiveDamage(damage);
            soundManager.PlaySoundEffect(takeDamageSound);
            animator.Play("TakeDamage");

            isTakingDamage = true;

            // Check for form switch based on health
            if (health.GetCurrentHealth() <= 0f)
            {
                StartCoroutine(DestroyAfterAnimation(gameObject));
            }
            else if (health.GetCurrentHealth() <= smallFormStats.MaxHealth && currentForm != PlayerForm.Small)
            {
                SwitchForm(PlayerForm.Small);
                health.SetMaxHealth(smallFormStats.MaxHealth);

            }
            else if (health.GetCurrentHealth() <= mediumFormStats.MaxHealth && health.GetCurrentHealth() > smallFormStats.MaxHealth && currentForm != PlayerForm.Medium)
            {
                SwitchForm(PlayerForm.Medium);
                health.SetMaxHealth(mediumFormStats.MaxHealth);
            }

            StartCoroutine(ResetIsTakingDamageAfterAnimation());
        }
    }

    private IEnumerator ResetIsTakingDamageAfterAnimation()
    {
        // Get the length of the TakeDamage animation
        float animationLength = animator.GetCurrentAnimatorClipInfo(0)[0].clip.length;

        // Wait for the animation to finish
        yield return new WaitForSeconds(animationLength);

        // Reset the flag back to false
        isTakingDamage = false;
    }

    public void ReloadScene()
    {

        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        // Reload the current scene
        SceneManager.LoadScene(currentSceneIndex);
    }

    private IEnumerator DestroyAfterAnimation(GameObject death)
    {
        isDying = true;
        Input.ResetInputAxes();
        // Play the specified animation
        animator.Play("Death");
        soundManager.PlaySoundEffect(deathSound);


        // Get the length of the specified animation
        float animationLength = animator.GetCurrentAnimatorClipInfo(0)[0].clip.length;

        // Wait for the animation to finish
        yield return new WaitForSeconds(animationLength + 4f);

        // Destroy the GameObject
       // ReloadScene();
    }

    private void MeleeAttack()
    {

        float animationLength = animator.GetCurrentAnimatorClipInfo(0)[0].clip.length;
        isAttacking = true;
        animator.Play("ThrowAttack");
        StartCoroutine(ResetIsAttackingAfterAnimationHit(animationLength + 0.1f));

        StartCoroutine(MeleeAttackCooldown());


    }
    private IEnumerator ResetIsAttackingAfterAnimationHit(float delay)
    {
        yield return new WaitForSeconds(delay);
        Vector2 chasingDirection = new Vector2(horizontalInput, 0.0f);
        Vector2 position = transform.position;
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(position + chasingDirection * meleePointOffset, attackRadius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy"))
            {
                enemy = hitCollider.GetComponent<Enemy>();
                enemy.TakeDamage(baseStats.Attack);

            }
        }

        isAttacking = false;
    }
    private IEnumerator MeleeAttackCooldown()
    {
        canAttack = false;

        // Wait for the attack speed duration
        yield return new WaitForSeconds(1.0f / baseStats.attackSpeed);

        // Reset the attack cooldown
        canAttack = true;
    }
    private void OnDrawGizmos()
    {
        // Draw the attack radius in the Scene view when selected
        Gizmos.color = Color.red;
        Vector2 chasingDirection = new Vector2(horizontalInput, 0.0f);
        Vector2 position = transform.position;
        Gizmos.DrawWireSphere(position + chasingDirection * meleePointOffset, attackRadius);
    }

    private void RangedAttack()
    {
        float animationLength = animator.GetCurrentAnimatorClipInfo(0)[0].clip.length;

        isAttacking = true;
        animator.Play("ThrowAttack");
        StartCoroutine(ResetIsAttackingAfterAnimationHitRanged(animationLength + 0.1f));

        StartCoroutine(RangedAttackCooldown());
    }

    private IEnumerator RangedAttackCooldown()
    {
        canShoot = false;

        // Wait for the attack speed duration
        yield return new WaitForSeconds(1.0f / baseStats.attackSpeed);

        // Reset the attack cooldown
        canShoot = true;
    }

    private IEnumerator ResetIsAttackingAfterAnimationHitRanged(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Instantiate a bullet
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        soundManager.PlaySoundEffect(throwAttackSound);

        // Get the bullet component and set its damage
        Bullet bulletComponent = bullet.GetComponent<Bullet>();
        if (bulletComponent != null)
        {
            bulletComponent.SetDamage(baseStats.Attack * 2f);

            // Calculate the direction the player is facing
            Vector2 direction = transform.localScale.x < 0 ? Vector2.right : Vector2.left;

            // Apply force to the bullet in the calculated direction
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            if (bulletRb != null)
            {
                bulletRb.AddForce(direction * 30, ForceMode2D.Impulse);
            }
        }

        isAttacking = false;
        this.TakeDamage(40);

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
                animator.runtimeAnimatorController = bigFormAnimator.runtimeAnimatorController;
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

    private void FlipPlayer(float horizontalInput)
    {
        if (horizontalInput < 0)
        {
            // Face left
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (horizontalInput > 0)
        {
            // Face right
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

}
