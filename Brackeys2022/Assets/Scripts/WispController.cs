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
    [SerializeField] private float speed = 3;
    [SerializeField] private float idleTime = 1;
    private bool facingLeft;


    [Header("Combat Fields")]
    [SerializeField] private Transform attackRange;
    [SerializeField] private BoxCollider2D detectRange;
    [SerializeField] private float knockBackForce = 5;
    [SerializeField] private float health = 10;
    [SerializeField] private float attackSpeed = 5;
    private static PlayerController player;

    private float timer;
    private WispState state;

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        state = WispState.Idle;
        timer = 0;
        targetWaypoint = points[pointIndex];
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
            case WispState.Idle:
                Idling();
                break;
            case WispState.Walking:
                Walking();
                break;
            case WispState.Chasing:
                Chasing();
                break;
            case WispState.Hit:
                break;
            case WispState.AttackReady:
                Attacking();
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (state == WispState.Die)
                return;
            state = WispState.Chasing;
            GetAngry();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (state == WispState.Die)
                return;
            state = WispState.Idle;
            CalmDown();
            targetWaypoint = points[pointIndex];
            FlipCheck();
        }
    }

    private void DetectPlayer()
    {
        if (state == WispState.Die)
            return;
        if (detectRange.IsTouchingLayers(128))
        {
            state = WispState.Chasing;
            GetAngry();
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
    public void HitEnd()
    {
        GetAngry();
        state = WispState.Chasing;
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
            CalmDown();
            targetWaypoint = points[pointIndex];
            FlipCheck();
            return;
        }
        targetWaypoint = player.transform.position;
        FlipCheck();
        if (Physics2D.OverlapBox(attackRange.position, attackRange.localScale, 0, 128) != null)
            AttackModeStart();

    }

    private void Attacking()
    {
        if (player.hidden)
        {
            CalmDown();
            targetWaypoint = points[pointIndex];
            FlipCheck();
            return;
        }
        targetWaypoint = player.transform.position;
        FlipCheck();
        if (Physics2D.OverlapBox(attackRange.position, attackRange.localScale, 0, 128) == null)
            AttackModeEnd();
        else
            StartCoroutine(Attack(player.transform.position));
    }

    /// <summary>Checks whether the walk is over</summary>
    private void Walking()
    {
        if (facingLeft)
        {
            if (transform.position.x <= targetWaypoint.x)
            {
                pointIndex++;
                pointIndex = pointIndex % points.Length;
                targetWaypoint = points[pointIndex];
                state = WispState.Idle;
                rb.velocity = Vector2.zero;
            }
        }
        else if (transform.position.x >= targetWaypoint.x)
        {
            pointIndex++;
            pointIndex = pointIndex % points.Length;
            targetWaypoint = points[pointIndex];
            state = WispState.Idle;
            rb.velocity = Vector2.zero;
        }
    }

    /// <summary>Checks whether idling is over</summary>
    private void Idling()
    {
        timer += Time.deltaTime;
        if (timer >= idleTime)
        {
            state = WispState.Walking;
            FlipCheck();
            timer = 0;
        }
    }

    private void CalmDown()
    {
        anim.SetBool("Angry", false);
        anim.SetBool("Attack", false);
    }

    private void GetAngry()
    {
        anim.SetBool("Angry", true);
        anim.SetBool("Attack", false);
    }

    private void AttackModeStart()
    {
        anim.SetBool("Attack", true);
    }

    public void AttackReady()
    {
        state = WispState.AttackReady;
    }

    private IEnumerator Attack(Vector3 target)
    {
        state = WispState.Attack;
        rb.AddForce((transform.position - target).normalized * attackSpeed, ForceMode2D.Impulse);

        while ((transform.position - player.transform.position).magnitude > 1)
            yield return null;

        state = WispState.AttackReady;
    }
    private void AttackModeEnd()
    {
        anim.SetBool("Attack", false);
    }

    /// <summary>Checks if it's facing right direction</summary>
    private void FlipCheck()
    {
        if (facingLeft)
        {
            if (targetWaypoint.x > transform.position.x)
            {
                facingLeft = sRend.flipX = false;
            }
        }
        else
            if (targetWaypoint.x < transform.position.x)
        {
            facingLeft = sRend.flipX = true;
        }

        if (facingLeft)
            rb.velocity = new Vector2(-1 * speed, 0);
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

public enum WispState
{
    Idle,
    Walking,
    Chasing,
    AttackReady,
    Attack,
    Hit,
    Die
}
