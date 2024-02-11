using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public BaseStatsContainer stats;
    public GameObject player;
    private Player playerReference;
    protected Rigidbody2D rb;
    protected Animator animator;
    protected bool canMove;
    protected bool inRange;
    public Vector2 chasingDirection;
    public Vector2 canDealMeleeDamage;
    public float stopDistance = 0.1f;
    private LayerMask groundLayer;
    private LayerMask platformLayer;
    bool onGround = true;
    protected bool isInRange = false;
    protected bool canMelee = true;
    protected bool isAttacking = false;
    protected bool isRunning = false;
    bool canJump = true;
    [SerializeField]
    private float meleePointOffset = 1.0f;
    [SerializeField]
    private float meleeCirlceRadius = 1.0f;
    [SerializeField]
    private float meleeCooldown = 5f;
    private float jumpCooldown = 0.3f;
    [SerializeField]
    private bool hasCreatedProjectile = false;
    float playerHealthCache;
    private int hitNumber;
    public GameObject projectile;
    private bool haveProjectile = false;
    public bool runAway;
    private bool isChasing = true;
    private HealthComponent healthComponent;
    [SerializeField]
    private Color gizmosColor = Color.red;  // Color for the Gizmos sphere
    private Vector3 spawnPos;
    public EnemySpawn spawn;

    private Transform newLookAtTarget;
    public CinemachineVirtualCamera virtualCamera;

    private SoundManger soundManager;
    public AudioClip takeDamageSound; // Assign your sound effect in the Unity Editor or through code

    private void OnDrawGizmosSelected()
    {
        // Draw a wire sphere to represent the attack range
        Gizmos.color = gizmosColor;
        Gizmos.DrawWireSphere(transform.position, meleeCirlceRadius * 3);
        Gizmos.DrawWireCube(GetComponent<Collider2D>().bounds.center, GetComponent<Collider2D>().bounds.size);
    }
    protected virtual void Awake()
    {
        soundManager = GetComponent<SoundManger>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        healthComponent = GetComponent<HealthComponent>();
        groundLayer = LayerMask.GetMask("Ground");
        platformLayer = LayerMask.GetMask("Platform");
        stats.CurrentHealth = stats.MaxHealth;
        spawnPos = transform.position;
    }

    private void Update()
    {
        IsGroundedOnSides();
        onGround = Physics2D.OverlapCircle(transform.position, 0.7f, groundLayer | platformLayer);
        if (onGround && isChasing)
        {
            MoveToPlayer();

        }
        if (!onGround && isChasing && !canJump)
        {
            SlightAerialMovement();
        }
        if (canJump)
        {
            JumpToPlayer();

        }

        if (canMelee)
        {
            Melee();
        }

        if (runAway)
        {
            RunFromPlayer();
        }
    }

    protected void MoveToPlayer()
    {
        if (IsGroundedOnSides())
        {
            if (!isAttacking)
            {
                animator.Play("BugRun");
                Vector2 direction = (player.transform.position - transform.position).normalized;

                // Set only the horizontal component of the chasingDirection
                chasingDirection = new Vector2(direction.x, 0f);

                // Set the horizontal velocity
                rb.velocity = chasingDirection * stats.Speed;


                // Flip the enemy based on its velocity
                FlipEnemy();
                Debug.Log("Chasing");
                Debug.Log(transform.position);

            }
            float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
            if (distanceToPlayer <= stopDistance)
            {
                // Stop horizontal movement if the enemy is close enough to the player
                rb.velocity = new Vector2(0f, rb.velocity.y);
                Debug.Log("Chasing2");
            }
        }
        else 
        {
            rb.velocity = new Vector2(0.3f, 2.0f);
        }
    }
    protected void SlightAerialMovement()
    {
        Vector2 direction = (player.transform.position - transform.position).normalized;

        // Set only the horizontal component of the chasingDirection
        chasingDirection = new Vector2(direction.x, 0f);

        // Set the horizontal velocity
        rb.velocity += chasingDirection * 0.8f;


        // Flip the enemy based on its velocity
        FlipEnemy();
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
        RaycastHit2D hitTop = Physics2D.Raycast(top, Vector2.right * direction, 0.7f, groundLayer);
        RaycastHit2D hitMiddle = Physics2D.Raycast(middle, Vector2.right * direction, 0.7f, groundLayer);
        RaycastHit2D hitBottom = Physics2D.Raycast(bottom, Vector2.right * direction, 0.7f, groundLayer);

        // Visualize the raycasts
        Debug.DrawLine(top, top + Vector2.right * direction * 0.7f, Color.red);
        Debug.DrawLine(middle, middle + Vector2.right * direction * 0.7f, Color.green);
        Debug.DrawLine(bottom, bottom + Vector2.right * direction * 0.7f, Color.blue);
   
        return (hitTop.collider == null) && (hitMiddle.collider == null) && (hitBottom.collider == null);
    }

    protected void OnDeath(GameObject death)
    {
        Destroy(death);
    }

    public virtual void DisableEnemy()
    {
        canMove = false;
    }

    public virtual void EnableEnemy()
    {
        canMove = true;
    }

    public void Melee()
    {
        if (!isRunning)
        {
            Vector2 position = transform.position;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(position + chasingDirection * meleePointOffset, meleeCirlceRadius);

            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].CompareTag("Player"))
                {
                    playerReference = colliders[i].GetComponent<Player>();


                    // Deal damage to the player
                    canMelee = false;
                    isAttacking = true;
                    animator.Play("BugAttack");

                    // Get the length of the BugAttack animation
                    float animationLength = animator.GetCurrentAnimatorClipInfo(0)[0].clip.length;

                    // Start a coroutine to reset isAttacking after the animation length
                    StartCoroutine(ResetIsAttackingAfterAnimation(animationLength + 0.3f));

                    StartCoroutine(MeleeCooldown());

                    // Check if the player's form has changed

                }
            }
        }
    }
    private IEnumerator ResetIsAttackingAfterAnimation(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Check if the player is still within the melee circle radius
        float distanceToPlayer = Vector2.Distance(transform.position, playerReference.transform.position);
        if (distanceToPlayer <= meleeCirlceRadius * 3)
        {
            playerHealthCache = playerReference.health.GetMaxHealth();
            // Deal damage to the player
            playerReference.TakeDamage(stats.Attack);
            if (playerReference.health.GetMaxHealth() != playerHealthCache)
            {
                // Player's form has changed, so the enemy should run away
                runAway = true;
                isChasing = false;
            }
            else if (playerReference.health.GetCurrentHealth() <= 0)
            {
                runAway = true;
                isChasing = false;
                StartCoroutine(WaitForDeath());
            }
        }
   
        isAttacking = false;
    }

    public IEnumerator WaitForDeath()
    {
        yield return new WaitForSeconds(1.5f);
        virtualCamera.Follow = transform;
    }
    public IEnumerator MeleeCooldown()
    {
        yield return new WaitForSeconds(meleeCooldown);
        canMelee = true;
    }

    public void TakeDamage(float damage)
    {
        healthComponent.ReceiveDamage(damage);
        soundManager.PlaySoundEffect(takeDamageSound);
        Debug.Log(healthComponent.GetCurrentHealth());
        if (healthComponent.GetCurrentHealth() <= 0f && !hasCreatedProjectile)
        {
            OnDeath(gameObject);
            if (haveProjectile)
            {
                CreateProjectileOnDeath();
                hasCreatedProjectile = true;
            }
        }
    }

    private void JumpToPlayer()
    {

        if (player.transform.position.y > (transform.position.y + 2) && onGround)
        {
            Vector2 jumpDirection = (player.transform.position - transform.position).normalized;
            rb.AddForce(jumpDirection * stats.jumpForce, ForceMode2D.Impulse);
            canJump = false;
            StartCoroutine(JumpCooldown());
        }
    }

    private IEnumerator JumpCooldown()
    {
        yield return new WaitForSeconds(jumpCooldown);
        canJump = true;
    }


    private void CreateProjectileOnDeath()
    {
        GameObject bullet = Instantiate(projectile, transform.position, Quaternion.identity);
        bullet.GetComponent<Rigidbody2D>().position = transform.position;
    }

    private void RunFromPlayer()
    {
        animator.Play("BugRunBread");
        haveProjectile = true;
        isRunning = true;
        // Calculate the opposite direction from the player
        Vector2 direction = (spawnPos - transform.position).normalized;

        // Set the velocity to move only horizontally away from the player
        float clampedXVelocity = Mathf.Clamp(direction.x * stats.Speed * 2, -stats.Speed * 2, stats.Speed * 2);
        float clampedYVelocity = Mathf.Clamp(rb.velocity.y, -stats.Speed * 2, stats.Speed * 2);
        Vector2 newVelocity = new Vector2(clampedXVelocity, clampedYVelocity);

        // Set the velocity to move only horizontally away from the player
        rb.velocity = newVelocity;
    }

    private void FlipEnemy()
    {
        if (rb.velocity.x < 0)
        {
            // Flip the sprite when moving right
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (rb.velocity.x > 0)
        {
            // Flip the sprite when moving left
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }
}
