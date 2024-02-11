using System.Collections;
using System.Collections.Generic;
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
    protected float health;
    protected bool isInRange = false;
    protected bool canMelee = true;
    protected bool isAttacking = false;
    bool canJump = true;
    [SerializeField]
    private float meleePointOffset = 1.0f;
    [SerializeField]
    private float meleeCirlceRadius = 1.0f;
    [SerializeField]
    private float meleeCooldown = 5f;
    private float jumpCooldown = 5f;
    [SerializeField]
    private bool hasCreatedProjectile = false;

    private int hitNumber;
    public GameObject projectile;
    private bool haveProjectile = false;
    private bool runAway;
    private bool isChasing = true;
    private HealthComponent healthComponent;
    AnimatorStateInfo stateInfo;
    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        healthComponent = GetComponent<HealthComponent>();
        groundLayer = LayerMask.GetMask("Ground");
        platformLayer = LayerMask.GetMask("Platform");
        stats.CurrentHealth = stats.MaxHealth;
    }

    protected virtual void Update()
    {
        Debug.Log(isAttacking);

        stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        onGround = Physics2D.OverlapCircle(transform.position, 0.7f, groundLayer | platformLayer);
        if (onGround && isChasing)
        {
            MoveToPlayer();
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
        if(!isAttacking)
        {
                animator.Play("BugRun");
            Vector2 direction = (player.transform.position - transform.position).normalized;

            // Set only the horizontal component of the chasingDirection
            chasingDirection = new Vector2(direction.x, 0f);

            // Set the horizontal velocity
            rb.velocity = chasingDirection * stats.Speed;

            RaycastHit2D hit = Physics2D.Raycast(transform.position, chasingDirection, 3f, groundLayer);

            // Flip the enemy based on its velocity
            FlipEnemy();

            float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
            if (distanceToPlayer <= stopDistance)
            {
                // Stop horizontal movement if the enemy is close enough to the player
                rb.velocity = new Vector2(0f, rb.velocity.y);
            }
            if (hitNumber == 3)
            {
                runAway = true;
                isChasing = false;
            }
        }
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
        Vector2 position = transform.position;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position + chasingDirection * meleePointOffset, meleeCirlceRadius);

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].CompareTag("Player"))
            {
                playerReference = colliders[i].GetComponent<Player>();
                playerReference.TakeDamage(stats.Attack);
                    canMelee = false;
                    hitNumber++;
                isAttacking = true;
                animator.Play("BugAttack");

                // Get the length of the BugAttack animation
                float animationLength = animator.GetCurrentAnimatorClipInfo(0)[0].clip.length;
                Debug.Log(animationLength);
                // Start a coroutine to reset isAttacking after the animation length
                StartCoroutine(ResetIsAttackingAfterAnimation(animationLength + 0.3f));

                StartCoroutine(MeleeCooldown());

            }
        }
    }
    private IEnumerator ResetIsAttackingAfterAnimation(float delay)
    {
        yield return new WaitForSeconds(delay);
        isAttacking = false;
    }

    public IEnumerator MeleeCooldown()
    {
        yield return new WaitForSeconds(meleeCooldown);
        canMelee = true;
    }

    public void TakeDamage(float damage)
    {
        healthComponent.ReceiveDamage(damage);
        //if (stats.CurrentHealth <= 0f && !hasCreatedProjectile)
        //{
        //    OnDeath(gameObject);
        //    if (haveProjectile)
        //    {
        //        CreateProjectileOnDeath();
        //        hasCreatedProjectile = true;
        //    }
        //}
    }

    private void JumpToPlayer()
    {
        if (player.transform.position.y > (transform.position.y + 2) && onGround)
        {
            Vector2 jumpDirection = (player.transform.position - transform.position).normalized;
            rb.AddForce(jumpDirection * stats.jumpForce, ForceMode2D.Impulse);
            canJump = false;
            StartCoroutine(JumpCooldown());
            Debug.Log("Jumping");
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

        // Calculate the opposite direction from the player
        Vector2 oppositeDirection = -(player.transform.position - transform.position).normalized;

        Vector2 runningDirection = new Vector2(oppositeDirection.x, 0);
        // Set the velocity to move away from the player
        rb.velocity = runningDirection * stats.Speed;

        if (rb.velocity.x < 0)
        {
            // Flip the sprite when moving left
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (rb.velocity.x > 0)
        {
            // Flip the sprite back when moving right
            transform.localScale = new Vector3(1, 1, 1);
        }
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
