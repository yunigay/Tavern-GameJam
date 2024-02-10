using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public BaseStatsContainer stats;
    public GameObject player;
    protected Rigidbody2D rb;
    protected Animator animator;
    protected bool canMove;
    protected bool inRange;
    public Vector2 chasingDirection;
    public Vector2 canDealMeleeDamage;
    public float stopDistance = 0.1f;
    private LayerMask groundLayer;
    bool onGround;
    protected float health;
    protected bool isInRange = false;
    protected bool canMelee = true;
    [SerializeField]
    private float meleePointOffset = 1.0f;
    [SerializeField]
    private float meleeCirlceRadius = 1.0f;
    [SerializeField]
    private float meleeCooldown = 15f;
    [SerializeField]
    private float meleeDamage = 1.0f;


    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        groundLayer = LayerMask.GetMask("Ground");
        stats.CurrentHealth = stats.MaxHealth;
  
        
    }

    protected virtual void Update()
    {
        onGround = Physics2D.OverlapCircle(transform.position, 0.8f, groundLayer);
    
        if (onGround)
        {
        }
            MoveToPlayer();

        if (canMelee )
        {
            Melee();
        }
    }

        protected void MoveToPlayer()
    {
        Vector2 direction = (player.transform.position - transform.position).normalized;
        chasingDirection = direction;
        rb.velocity = chasingDirection * stats.Speed;
        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
        if (distanceToPlayer <= stopDistance)
        {
            rb.velocity = Vector2.zero;
            return;
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
        if (canMelee)
        {
            Vector2 position = transform.position;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(position + chasingDirection * meleePointOffset, meleeCirlceRadius);

            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].CompareTag("Player"))
                {
                    Player pHealth = colliders[i].GetComponent<Player>();
                    if (pHealth != null)
                    {
                        pHealth.TakeDamage(meleeDamage);
                        Debug.Log(pHealth.baseStats.CurrentHealth);
                        canMelee = false;
                    }
                }
            }

            StartCoroutine(MeleeCooldown());
        }
    }


    public IEnumerator MeleeCooldown()
    {
        yield return new WaitForSeconds(meleeCooldown);
        canMelee = true;
    }

    public void TakeDamage(float damage)
    {
        stats.CurrentHealth -= damage;
        if (stats.CurrentHealth <= 0f)
        {
            OnDeath(gameObject);
        }
    }
}
  

