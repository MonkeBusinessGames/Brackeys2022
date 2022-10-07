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
    private bool facingLeft;
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
    private float timer;
    private WispState state;

    Rigidbody2D playerRigidbody2D;

    private void Awake()
    {
        player = FindObjectOfType<PlayerController>();
        playerRigidbody2D = player.GetComponent<Rigidbody2D>();
    }
    void Start()
    {
        targetWaypoint = points[pointIndex];

        state = WispState.Idle;
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
        FlipCheck();
        DetectPlayer();

        print(state);

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

            print("Collided " + transform.name);
            startKnockback = true;
        }
    }
    private void CheckIfCollided()
    {
        if (startKnockback)
        {
            startKnockback = false;
            state = WispState.Hit;
        }
    }

    private void DetectPlayer()
    {
        if (state == WispState.Die)
            return;

        if (detectRange.IsTouchingLayers(playerMask) && !playerDetected)
        {
            playerDetected = true;
            state = WispState.Chasing;
            SetAngryAnimation();
        }
    }

    public void TakeDamage(float damageDealt, Vector2 playerPosition)
    {
        state = WispState.Hit;
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

    public void Die()
    {
        Destroy(gameObject);
    }

    /// <summary>Move towards the player</summary>
    private void Chasing()
    {
        if (player.hidden)
        {
            playerDetected = false; 
            CalmDown();
            targetWaypoint = points[pointIndex];
            Patrol();
            return;
        }

        MoveToPlayer();
    }

    private void Charge()
    {
        StartTimer(1.6f);

        if (timerEnded)
            state = WispState.Attack;
    }

    private void WaitForNextAttack()
    {
        StartTimer(3f);

        if (timerEnded)
        {
            timerEnded = false;
            state = WispState.Chasing;
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
                timerEnded = false;
                state = WispState.Patroling;
                Patrol();
            }
        }
    }

    private void Attack()
    {
        rb.AddForce(chargeDirection * 8, ForceMode2D.Impulse);
        state = WispState.Dashing;
    }

    private void KnockBack()
    {
        AttackModeEnd();
        rb.velocity = Vector2.zero;
        rb.AddForce(-chargeDirection * 4, ForceMode2D.Impulse);

        state = WispState.Cooldown;
    }

    private void MoveToPlayer() //Move to the player
    {
        if (playerDetected && state.Equals(WispState.Chasing))
        {
            if (Vector3.Distance(transform.position, targetWaypoint) <= 2f)
            {
                targetWaypoint = player.transform.position;
                state = WispState.Attack;
            }
            else
            {
                transform.position = Vector2.MoveTowards(transform.position, targetWaypoint, speed * Time.deltaTime);
            }
        }
    }

    private void Patrol() //Move to the next point
    {
        if (!playerDetected)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetWaypoint, speed * Time.deltaTime);

            if (transform.position == (Vector3)targetWaypoint)
            {
                current++;
                current = current % points.Length;
                targetWaypoint = points[current];
                state = WispState.Idle;
                rb.velocity = Vector2.zero;
            }
        }
    }

    /// <summary>Checks if it's facing right direction</summary>
    private void FlipCheck()
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
            if (targetWaypoint.x < transform.position.x)
        {
            facingLeft = true;
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    private void AttackModeEnd()
    {
        anim.SetBool("Attack", false);
    }
    private void CalmDown()
    {
        anim.SetBool("Angry", false);
        anim.SetBool("Attack", false);
    }

    private void SetAngryAnimation()
    {
        anim.SetBool("Angry", true);
        anim.SetBool("Attack", false);
    }

    private void AttackModeAnimationStart()
    {
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
