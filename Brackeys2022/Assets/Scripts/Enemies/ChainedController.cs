using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChainedController : EnemyController
{

    [Header("General Components")]
    [SerializeField] private SpriteRenderer sRend;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator boneAnim;
    [SerializeField] private Animator fireAnim;


    [Header("Combat Fields")]
    [SerializeField] private Transform attackRange;
    [SerializeField] private BoxCollider2D detectRange;
    [SerializeField] private float health = 10;
    [SerializeField] private float attackPower = 3;
    [SerializeField] private float attackRate = 2;

    private static PlayerController player;
    private bool facingLeft;
    private float timer;
    private SkeletonState state;

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        state = SkeletonState.Idle;
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
            case SkeletonState.Idle:
                break;
            case SkeletonState.Angry:
                FlipCheck();
                AttackCheck();
                break;
            case SkeletonState.Attacking:
                break;
            case SkeletonState.Hit:
                break;
            case SkeletonState.Die:
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (state == SkeletonState.Die)
                return;
            state = SkeletonState.Angry;
            SetAnimation();
        }

        if (collision.CompareTag("Dive"))
        {
            if (state == (SkeletonState.Die | SkeletonState.Hit))
                return;
            TakeDamage(3, collision.transform.position);
}
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (state == SkeletonState.Die)
                return;
            state = SkeletonState.Idle;
            SetAnimation();
        }
    }

    private void DetectPlayer()
    {
        if (state == SkeletonState.Die)
            return;
        if (detectRange.IsTouchingLayers(128))
        {
            state = SkeletonState.Angry;
            SetAnimation();
        }
    }
    private void AttackCheck()
    {
        if (timer >= attackRate)
        {
            timer = 0;
            state = SkeletonState.Attacking;
            SetAnimation();
        }
        else
            timer += Time.deltaTime;
    } 

    public void HitCheck()
    {
        if(Physics2D.OverlapBox(attackRange.position, attackRange.localScale, 0, 128) != null)
            player.DamageCheck(attackRange);
    }

    public override void TakeDamage(float damageDealt, Vector2 playerPosition)
    {
        state = SkeletonState.Hit;
        health -= damageDealt;
        timer = 0;
        if(health <= 0)
            state = SkeletonState.Die;
        SetAnimation();
    }
    
    public void AnimationEnd()
    {
        state = SkeletonState.Idle;
        if (detectRange.IsTouchingLayers(128))
            state = SkeletonState.Angry;
        SetAnimation();
    }

    /// <summary>Sets the animation paramters based on the skeleton state</summary>
    private void SetAnimation()
    {
        switch (state)
        {
            case SkeletonState.Idle:
                boneAnim.SetBool("isAngry", false);
                fireAnim.SetBool("fireOn", false);
                break;
            case SkeletonState.Angry:
                boneAnim.SetBool("isAngry", true);
                fireAnim.SetBool("fireOn", true);
                break;
            case SkeletonState.Attacking:
                boneAnim.SetTrigger("Attack");
                fireAnim.SetBool("fireOn", true);
                break;
            case SkeletonState.Hit:
                boneAnim.SetTrigger("Hit");
                fireAnim.SetBool("fireOn", true);
                break;
            case SkeletonState.Die:
                boneAnim.SetTrigger("Die");
                fireAnim.SetBool("fireOn", false);
                break;
        }
    }

    /// <summary>Destorys the game objects once the death animation is complete</summary>
    public void Die()
    {
        Destroy(gameObject);
    }

    /// <summary>Checks if it's facing right direction</summary>
    private void FlipCheck()
    {
        if (player.hidden)
        {
            state = SkeletonState.Idle;
            SetAnimation();
            return;
        }
            
        if (facingLeft)
        {
            if (player.transform.position.x > transform.position.x)
            {
                facingLeft = false;
                transform.localScale = new Vector3(-1, 1, 1);
            }
        }
        else
            if (player.transform.position.x < transform.position.x)
        {
            facingLeft = true;
            transform.localScale = new Vector3(1, 1, 1);
        }
    }

    /// <summary> Allows the editor to show the transform points </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(attackRange.position, attackRange.localScale);
    }
}

public enum SkeletonState
{
    Idle,
    Angry,
    Attacking,
    Hit,
    Die
}