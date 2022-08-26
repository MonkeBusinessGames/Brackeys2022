using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    public delegate void PlayerAction();
    public static event PlayerAction OnHideEnd;

    [Header("General Components")]
    [SerializeField] private SpriteRenderer sRend;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator anim;
    [SerializeField] private SpriteRenderer wingBack;
    [SerializeField] private SpriteRenderer wingFront;


    [Header("Movement Fields")]
    [SerializeField] private float speed;
    [SerializeField] private float jumpForce;
    [SerializeField] private BoxCollider2D groundCheck;
    [SerializeField] private LayerMask ground;
    private float movementX;
    private bool flip;
    private PlayerState state;
    public bool hidden;
    private bool doubleJumped;

    [Header("Combat Fields")]
    [SerializeField] private BoxCollider2D attackRange;
    [SerializeField] private BoxCollider2D hitBox;
    [SerializeField] private ContactFilter2D enemies;
    [SerializeField] private float health = 100;
    [SerializeField] private float attackPower = 10;
    [SerializeField] private float knockBackForce = 100;
    [SerializeField] private float recoveryTime = .5f;
    [SerializeField] private TMP_Text healthText;

    [Header("Long/Double Jump Fields")]
    [Range(0, 1)]
    [Tooltip("By how many percent will the jump force increase, if the player holds for whole defined duration. 0s - 0%; 0.5s - 50%; 1s - 200%")]
    [SerializeField] private float jumpHoldTime;

    [Tooltip("Enables: Long Jump")]
    [SerializeField] private bool longJumpEnabled;

    [Tooltip("Enables: Double Jump")]
    [SerializeField] private bool doubleJumpEnabled; 


    void Start()
    {
        state = PlayerState.Idle;
        flip = false;
        doubleJumped = false;
        hidden = false;
    }

    void Update()
    {
        if (state == (PlayerState.Hit))
            return;
        if (state == (PlayerState.Attack))
            return;

        //Get Walk Input
        movementX = Input.GetAxis("Horizontal");

        //Flip sprite based on movement direction
        if (flip)
        {
            if (movementX > 0)
                flip = sRend.flipX = wingBack.flipX = wingFront.flipX = false;
        }
        else
        {
            if (movementX < 0)
                flip = sRend.flipX = wingBack.flipX = wingFront.flipX = true;
        }

        //Get Input Based on State
        switch (state)
        {
            case PlayerState.Idle:
                //Handle Hide Input
                if (HideCheck())
                    break;

                //Start Jumping
                if (Input.GetButtonDown("Jump"))
                {
                    EndFall();
                    state = PlayerState.JumpStart;
                    SetAnimation();
                }
                else if (!groundCheck.IsTouchingLayers(ground))
                {
                    EndFall();
                    state = PlayerState.Falling;
                    SetAnimation();
                }
                else if (Input.GetButtonDown("Attack"))
                {
                    EndFall();
                    state = PlayerState.Attack;
                    SetAnimation();
                    movementX = 0;
                }
                break;
            case PlayerState.Walking:
                //Handle Hide Input
                if (HideCheck())
                    break;

                //Start Jumping
                if (Input.GetButtonDown("Jump"))
                {
                    state = PlayerState.JumpStart;
                    SetAnimation();
                }
                else if (!groundCheck.IsTouchingLayers(ground))
                {
                    state = PlayerState.Falling;
                    SetAnimation();
                }
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
                anim.SetFloat("Jump Velocity", rb.velocity.y);
                if (Input.GetButtonUp("Jump"))
                {
                    rb.velocity *= new Vector2(1, .5f);
                    state = PlayerState.JumpStop;
                }
                else if (rb.velocity.y < 5f) 
                { 
                    state = PlayerState.JumpStop;
                }
                break;
            case PlayerState.JumpStop:
                anim.SetFloat("Jump Velocity", rb.velocity.y);

                DoubleJumpCheck();
                break;
            case PlayerState.Falling:
                anim.SetFloat("Jump Velocity", rb.velocity.y);

                DoubleJumpCheck();

                //If the player touches the ground, reset them to idle.
                if (groundCheck.IsTouchingLayers(ground))
                {
                    doubleJumped = false;
                    state = PlayerState.Idle;
                    SetAnimation();
                }
                break;
            case PlayerState.DoubleJump:
                anim.SetFloat("Jump Velocity", rb.velocity.y);
                break;
            case PlayerState.Hit:
                //Handle Hide Input
                HideCheck();
                break;
        }
    }

    void FixedUpdate()
    {
        if (state == (PlayerState.Hit))
            return;

        //Set Velocity
        rb.velocity = new Vector2(movementX * speed, rb.velocity.y);

        //State Machine
        switch (state)
        {
            case PlayerState.Idle:
                if (rb.velocity.x != 0)
                {
                    EndFall();
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
                rb.AddForce(new Vector2(0, jumpForce));  // + 1, because if player releases jump button after 0.5s then it would decrease jumpForce by 50% 
                state = PlayerState.Jumping;
                SetAnimation();
                break;
            case PlayerState.Jumping:
                if (rb.velocity.y <= 0)
                    state = PlayerState.Falling;
                break;
            case PlayerState.JumpStop:
                //Keep Floating
                if(rb.velocity.y <= 0)
                    rb.velocity = new Vector2(movementX * speed, 0);
                break;
            case PlayerState.DoubleJump:
                //Keep Floating
                if (rb.velocity.y <= 0)
                    rb.velocity = new Vector2(movementX * speed/2, 0);
                break;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hidden)
            return;

        if (collision.collider.CompareTag("Enemy"))
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

    /// <summary>Use this method to enable doublejump, when animal selection event is fired</summary>
    public void AddDoubleJump()
    {
        doubleJumpEnabled = true;
    }

    /// <summary>Use this method to enable long jump, when animal selection event is fired</summary>
    public void EnableLongJump()
    {
        longJumpEnabled = true;
    }

    /// <summary>Checks if the any enemies were hit</summary>
    public void HitCheck() 
    {
        List<Collider2D> hitEnemies = new List<Collider2D>();
        attackRange.OverlapCollider(enemies, hitEnemies);
        for(int i = 0; i < hitEnemies.Count; i++)
        {
            try
            {
                hitEnemies[i].GetComponent<EnemyController>().TakeDamage(attackPower, transform.position);
            }
            catch (System.NullReferenceException)
            {
                hitEnemies[i].GetComponent<WispController>().TakeDamage(attackPower, transform.position);
            }
        }
    }

    /// <summary>Checks how much damage to take</summary>
    public void DamageCheck(Transform enemyRange, float damage)
    {
            state = PlayerState.Hit;
            sRend.color = Color.grey;
            rb.velocity = Vector2.zero;
            rb.AddForce((transform.position - enemyRange.position).normalized * knockBackForce, ForceMode2D.Impulse);
            health -= damage;
            healthText.text = "Health: " + health.ToString();
            if (health <= 0)
                state = PlayerState.Die;
            SetAnimation();
    }

    /// <summary>Ends the animation and resets to Idle</summary>
    public void AnimationEnd()
    {
        sRend.color = Color.white;
        state = PlayerState.Idle;
        SetAnimation();
    }

    /// <summary>Checks whether to double jump</summary>
    public void DoubleJumpCheck()
    {
        if (doubleJumpEnabled & !doubleJumped)
            if (Input.GetButtonDown("Jump"))
            {
                doubleJumped = true;
                state = PlayerState.DoubleJump;
                SetAnimation();
            }
    }

    /// <summary>Initiates the double jump force</summary>
    public void StartDoubleJump()
    {
        rb.AddForce(new Vector2(0, jumpForce));
        state = PlayerState.Jumping;
        SetAnimation();
    }

    /// <summary>Starts player falling after peaking a jump</summary>
    public void StartFall()
    {
        state = PlayerState.Falling;
    }

    /// <summary>Lands a player on the ground</summary>
    public void EndFall()
    {
        anim.SetFloat("Jump Velocity", 0);
    }

    /// <summary>Checks whether to hide</summary>
    private bool HideCheck()
    {
        if (hidden)
        {
            if (state == PlayerState.Hit)
            {
                anim.speed = 1;
                sRend.color = Color.white;
                hidden = false;
                speed *= 4;
                Physics2D.IgnoreLayerCollision(3, 7, false);
                OnHideEnd();
            }
            else if (Input.GetButtonUp("Hide"))
            {
                anim.speed = 1;
                sRend.color = Color.white;
                hidden = false;
                speed *= 4;
                Physics2D.IgnoreLayerCollision(3, 7, false);
                OnHideEnd();
            }
            return true;
        }
        else if (Input.GetButtonDown("Hide"))
        {
            anim.speed = .5f;
            sRend.color = Color.black;
            hidden = true;
            speed /= 4;
            Physics2D.IgnoreLayerCollision(3, 7, true);
        }
        return false;
    }

    /// <summary>Sets the animation based on the player state</summary>
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
            case PlayerState.Jumping:
                anim.SetInteger("State", 6);
                break;
            case PlayerState.DoubleJump:
                anim.SetInteger("State", 7);
                break;
            case PlayerState.Falling:
                anim.SetInteger("State", 8);
                break;
        }
    }
}

/// <summary>The state the player is in</summary>
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
    DoubleJump,
}
