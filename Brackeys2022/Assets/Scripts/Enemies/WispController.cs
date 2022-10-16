using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WispController : MonoBehaviour
{

    [Header("General Components")]
    [SerializeField] private SpriteRenderer sRend;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator anim;

    [Header("Movement Fields")]
    [SerializeField] private Vector2[] points;
    private int pointIndex = 0;
    private Vector2 targetWaypoint;
    [SerializeField] private int current = 0;
    [SerializeField] private float speed = 3;
    [SerializeField] private float idleTime = 1;
    private bool facingLeft = false;
    private bool playerDetected = false;


    [Header("Combat Fields")]
    [SerializeField] private BoxCollider2D detectRange;
    [SerializeField] private float knockBackForce = 5;
    [SerializeField] private float health = 10;
    [SerializeField] private float attackSpeed = 5;
    [SerializeField] private LayerMask playerMask;
    private PlayerController player;
    private Vector3 chargeDirection;
    private bool startKnockback = false;
    private bool timerEnded = false;
    private bool attackMissed;
    private float timer = 0;
    private WispState state;

    private void Awake()
    {
        player = FindObjectOfType<PlayerController>();
    }
    void Start()
    {
        rb.drag = 0.9f;
        targetWaypoint = points[pointIndex];
        state = WispState.Idle;
    }

    //private void OnEnable()
    //{
    //    PlayerController.OnHideEnd += CheckForPlayer;
    //}

    //private void OnDisable()
    //{
    //    PlayerController.OnHideEnd -= CheckForPlayer;
    //}

    void Update()
    {
        FlipCheck();
        CheckForPlayer();

        switch (state)
        {
            case WispState.Idle:
                Idling();
                break;
            case WispState.Patroling:
                Patrol();
                break;
            case WispState.Chasing:
                Chasing();
                break;
            case WispState.Charging:
                Charge();
                break;
            case WispState.Attack:
                Attack();
                break;
            case WispState.Dashing:
                CheckIfCollided();
                break;
            case WispState.Hit:
                KnockBack();
                break;
            case WispState.Cooldown:
                WaitForNextAttack();
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (state == WispState.Die)
                return;

            if(state == WispState.Dashing)
                startKnockback = true;
        }
    }

    private void CheckIfCollided()
    {
        StartTimer(1.5f);

        if (startKnockback)
        {
            attackMissed = false;
            state = WispState.Hit;
            ResetTimer();
            return;
        }

        if (timerEnded && !startKnockback)
        {
            if (!player.hidden)
            {
                attackMissed = true;
                ResetTimer();
                AttackEndAnimation();
                state = WispState.Cooldown;
            }
            else
            {
                ResetTimer();
                CalmDownAnimation();
                state = WispState.Patroling;
            }
        }
    }

    private void CheckForPlayer()
    {
        if (state == WispState.Die)
            return;

        if (detectRange.IsTouchingLayers(playerMask))
        {
            if (!player.hidden && !playerDetected)
            {
                playerDetected = true;
                AngryAnimation();
                state = WispState.Chasing;
            }
            else if (player.hidden && playerDetected)
            {
                playerDetected = false;
                CalmDownAnimation();
                state = WispState.Patroling;
            }
        }
    }

    public void TakeDamage(float damageDealt, Vector2 playerPosition)
    {
        if(state == WispState.Hit)
        {
            anim.SetTrigger("Hit");
            rb.velocity = Vector2.zero;
            rb.AddForce(((Vector2)transform.position - playerPosition).normalized * knockBackForce, ForceMode2D.Impulse);
            health -= damageDealt;
            timer = 0;
            if (health <= 0)
            {
                state = WispState.Die;
                anim.SetTrigger("Die");
            }
        }
        else
        {
            return;
        }
    }

    public void Die()
    {
        Destroy(gameObject);
    }

    /// <summary>Move towards the player</summary>
    private void Chasing()
    {
        if (playerDetected && player.hidden)
        {
            playerDetected = false; 
            CalmDownAnimation();
            //targetWaypoint = points[pointIndex];
            state = WispState.Patroling;
            return;
        }
        else if (playerDetected && !player.hidden)
        {
            MoveToPlayer();
        }
            
    }

    private void Charge()
    {
        StartTimer(2.5f);
        
        if(player.hidden)
        {
            ResetTimer();
            CalmDownAnimation();
            state = WispState.Patroling;
            return;
        }

        if (timerEnded)
        {
            AttackStartAnimation();
            ResetTimer();
            state = WispState.Attack;
        }
        else
        {
            chargeDirection = (player.transform.position - transform.position).normalized;
            transform.position += 0.5f * Time.deltaTime * -chargeDirection;
        }
           
    }

    private void WaitForNextAttack()
    {
        StartTimer(2.5f);

        if (player.hidden)
        {
            ResetTimer();
            CalmDownAnimation();
            state = WispState.Patroling;
            return;
        }

        if (attackMissed)
        {
            if (Vector3.Distance(transform.position, player.transform.position) > 2.4f)
            {
                state = WispState.Chasing;
            }
            else
            {
                state = WispState.Charging;
            }
        }
        else
        {
            if (timerEnded)
            {
                if (Vector3.Distance(transform.position, player.transform.position) > 2.4f)
                {
                    state = WispState.Chasing;
                }
                else
                {
                    state = WispState.Charging;
                }
                ResetTimer();
            }
        }
        
    }

    /// <summary>Checks whether idling is over</summary>
    private void Idling()
    {
        if (!playerDetected)
        {
            StartTimer(1.5f);

            if (timerEnded)
            {
                ResetTimer();
                state = WispState.Patroling;
                Patrol();
            }
        }
        else
        {
            ResetTimer();
            AngryAnimation();
            state = WispState.Chasing;
        }
    }

    private void Attack()
    {
        rb.AddForce(chargeDirection * 9, ForceMode2D.Impulse);
        state = WispState.Dashing;
    }

    private void KnockBack()
    {
        startKnockback = false;

        AttackEndAnimation();
        rb.velocity = Vector2.zero;
        rb.AddForce(-chargeDirection * 4, ForceMode2D.Impulse);
        state = WispState.Cooldown;
    }

    private void MoveToPlayer()
    {
        if (Vector3.Distance(transform.position, player.transform.position) <= 2.4f)
        {
            state = WispState.Charging;
        }
        else
        {
            transform.position = Vector2.MoveTowards(transform.position, player.transform.position, speed * Time.deltaTime);
        }
    }

    private void Patrol()
    {
        if (!playerDetected)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetWaypoint, speed * Time.deltaTime);

            if (transform.position == (Vector3)targetWaypoint)
            {
                current++;
                current %= points.Length;
                targetWaypoint = points[current];
                state = WispState.Idle;
            }
        }
        else
        {
            AngryAnimation();
            state = WispState.Chasing;
        }
    }

    /// <summary>Checks if it's facing right direction</summary>
    private void FlipCheck()
    {
        if (!playerDetected)
        {
            if (facingLeft)
            {
                if (targetWaypoint.x > transform.position.x)
                {
                    facingLeft = false;
                    transform.localScale = new Vector3(1, 1, 1);
                }
            }
            else
            {
                if (targetWaypoint.x < transform.position.x)
                {
                    facingLeft = true;
                    transform.localScale = new Vector3(-1, 1, 1);
                }
            }
            
        }
        else
        {
            if (facingLeft)
            {
                if (player.transform.position.x > transform.position.x)
                {
                    facingLeft = false;
                    transform.localScale = new Vector3(1, 1, 1);
                }
            }
            else
            {
                if (player.transform.position.x < transform.position.x)
                {
                    facingLeft = true;
                    transform.localScale = new Vector3(-1, 1, 1);
                }
            }
        }
    }

    private void AttackEndAnimation()
    {
        anim.SetBool("Attack", false);
    }
    private void CalmDownAnimation()
    {
        anim.SetBool("Angry", false);
        anim.SetBool("Attack", false);
    }

    private void AngryAnimation()
    {
        anim.SetBool("Angry", true);
        anim.SetBool("Attack", false);
    }

    private void AttackStartAnimation()
    {
        anim.SetBool("Attack", false);
        anim.SetBool("Attack", true);
    }

    private void StartTimer(float cooldown)
    {
        if (!timerEnded)
        {
            timer += Time.deltaTime;

            if (timer >= cooldown)
            {
                timer = 0;
                timerEnded = true;
            }
        }
    }

    private void ResetTimer()
    {
        timer = 0;
        timerEnded = false;
    }

    private void OnDrawGizmos()
    {
        for (int i = 1; i < points.Length; i++)
        {
            Debug.DrawLine(points[i-1], points[i], Color.green);
        }
    }
}

public enum WispState
{
    Idle,
    Patroling,
    Chasing,
    Charging,
    Attack,
    Dashing,
    Hit,
    Cooldown,
    Die
}
