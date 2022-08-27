using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonController : MonoBehaviour
{

    [Header("General Components")]
    [SerializeField] private SpriteRenderer sRend;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator anim;


    [Header("Movement Fields")]
    private bool facingLeft;
    private Vector3 targetWaypoint;


    [Header("Combat Fields")]
    [SerializeField] private Transform attackRange;
    [SerializeField] private BoxCollider2D detectRange;
    [SerializeField] private float knockBackForce = 5;
    [SerializeField] private float health = 10;
    [SerializeField] private float attackPower = 3;
    private static PlayerController player;

    private float timer;
    private EnemyState state;

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        state = EnemyState.Idle;
        timer = 0;
        facingLeft = false;
    }

    private void OnEnable()
    {
        PlayerController.OnHideEnd += DetectPlayer;
    }

    private void OnDisable()
    {
        PlayerController.OnHideEnd -= DetectPlayer;
    }

    void Update()
    {
        switch (state)
        {
            case EnemyState.Idle:
                break;
            case EnemyState.Attack:
                break;
            case EnemyState.Hit:
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (state == EnemyState.Die)
                return;
            state = EnemyState.Chasing;
            SetAnimation();
        }

        if (collision.CompareTag("Dive"))
        {
            if (state == (EnemyState.Die | EnemyState.Hit))
                return;
            TakeDamage(3, collision.transform.position);
}
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (state == EnemyState.Die)
                return;
            state = EnemyState.Idle;
            SetAnimation();
            FlipCheck();
        }
    }

    private void DetectPlayer()
    {
        if (state == EnemyState.Die)
            return;
        if (detectRange.IsTouchingLayers(128))
        {
            state = EnemyState.Chasing;
            SetAnimation();
        }
    }
    private void StartAttack()
    {
        state = EnemyState.Attack;
        SetAnimation();
        rb.velocity = new Vector2(0, rb.velocity.y);
    } 

    public void HitCheck()
    {
        if(Physics2D.OverlapBox(attackRange.position, attackRange.localScale, 0, 128) != null)
            player.DamageCheck(attackRange, attackPower);
    }

    public void TakeDamage(float damageDealt, Vector2 playerPosition)
    {
        state = EnemyState.Hit;
        rb.velocity = Vector2.zero;
        rb.AddForce(((Vector2)transform.position - playerPosition).normalized * knockBackForce, ForceMode2D.Impulse);
        health -= damageDealt;
        timer = 0;
        if(health <= 0)
            state = EnemyState.Die;
        SetAnimation();
    }
    
    public void AnimationEnd()
    {
        state = EnemyState.Idle;
        if (detectRange.IsTouchingLayers(128))
            state = EnemyState.Chasing;
        SetAnimation();
    }

    private void SetAnimation()
    {
        switch (state)
        {
            case EnemyState.Idle:
                anim.SetInteger("State", 0);
                break;
            case EnemyState.Walking:
                anim.SetInteger("State", 1);
                break;
            case EnemyState.Chasing:
                anim.SetInteger("State", 1);
                break;
            case EnemyState.Hit:
                anim.SetInteger("State", 2);
                break;
            case EnemyState.Die:
                anim.SetInteger("State", 4);
                break;
            case EnemyState.Attack:
                anim.SetInteger("State", 5);
                break;
        }
    }

    public void Die()
    {
        Destroy(gameObject);
    }



    /// <summary>Checks if it's facing right direction</summary>
    private void FlipCheck()
    {
        if (facingLeft)
        {
            if (targetWaypoint.x > transform.position.x)
            {
                facingLeft = sRend.flipX = false;
                attackRange.localPosition = Vector2.right;
            }
        }
        else
            if (targetWaypoint.x < transform.position.x)
        {
            facingLeft = sRend.flipX = true;
            attackRange.localPosition = Vector2.left;
        }
    }

    /// <summary> Allows the editor to show the transform points </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(attackRange.position, attackRange.localScale);
    }
}