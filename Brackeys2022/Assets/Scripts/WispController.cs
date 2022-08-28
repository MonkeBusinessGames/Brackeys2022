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
    private bool playerInRange = false;


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
                Move();
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
            MoveToPlayer();
        }

        if (collision.CompareTag("Dive"))
        {
            if (state == (WispState.Die | WispState.Hit))
                return;
            TakeDamage(3, collision.transform.position);
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
            Move();
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
            Move();
            return;
        }
        MoveToPlayer();
        if (Physics2D.OverlapBox(attackRange.position, attackRange.localScale, 0, 128) != null)
            AttackModeStart();

    }

    private void Attacking()
    {
        if (player.hidden)
        {
            CalmDown();
            targetWaypoint = points[pointIndex];
            Move();
            return;
        }
        MoveToPlayer();
        if (Physics2D.OverlapBox(attackRange.position, attackRange.localScale, 0, 128) == null)
            AttackModeEnd();
        else
            StartCoroutine(Attack(player.transform.position));
    }

    /// <summary>Checks whether idling is over</summary>
    private void Idling()
    {
        timer += Time.deltaTime;
        if (timer >= idleTime)
        {
            state = WispState.Walking;
            Move();
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
        print((transform.position - target).normalized * attackSpeed);

        while (transform.position != target)
        {
            transform.position = Vector2.MoveTowards(transform.position, target, attackSpeed * Time.deltaTime);
            yield return null;
        }

        state = WispState.AttackReady;
    }

    private void AttackModeEnd()
    {
        anim.SetBool("Attack", false);
    }
    private void MoveToPlayer() //Move to the player
    {
        targetWaypoint = player.transform.position;
        FlipCheck();
        transform.position = Vector2.MoveTowards(transform.position, targetWaypoint, speed * Time.deltaTime);
    }

    private void Move() //Move to the next point
    {
        FlipCheck();
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


    /// <summary> Allows the editor to show the transform points </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green; Gizmos.color = Color.green;
        for (int i = 0; i < points.Length; i++)
        {
            Gizmos.DrawSphere(points[i], .2f);
            Gizmos.DrawLine(points[i], points[(i + 1) % points.Length]);
        }
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
