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
    public float stopDistance = 1f;
    private LayerMask groundLayer;
    bool onGround;
    protected float health;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        groundLayer = LayerMask.GetMask("Ground");
        stats.CurrentHealth = health;
        
    }

    protected virtual void Update()
    {
        onGround = Physics2D.OverlapCircle(transform.position, 0.8f, groundLayer);
    
        if (onGround)
        {
        }
            MoveToPlayer();
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
}
