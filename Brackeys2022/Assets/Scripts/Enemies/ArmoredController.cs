using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmoredController : MonoBehaviour
{

    [Header("General Components")]
    [SerializeField] private SpriteRenderer sRend;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator anim;


    [Header("Movement Fields")]
    [SerializeField] private Vector2[] points;
    private int pointIndex = 0;
    private Vector2 targetWaypoint;
    [SerializeField] private float speed = 3;
    [SerializeField] private float idleTime = 1;
    private bool facingLeft;


    [Header("Combat Fields")]
    [SerializeField] private Transform attackRange;
    [SerializeField] private BoxCollider2D detectRange;
    [SerializeField] private float knockBackForce = 5;
    [SerializeField] private float health = 10;
    [SerializeField] private float attackPower = 3;
    private static PlayerController player;

    private float timer;
    private EnemyState state;

    [Header("SFX")]
    [SerializeField] private AK.Wwise.Event getHitSound;

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        state = EnemyState.Idle;
        timer = 0;
        targetWaypoint = points[pointIndex];
        facingLeft = false;
        anim.SetBool("Left", false);
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
                Idling();
                break;
            case EnemyState.Walking:
                Walking();
                break;
            case EnemyState.Chasing:
                Chasing();
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
            if (state == ( EnemyState.Die | EnemyState.Hit))
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
            targetWaypoint = points[pointIndex];
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
        AkSoundEngine.PostEvent(getHitSound.Id, this.gameObject);
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


    /// <summary>Move towards the player</summary>
    private void Chasing()
    {
        if (player.hidden)
        {
            state = EnemyState.Idle;
            SetAnimation();
            targetWaypoint = points[pointIndex];
            FlipCheck();
            return;
        }
        targetWaypoint = player.transform.position;
        FlipCheck();
        if (Physics2D.OverlapBox(attackRange.position, attackRange.localScale, 0, 128) != null)
            StartAttack();
        SetAnimation();

    }

    /// <summary>Checks whether the walk is over</summary>
    private void Walking()
    {
        //print(transform.position.x - targetWaypoint.x);
        if (facingLeft)
        {
            if (transform.position.x <= targetWaypoint.x)
            {
                pointIndex++;
                pointIndex = pointIndex % points.Length;
                targetWaypoint = points[pointIndex];
                state = EnemyState.Idle;
                rb.velocity = Vector2.zero;
                SetAnimation();
            }
        }
        else if (transform.position.x >= targetWaypoint.x)
        {
            pointIndex++;
            pointIndex = pointIndex % points.Length;
            targetWaypoint = points[pointIndex];
            state = EnemyState.Idle;
            rb.velocity = Vector2.zero;
            SetAnimation();
        }
    }
    /// <summary>Checks whether idling is over</summary>
    private void Idling()
    {
        timer += Time.deltaTime;
        if (timer >= idleTime)
        {
            state = EnemyState.Walking;
            FlipCheck();
            timer = 0;
            SetAnimation();
        }
    }

    /// <summary>Checks if it's facing right direction</summary>
    private void FlipCheck()
    {
        if (player.hidden)
        {
            state = EnemyState.Walking;
            SetAnimation();
            return;
        }

        if (facingLeft)
        {
            if (player.transform.position.x > transform.position.x)
            {
                facingLeft = false;
                anim.SetBool("Left", false);
                transform.localScale = new Vector3(-1, 1, 1);
            }
        }
        else if (player.transform.position.x < transform.position.x)
            {
                facingLeft = true;
                anim.SetBool("Left", true);
                transform.localScale = new Vector3(1, 1, 1);
            }

        if (facingLeft)
            rb.velocity = new Vector2(-1*speed, 0);
        else
            rb.velocity = new Vector2(speed, 0);
    }

    /// <summary> Allows the editor to show the transform points </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(new Vector2(points[0].x, transform.position.y), .2f);
        Gizmos.DrawSphere(new Vector2(points[1].x, transform.position.y), .2f);
        Gizmos.DrawLine(new Vector2(points[0].x, transform.position.y), new Vector2(points[1].x, transform.position.y));
        Gizmos.DrawWireCube(attackRange.position, attackRange.localScale);
    }
}
