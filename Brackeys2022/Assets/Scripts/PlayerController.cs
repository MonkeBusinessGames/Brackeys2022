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
    [SerializeField] private UIManager uiManager;


    [Header("Movement Fields")]
    [SerializeField] private float speed = 8;
    [SerializeField] private float jumpForce = 900;
    [SerializeField] private BoxCollider2D groundCheck;
    [SerializeField] private LayerMask ground;
    private float movementX;
    private bool flip = false;
    private PlayerState state;
    public bool hidden = false;
    private bool doubleJumped = false;

    [Header("Combat Fields")]
    [SerializeField] private BoxCollider2D clawRange;
    [SerializeField] private PolygonCollider2D diveRange;
    [SerializeField] private PolygonCollider2D spikeRange;
    [SerializeField] private BoxCollider2D hitBox;
    [SerializeField] private ContactFilter2D enemies;
    private float health = 5;
    [SerializeField] private float attackPower = 10;
    [SerializeField] private float knockBackForce = 100;
    [SerializeField] private bool keepAttacking = false;

    [Header("Long/Double Jump Fields")]
    [SerializeField] private float jumpHoldTime = 2;

    [Tooltip("Enables: Long Jump and Claw Abilities")]
    [SerializeField] private bool catAcquired = false;
    [SerializeField] private float longJumpForce = 500;
    private float timer = 0;
    private bool longJumpCharged = false;

    [Tooltip("Enables: Double Jump and Dive Abilities")]
    [SerializeField] private bool birbAcquired;
    [SerializeField] private Vector2 diveSpeed;

    [Tooltip("Enables: Dig and Spike Abilities")]
    [SerializeField] private bool moleAcquired;

    [Header("Player SFX")]
    [SerializeField] private AK.Wwise.Event footstepsEvent;
    [SerializeField] private AK.Wwise.Event PlayerGetHit;
    [SerializeField] private AK.Wwise.Event PlayerLanding;
    [SerializeField] private AK.Wwise.Event jumpSound;
    [SerializeField] private AK.Wwise.Event hideSound;
    [SerializeField] private AK.Wwise.Event unhideSound;
    [SerializeField] private AK.Wwise.Event PlayerAttack1;
    [SerializeField] private AK.Wwise.Event PlayerAttack2;
    [SerializeField] private AK.Wwise.Event PlayerAttack3;
    [SerializeField] private AK.Wwise.Event PlayerDeath;
    
    void Start()
    {
        catAcquired = false;
        birbAcquired = false;
        moleAcquired = false;

        state = PlayerState.Idle;
        flip = false;
        doubleJumped = false;
        hidden = false;
        timer = 0;
        longJumpCharged = false;
    }

    void Update()
    {
        if (state == (PlayerState.Hit))
            return;

        //Get Walk Input
        movementX = Input.GetAxis("Horizontal");

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
                    if(catAcquired)
                    {
                        EndFall();
                        state = PlayerState.Attack;
                        anim.SetInteger("AttackCounter", 0);
                        SetAnimation();
                        movementX = 0;
                    }
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
                    if (catAcquired)
                    {
                        EndFall();
                        state = PlayerState.Attack;
                        anim.SetInteger("AttackCounter", 0);
                        SetAnimation();
                        movementX = 0;
                    }
                }
                break;
            case PlayerState.JumpStart:
                break;
            case PlayerState.Jumping:
                // Short Jump
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
                DiveCheck();
                break;
            case PlayerState.JumpStop:
                anim.SetFloat("Jump Velocity", rb.velocity.y);

                DoubleJumpCheck();
                DiveCheck();
                break;
            case PlayerState.Falling:
                anim.SetFloat("Jump Velocity", rb.velocity.y);

                DoubleJumpCheck();
                DiveCheck();

                //If the player touches the ground, reset them to idle.
                if (groundCheck.IsTouchingLayers(ground))
                {
                    longJumpCharged = false;
                    doubleJumped = false;
                    state = PlayerState.Idle;
                    SetAnimation();
                    anim.SetFloat("Jump Velocity", -1);
                }
                break;
            case PlayerState.Dive:
                anim.SetFloat("Jump Velocity", rb.velocity.y);
                movementX = 0;
                //If the player touches the ground, reset them to idle.
                if (groundCheck.IsTouchingLayers(ground))
                {
                    longJumpCharged = false;
                    doubleJumped = false;
                    state = PlayerState.Idle;
                    SetAnimation();
                    diveRange.enabled = false;
                    anim.SetFloat("Jump Velocity", -1);
                }
                break;
            case PlayerState.DoubleJump:
                anim.SetFloat("Jump Velocity", rb.velocity.y);
                break;
            case PlayerState.Hit:
                //Handle Hide Input
                HideCheck();
                break;
            case PlayerState.JumpCharge:
                if (Input.GetAxis("Vertical") >= 0)
                {
                    longJumpCharged = false;
                    sRend.color = Color.white;
                    timer = 0;
                    state = PlayerState.Idle;
                    SetAnimation();
                }
                if (longJumpCharged)
                {
                    if (Input.GetButtonDown("Jump"))
                    {
                        sRend.color = Color.white;
                        state = PlayerState.JumpStart;
                        SetAnimation();
                        timer = 0;
                    }
                }
                else
                {
                    timer += Time.deltaTime;
                    sRend.color = Color.grey;
                    if (timer >= jumpHoldTime)
                    {
                        longJumpCharged = true;
                        doubleJumped = true;
                        sRend.color = Color.cyan;
                        timer = 0;
                    }
                }
                break;
            case PlayerState.Attack:
                if (Input.GetButtonDown("Attack"))
                    keepAttacking = true;
                break;
        }

        //Flip sprite based on movement direction
        if (flip)
        {
            if (movementX > 0)
            {
                sRend.flipX = false;
                flip = false;
            }
        }
        else
        {
            if (movementX < 0)
            {
                sRend.flipX = true;
                flip = true;
            }
        }

    }

    void FixedUpdate()
    {
        if (state == (PlayerState.Hit))
            return; 
        if (state == (PlayerState.JumpCharge))
            return;

        //Set Velocity
        if (!longJumpCharged)
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
                if (longJumpCharged)
                {
                    if (flip)
                        rb.AddForce(new Vector2(-longJumpForce, jumpForce));
                    else
                        rb.AddForce(new Vector2(longJumpForce, jumpForce));
                }
                else
                    rb.AddForce(new Vector2(0, jumpForce)); 
                state = PlayerState.Jumping;
                SetAnimation();
                break;
            case PlayerState.Jumping:
                if (rb.velocity.y <= 0)
                    state = PlayerState.Falling;
                break;
            case PlayerState.JumpStop:
                //Keep Floating
                if (rb.velocity.y <= 0)
                    rb.velocity = new Vector2(movementX * speed, 0);
                break;
            case PlayerState.DoubleJump:
                //Keep Floating
                if (rb.velocity.y <= 0)
                    rb.velocity = new Vector2(movementX * speed/2, 0);
                break;
            case PlayerState.Dive:
                //Keep Diving
                if (flip)
                    rb.velocity = new Vector2(-1*diveSpeed.x, diveSpeed.y);
                else
                    rb.velocity = diveSpeed;
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
                uiManager.RemoveHealth();
                if (health <= 0)
                {
                    state = PlayerState.Die;
                    anim.updateMode = AnimatorUpdateMode.UnscaledTime;
                    Time.timeScale = 0;
                    Time.timeScale = 0;
                }

                SetAnimation();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {

        if (collider.CompareTag("End"))
        {
            uiManager.GameComplete();
        }

        if (hidden)
        return;

            if (collider.CompareTag("Enemy"))
        {
            if (state != PlayerState.Hit)
            {
                state = PlayerState.Hit;
                rb.velocity = Vector2.zero;
                rb.AddForce((transform.position - collider.transform.position).normalized * knockBackForce, ForceMode2D.Impulse);
                health -= 1;
                uiManager.RemoveHealth();
                if (health <= 0)
                {
                    state = PlayerState.Die;
                    anim.updateMode = AnimatorUpdateMode.UnscaledTime;
                    Time.timeScale = 0;
                    Time.timeScale = 0;
                }

                SetAnimation();
            }
        }
    }

    /// <summary>Use this method to enable doublejump and dive, when animal selection event is fired</summary>
    public void AcquireBirbAbilities()
    {
        birbAcquired = true;
    }

    /// <summary>Use this method to enable long jump and claws, when animal selection event is fired</summary>
    public void AcquireCatAbilities()
    {
        catAcquired = true;
    }

    /// <summary>Use this method to enable dig and spikes, when animal selection event is fired</summary>
    public void AcquireMoleAbilities()
    {
        moleAcquired = true;
    }

    /// <summary>Checks if the any enemies were hit by the claw</summary>
    public void HitCheck() 
    {
        
        List<Collider2D> hitEnemies = new List<Collider2D>();
        clawRange.OverlapCollider(enemies, hitEnemies);
        for (int i = 0; i < hitEnemies.Count; i++)
        {
            try
            {
                hitEnemies[i].GetComponent<EnemyController>().TakeDamage(attackPower, transform.position);
            }
            catch (System.NullReferenceException)
            {
                try
                {
                    hitEnemies[i].GetComponent<WispController>().TakeDamage(attackPower, transform.position);
                }
                catch (System.NullReferenceException)
                {
                    hitEnemies[i].GetComponent<SkeletonController>().TakeDamage(attackPower, transform.position);
                }
            }
        }
    }

    /// <summary>Checks how much damage to take</summary>
    public void DamageCheck(Transform enemyRange, float damage)
    {
        if (hidden)
        {
            anim.speed = 1;
            sRend.color = Color.white;
            hidden = false;
            speed *= 2;
            Physics2D.IgnoreLayerCollision(3, 7, false);
            OnHideEnd();
        }
        
        state = PlayerState.Hit;
        anim.SetInteger("AttackCounter", 1);
        diveRange.enabled = false;
        rb.velocity = Vector2.zero;
        rb.AddForce((transform.position - enemyRange.position).normalized * knockBackForce, ForceMode2D.Impulse);
        health -= 1;
        //AkSoundEngine.PostEvent(PlayerGetHit.Id, this.gameObject);
        uiManager.RemoveHealth();
        if (health <= 0)
                state = PlayerState.Die;
            SetAnimation();
    }

    /// <summary>Ends the animation and resets to Idle</summary>
    public void AnimationEnd()
    {
        sRend.color = Color.white;
        if (keepAttacking)
        {
            keepAttacking = false;
            if (anim.GetInteger("AttackCounter") == 0)
            {
                state = PlayerState.Attack;
                anim.SetInteger("AttackCounter", 1);
                return;
            }
            if (anim.GetInteger("AttackCounter") == 1)
            {
                state = PlayerState.Attack;
                anim.SetInteger("AttackCounter", 2);
                return;
            }
            
        }
        state = PlayerState.Idle;
    }

    /// <summary>Activates the game over experience</summary>
    public void DeathEnd()
    {
        anim.updateMode = AnimatorUpdateMode.UnscaledTime;
        uiManager.GameOver();
    }

    /// <summary>Checks whether to double jump</summary>
    public void DoubleJumpCheck()
    {
        if (birbAcquired & !doubleJumped)
            if (Input.GetButtonDown("Jump"))
            {
                doubleJumped = true;
                state = PlayerState.DoubleJump;
                //AkSoundEngine.PostEvent(jumpSound.Id, this.gameObject);
                SetAnimation();
            }
    }
    /// <summary>Checks whether to dive</summary>
    public void DiveCheck()
    {
        if (moleAcquired)
            if (Input.GetButtonDown("Attack"))
            {
                doubleJumped = true;
                state = PlayerState.Dive;
                SetAnimation();
                diveRange.enabled = true;
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
        anim.SetFloat("Jump Velocity", -1);
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
            if (Input.GetButtonUp("Hide"))
            {
                anim.speed = 1;
                sRend.color = Color.white;
                hidden = false;
                speed *= 2;
                Physics2D.IgnoreLayerCollision(3, 7, false);
                //AkSoundEngine.PostEvent(unhideSound.Id, this.gameObject);
                OnHideEnd();
                

            }
            return true;
        }
        else if (Input.GetButtonDown("Hide"))
        {
            //AkSoundEngine.PostEvent(hideSound.Id, this.gameObject);
            anim.speed = .5f;
            sRend.color = Color.black;
            hidden = true;
            speed /= 2;
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
            case PlayerState.Dive:
                anim.SetInteger("State", 9);
                break;
        }
    }

    //SFX
    public void PlayFootstepSound()
    {
       // AkSoundEngine.PostEvent(footstepsEvent.Id, this.gameObject);
    }
    public void PlayLandingSound()
    {
       // AkSoundEngine.PostEvent(PlayerLanding.Id, this.gameObject);
    }

    public void PlayAttackSound1()
    {
        //AkSoundEngine.PostEvent(PlayerAttack1.Id, this.gameObject);
    }

    public void PlayAttackSound2()
    {
        //AkSoundEngine.PostEvent(PlayerAttack2.Id, this.gameObject);
    }
    
    public void PlayAttackSound3()
    {
        //AkSoundEngine.PostEvent(PlayerAttack3.Id, this.gameObject);
    }

    public void PlayDeathSound()
    {
        //AkSoundEngine.PostEvent(PlayerDeath.Id, this.gameObject);
    }
    public void PlayerGethitSound()
    {
        //AkSoundEngine.PostEvent(PlayerGetHit.Id, this.gameObject);
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
    JumpCharge,
    Dive
}
