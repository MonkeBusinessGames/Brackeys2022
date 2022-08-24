using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{


    [Header("General Components")]
    [SerializeField] private SpriteRenderer sRend;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator anim;

    [Header("Movement Fields")]
    [SerializeField] private float speed;
    [SerializeField] private float jumpForce;
    [SerializeField] private BoxCollider2D groundCheck;
    [SerializeField] private LayerMask ground;
    private float movementX;
    private bool flip;
    private PlayerState state;

    [Header("Combat Fields")]
    [SerializeField] private BoxCollider2D attackRange;
    [SerializeField] private BoxCollider2D hitBox;
    [SerializeField] private ContactFilter2D enemies;
    [SerializeField] private float health = 100;
    [SerializeField] private float attackPower = 10;
    [SerializeField] private float knockBackForce = 100;
    [SerializeField] private float recoveryTime = .5f;
    private float timer;

    [SerializeField] private TMP_Text healthText;

    void Start()
    {
        state = PlayerState.Idle;
        flip = false;
        timer = 0;
    }

    void Update()
    {
        if (state == (PlayerState.Hit | PlayerState.Attack))
            return;

        //Get Walk Input
        movementX = Input.GetAxis("Horizontal");

        //Flip sprite based on movement direction
        if (flip)
        {
            if (movementX > 0)
                flip = sRend.flipX = false;
        }
        else
        {
            if (movementX < 0)
                flip = sRend.flipX = true;
        }

        //Get Input Based on State
        switch (state)
        {
            case PlayerState.Idle:
                //Start Jumping
                if (Input.GetButtonDown("Jump"))
                    state = PlayerState.JumpStart;
                else if (Input.GetButtonDown("Attack"))
                {
                    state = PlayerState.Attack;
                    SetAnimation();
                    movementX = 0;
                }
                break;
            case PlayerState.Walking:
                //Start Jumping
                if (Input.GetButtonDown("Jump"))
                    state = PlayerState.JumpStart;
                else if (Input.GetButtonDown("Attack"))
                {
                    state = PlayerState.Attack;
                    SetAnimation();
                    movementX = 0;
                }
                break;
            case PlayerState.JumpStart:
                break;
            case PlayerState.Jumping:
                //Short Jump
                if (Input.GetButtonUp("Jump"))
                    state = PlayerState.JumpStop;
                break;
            case PlayerState.JumpStop:
                break;
            case PlayerState.Falling:
                break;
        }
            
    }

    void FixedUpdate()
    {
        if (state == PlayerState.Hit)
            return;

        //Set Velocity
        rb.velocity = new Vector2(movementX * speed, rb.velocity.y);

        //State Machine
        switch (state)
        {
            case PlayerState.Idle:
                if (rb.velocity.x != 0)
                {
                    state = PlayerState.Walking;
                    SetAnimation();
                }
                break;
            case PlayerState.Walking:
                if (rb.velocity.x == 0)
                {
                    state = PlayerState.Idle;
                    SetAnimation();
                }
                break;
            case PlayerState.JumpStart:
                //Initiates Jump
                rb.AddForce(new Vector2(0, jumpForce));
                state = PlayerState.Jumping;
                SetAnimation();
                break;
            case PlayerState.Jumping:
                if (rb.velocity.y <= 0)
                    state = PlayerState.Falling;
                break;
            case PlayerState.JumpStop:
                //Cuts Jump Short
                rb.velocity = new Vector2(movementX * speed, rb.velocity.y / 2);
                state = PlayerState.Falling;
                break;
            case PlayerState.Falling:
                if(groundCheck.IsTouchingLayers(ground))
                    state = PlayerState.Idle;
                break;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.collider.tag == "Enemy")
        {
            if (state != PlayerState.Hit)
            {
                state = PlayerState.Hit;
                rb.velocity = Vector2.zero;
                rb.AddForce((transform.position - collision.transform.position).normalized * knockBackForce, ForceMode2D.Impulse);
                health -= 1;
                healthText.text = "Health: " + health.ToString();
                if (health <= 0)
                    state = PlayerState.Die;

                SetAnimation();
            }
        }
    }

    public void HitCheck() 
    {

        List<Collider2D> hitEnemies = new List<Collider2D>();
        attackRange.OverlapCollider(enemies, hitEnemies);
        for(int i = 0; i < hitEnemies.Count; i++)
        {
            hitEnemies[i].GetComponent<EnemyController>().TakeDamage(attackPower, transform.position);
        }
    }
    public void DamageCheck(Transform enemyRange, float damage)
    {
            state = PlayerState.Hit;
            rb.velocity = Vector2.zero;
            rb.AddForce((transform.position - enemyRange.position).normalized * knockBackForce, ForceMode2D.Impulse);
            health -= damage;
            healthText.text = "Health: " + health.ToString();
            if (health <= 0)
                state = PlayerState.Die;
            SetAnimation();
    }

    public void animationEnd()
    {
        state = PlayerState.Idle;
        SetAnimation();
    }

    private void SetAnimation()
    {
        switch (state)
        {
            case PlayerState.Idle:
                anim.SetInteger("State", 0);
                break;
            case PlayerState.Walking:
                anim.SetInteger("State", 1);
                break;
            case PlayerState.Hit:
                anim.SetInteger("State", 2);
                break;
            case PlayerState.JumpStart:
                anim.SetInteger("State", 3);
                break;
            case PlayerState.Die:
                anim.SetInteger("State", 4);
                break;
            case PlayerState.Attack:
                anim.SetInteger("State", 5);
                break;
        }
    }
}

public enum PlayerState
{
    Idle,
    Walking,
    JumpStart,
    Jumping,
    JumpStop,
    Falling,
    Hit,
    Die,
    Attack,
}
