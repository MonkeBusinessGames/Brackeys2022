using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private bool startFresh = false;
    public SaveData data;
    [SerializeField] private ParticleSystem particles;

    [Header("Object Lists")]
    [SerializeField] private Transform[] entranceList;

    [Header("Movement Fields")]
    public static bool frozen = false;
    [SerializeField] private float speed = 8;
    [SerializeField] private float jumpForce = 900;
    [SerializeField] private BoxCollider2D groundCheck;
    [SerializeField] private LayerMask ground;
    private float movementX;
    private bool flip = false;
    private PlayerState state;
    public bool hidden = false;
    public bool meditating = false;
    private bool doubleJumped = false;
    private bool isLooking;
    [SerializeField] private float coyoteTime = 0.2f;
    [SerializeField] private float coyoteTimeCounter = 0f;

    [Header("Dashing Fields")]
    [SerializeField] private float dashTime = 0.2f;
    [SerializeField] private float dashSlowdown = 0.4f;
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashCooldown;
    [HideInInspector] public bool isDashing = false;
    bool canDash = true;
    float normalGravity, initialDrag;
    DashAfterImage afterImage;  // A bit of a dependency meme

    [Header("Combat Fields")]
    [SerializeField] private BoxCollider2D clawRange;
    [SerializeField] private PolygonCollider2D diveRange;
    [SerializeField] private PolygonCollider2D spikeRange;
    [SerializeField] private BoxCollider2D hitBox;
    [SerializeField] private ContactFilter2D enemies;
    private int health = 5;
    private float mana = 20;
    [SerializeField] private float attackPower = 10;
    [SerializeField] private float knockBackForce = 100;
    [SerializeField] private bool keepAttacking = false;
    private bool invulnerable = false;

    bool timerEnded = false;
    float timer = 0;

    [Tooltip("Enables: Claw Abilities")]
    [SerializeField] private bool catAcquired = false;

    [Tooltip("Enables: Double Jump and Dive Abilities")]
    [SerializeField] private bool birbAcquired;
    [SerializeField] private Vector2 diveSpeed;

    [Tooltip("Enables: Ground Slam Abilities")]
    [SerializeField] private bool moleAcquired;

    [Tooltip("Enables: Dash/Bash Abilities")]
    [SerializeField] private bool goatAcquired;

    [Tooltip("Enables: Climb Abilities")]
    [SerializeField] private bool monkeyAcquired;

    [Header("Player SFX")]
    [SerializeField] private AK.Wwise.Event jumpSound;
    [SerializeField] private AK.Wwise.Event hideSound;
    [SerializeField] private AK.Wwise.Event unhideSound;
    [SerializeField] private AK.Wwise.Event playManaCollectSound;
    [SerializeField] private AK.Wwise.Event playBigManaCollect;

    public bool inDialogue = false; // check if dialogue is already were started

    private void Awake()
    {
        print("Awake" + gameObject.GetInstanceID() + gameObject.name);
        if (startFresh)
            data = new SaveData();
        else
            data = SaveSystem.Load();
        uiManager.pauseKey = data.keyManager.keys[Key.Pause];
        afterImage = GetComponent<DashAfterImage>();
        frozen = false;
    }

    void Start()
    {
        InitializeArea();
        isLooking = false;
        state = PlayerState.Idle;
        flip = false;
        doubleJumped = false;
        hidden = false;
        meditating = false;
        normalGravity = rb.gravityScale;
        initialDrag = rb.drag;
    }
    
    void Update()
    {
        //Handles Look Input
        Look();
        if (frozen)
            return;
        if (state == (PlayerState.Hit))
            return;

        //Get Walk Input
        movementX = data.keyManager.Horizontal();

        if (groundCheck.IsTouchingLayers(ground))
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        //Get Input Based on State
        switch (state)
        {
            case PlayerState.Idle:
                //Handle Hide Input
                if (HideCheck())
                    break;
                
                //Handle Meditate input
                if (MeditateCheck())
                    break;

                //Start Jumping
                if (Input.GetKeyDown(data.keyManager.keys[Key.Jump]) && coyoteTimeCounter > 0f)
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
                else if (Input.GetKeyDown(data.keyManager.keys[Key.Attack]))
                {
                    if(catAcquired)
                    {
                        if (mana > 1)
                        {
                            EndFall();
                            state = PlayerState.Attack;
                            anim.SetInteger("AttackCounter", 0);
                            SetAnimation();
                            movementX = 0;
                        }
                    }
                }

                break;
            case PlayerState.Walking:
                //Handle Hide Input
                if (HideCheck())
                    break;


                //Handle Meditate input
                if (MeditateCheck())
                    break;

                //Start Jumping
                if (Input.GetKeyDown(data.keyManager.keys[Key.Jump]) && coyoteTimeCounter > 0f)
                {
                    state = PlayerState.JumpStart;
                    SetAnimation();
                }
                else if (!groundCheck.IsTouchingLayers(ground) && coyoteTimeCounter <= 0f)
                {
                    state = PlayerState.Falling;
                    SetAnimation();
                }
                else if (Input.GetKeyDown(data.keyManager.keys[Key.Attack]))
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

                // Dashing
                if (goatAcquired)
                {
                    if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
                    {
                        state = PlayerState.Dash;
                    }
                }

                break;
            case PlayerState.JumpStart:
                break;
            case PlayerState.Jumping:
                // Short Jump
                anim.SetFloat("Jump Velocity", rb.velocity.y);
                if (Input.GetKeyUp(data.keyManager.keys[Key.Jump]))
                {
                    coyoteTimeCounter = 0f;
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
                if (goatAcquired)
                {
                    if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
                    {
                        state = PlayerState.Dash;

                        afterImage.ActivateAfterImages(true);
                        StartCoroutine(Dash());
                    }
                }
                DoubleJumpCheck();
                DiveCheck();
                break;
            case PlayerState.Falling:
                anim.SetFloat("Jump Velocity", rb.velocity.y);
                if (goatAcquired)
                {
                    if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
                    {
                        state = PlayerState.Dash;

                        afterImage.ActivateAfterImages(true);
                        StartCoroutine(Dash());
                    }
                }
                DoubleJumpCheck();
                DiveCheck();

                //If the player touches the ground, reset them to idle.
                if (groundCheck.IsTouchingLayers(ground))
                {
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
                    doubleJumped = false;
                    state = PlayerState.Idle;
                    SetAnimation();
                    diveRange.enabled = false;
                    anim.SetFloat("Jump Velocity", -1);
                }
                break;
            case PlayerState.DoubleJump:
                if (goatAcquired)
                {
                    if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
                    {
                        state = PlayerState.Dash;

                        afterImage.ActivateAfterImages(true);
                        StartCoroutine(Dash());
                    }
                }
                anim.SetFloat("Jump Velocity", rb.velocity.y);
                break;
            case PlayerState.Hit:
                //Handle Hide Input
                HideCheck();
                break;
            case PlayerState.Attack:
                if (Input.GetKeyDown(data.keyManager.keys[Key.Attack]))
                    keepAttacking = true;
                movementX = 0;
                break;
            case PlayerState.AttackEnd:
                movementX = 0;
                break;
            case PlayerState.Dash:
                afterImage.ActivateAfterImages(true);
                StartCoroutine(Dash());
                break;
        }

        //Flip sprite based on movement direction
        if (flip)
        {
            if (movementX > 0)
            {
                transform.localScale = new Vector3(-1, 1, 1);
                flip = false;
            }
        }
        else
        {
            if (movementX < 0)
            {
                transform.localScale = new Vector3(1, 1, 1);
                flip = true;
            }
        }

    }

    void FixedUpdate()
    {
        if (frozen)
            return;
        if (state == (PlayerState.Hit))
            return;

        CheckForDash();
    
        //State Machine
        switch (state)
        {
            case PlayerState.Idle:
                if (rb.velocity.x != 0 && !isDashing)
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

    private void CheckForDash()
    {
        //Set Velocity
        if (!isDashing)
        {
            rb.velocity = new Vector2(movementX * speed, rb.velocity.y);
        }
        else
        {
            rb.AddForce(new Vector2(movementX * dashSpeed, 0), ForceMode2D.Impulse);
        }
    }
    private void PushEnemy(Collision2D enemy)
    {
        EnemyController enemyController = enemy.gameObject.GetComponent<EnemyController>();

        rb.velocity = Vector2.zero;
        enemy.rigidbody.AddForce((transform.position - enemy.transform.position).normalized * knockBackForce, ForceMode2D.Impulse);
        enemy.rigidbody.drag = 0.4f;

        enemyController.TakeDamage(2, transform.position);
    }

    #region"Collision handling"
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hidden)
            return;

        if (collision.collider.CompareTag("Enemy"))
        {
            if (isDashing)
            {
                PushEnemy(collision); 
            }
            else
            {
                DamageCheck(collision.transform);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("DialogueTrigger"))
        {
            inDialogue = false;
            Debug.Log("Dialogue area entered!");
        }

        if (collider.CompareTag("Unstable"))
        {
            print("Try Start Loop");

            StartCoroutine(collider.GetComponent<UnstableTile>().UnstableLoop());
        }

        if (collider.CompareTag("Exit"))
        {
            ExitLevel(collider.GetComponent<Path>());
        }

        if (collider.CompareTag("End"))
        {
            uiManager.GameComplete();
            data.Reset();
            SaveSystem.Save(data);
        }

        if (collider.CompareTag("Checkpoint"))
        {
            SetCheckPoint(collider.GetComponent<Checkpoint>().Check());
        }

        if (collider.CompareTag("Star"))
        {
            //print("Star collided!");

            AcquireStar(collider.GetComponent<HealthStar>().Acquire());
        }

        if (collider.CompareTag("ManaOrb"))
        {
            AcquireOrb(collider.GetComponent<ManaOrb>().Acquire());
        }

        if (collider.CompareTag("ManaDust"))
        {
            mana = uiManager.RecoverMana(1);
            AkSoundEngine.PostEvent(playManaCollectSound.Id, gameObject);
            Destroy(collider.gameObject);

        }

        if (hidden)
            return;

        if (collider.CompareTag("Enemy"))
        {

            DamageCheck(collider.transform);
        }
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        if (collider.CompareTag("DialogueTrigger"))
        {
            if (Input.GetKey("f") && inDialogue == false)
            {
                inDialogue = true;
                collider.GetComponent<DialogueTrigger>().TriggerDialogue();
               // Debug.Log("Dialogue started!");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.CompareTag("DialogueTrigger"))
        {
            inDialogue = false;
            Debug.Log("Dialogue area exit!");
        }
    }
    #endregion

    #region"Ability Acquisition"
    /// <summary>Use this method to enable doublejump and dive, when animal selection event is fired</summary>
    public void AcquireBirbAbilities()
    {
        birbAcquired = true;
        data.hasBirb = true;
    }

    /// <summary>Use this method to enable long jump and claws, when animal selection event is fired</summary>
    public void AcquireCatAbilities()
    {
        catAcquired = true;
        data.hasCat = true;
    }

    /// <summary>Use this method to enable dig and spikes, when animal selection event is fired</summary>
    public void AcquireMoleAbilities()
    {
        moleAcquired = true;
        data.hasMole = true;
    }   
    
    /// <summary>Use this method to enable dash/bash, when animal selection event is fired</summary>
    public void AcquireGoatAbilities()
    {
        goatAcquired = true;
        data.hasGoat = true;
    }

    /// <summary>Use this method to enable climb, when animal selection event is fired</summary>
    public void AcquireMonkeyAbilities()
    {
        monkeyAcquired = true;
        data.hasMonkey = true;
    }
    #endregion

    /// <summary>Allows the player to look up or down</summary>
    private void Look()
    {
        if (isLooking)
        {
            if (state == PlayerState.Idle)
            {
                float vertical = data.keyManager.Vertical();

                if (vertical > .1f)
                {
                    cameraTarget.localPosition = new Vector2(0, Mathf.Lerp(cameraTarget.localPosition.y, 4, Time.deltaTime));
                    return;
                }
                if (vertical < -.1f)
                {
                    cameraTarget.localPosition = new Vector2(0, Mathf.Lerp(cameraTarget.localPosition.y, -4, Time.deltaTime));
                    return;
                }
            }

            //If not idle or not looking, return cameratarget to normal
            cameraTarget.localPosition = Vector2.Lerp(cameraTarget.localPosition, Vector2.zero, 4 * Time.deltaTime);
            if (cameraTarget.localPosition == Vector3.zero)
                isLooking = false;
            return;
        }

        else if (state == PlayerState.Idle)
        {
            float vertical = data.keyManager.Vertical();

            if (vertical > .1f)
            {
                cameraTarget.localPosition = new Vector2(0, Mathf.Lerp(cameraTarget.localPosition.y, 4, Time.deltaTime));
                isLooking = true;
                return;
            }
            if (vertical < -.1f)
            {
                cameraTarget.localPosition = new Vector2(0, Mathf.Lerp(cameraTarget.localPosition.y, -4, Time.deltaTime));
                isLooking = true;
                return;
            }
        }
    }

    /// <summary>
    /// Initializes the area around the player
    /// </summary>
    private void InitializeArea()
    {
        print("Initialize");
        //Spawn
        if (!PlayerPrefs.HasKey("Exit"))
        {
            //Check if level is correct
            if(data.checkPointLevelIndex != SceneManager.GetActiveScene().buildIndex)
            {
                //If level is not correct, change the level
                SceneManager.LoadScene(data.checkPointLevelIndex);
                return;
            }

            //Set Position to Checkpoint
            transform.position = Checkpoint.GetCheckPointPosition(data.checkPointIndex);
        }
        else
        {
            //Set Position to Entrance
            transform.position = entranceList[PlayerPrefs.GetInt("Exit")].position;
        }

        //Health
        health = uiManager.AddHealthStar(5 + data.starsAcquired.Count);
        if(PlayerPrefs.HasKey("Health"))
            health = PlayerPrefs.GetInt("Health");

        //Mana
        mana = uiManager.IncreaseManaLimit(5 * data.orbsAcquired.Count);
        if (PlayerPrefs.HasKey("Mana"))
            mana = PlayerPrefs.GetFloat("Mana");

        //Abilities
        catAcquired = data.hasCat;
        birbAcquired = data.hasBirb;
        moleAcquired = data.hasMole;
        goatAcquired = data.hasGoat;
        monkeyAcquired = data.hasMonkey;

        PlayerPrefs.DeleteAll();
    }

    /// <summary>Sets the checkPoint number</summary>
    /// <param name="checkPointNumber"></param>
    private void SetCheckPoint(int checkPointIndex)
    {
        if (!data.checkPointsUnlocked.Contains(checkPointIndex))
            data.checkPointsUnlocked.Add(checkPointIndex);
        data.checkPointIndex = checkPointIndex;
        data.checkPointLevelIndex = SceneManager.GetActiveScene().buildIndex;
        health = uiManager.RecoverHealth();
        mana = uiManager.RecoverMana(100);
        SaveSystem.Save(data);
    }

    /// <summary>Acquires a health star</summary>
    /// <param name="starIndex">The index number of the star</param>
    private void AcquireStar(int starIndex)
    {
        if (!data.starsAcquired.Contains(starIndex))
            data.starsAcquired.Add(starIndex);
        health = uiManager.AddHealthStar(5 + data.starsAcquired.Count);
        SaveSystem.Save(data);
    }

    /// <summary>Acquires a health star</summary>
    /// <param name="starIndex">The index number of the star</param>
    private void AcquireOrb(int orbIndex)
    {
        if (!data.orbsAcquired.Contains(orbIndex))
            data.orbsAcquired.Add(orbIndex);
        mana = uiManager.IncreaseManaLimit(5);
        AkSoundEngine.PostEvent(playBigManaCollect.Id, gameObject);
        SaveSystem.Save(data);
    }

    /// <summary>Exits to another level</summary>
    /// <param name="index">Provides the values to determine the next level's entrance</param>
    private void ExitLevel(Path path)
    {
        PlayerPrefs.SetInt("Exit", path.indexNumber);
        PlayerPrefs.SetFloat("Mana", mana);
        PlayerPrefs.SetInt("Health", health);
        SceneManager.LoadScene(path.nextSceneIndex);

    }

    /// <summary>Checks if the any enemies were hit by the claw</summary>
    public void HitCheck() 
    {
        List<Collider2D> hitEnemies = new List<Collider2D>();
        clawRange.OverlapCollider(enemies, hitEnemies);
        for (int i = 0; i < hitEnemies.Count; i++)
        {
            if(hitEnemies[i].TryGetComponent(out EnemyController enemy))
            {
                print(enemy.GetType());
                enemy.TakeDamage(attackPower, transform.position);
            }
            else if (hitEnemies[i].CompareTag("Gate"))
                Destroy(hitEnemies[i].gameObject.GetComponentInChildren<PolygonCollider2D>().gameObject);
        }
    }

    /// <summary>Checks how much damage to take</summary>
    public void DamageCheck(Transform enemyRange)
    {
        if (invulnerable)
            return;

        invulnerable = true;
        sRend.color = new Color(1, 1, 1, .5f);
        StartCoroutine(InvulnerabilityTimer());

        if (hidden)
        {
            anim.speed = 1;
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
        uiManager.RemoveHealth(health);
        if (health <= 0)
            state = PlayerState.Die;
        SetAnimation();
    }

    IEnumerator InvulnerabilityTimer()
    {
        yield return new WaitForSeconds(1);

        invulnerable = false;
        if (!hidden)
            sRend.color = Color.white;

    }

    /// <summary>Ends the animation and resets to Idle</summary>
    public void AnimationEnd()
    {
        if(state == PlayerState.Attack)
            return;
        state = PlayerState.Idle;
        SetAnimation();
    }
    
    /// <summary>Ends the animation and resets to Idle</summary>
    public void NextAttackCheck()
    {
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
        state = PlayerState.AttackEnd;
    }

    /// <summary>Activates the game over experience</summary>
    public void DeathEnd()
    {
        anim.updateMode = AnimatorUpdateMode.UnscaledTime;
        uiManager.GameOver();
    }

    #region"Jump Methods"
    /// <summary>Checks whether to double jump</summary>
    public void DoubleJumpCheck()
    {
        if (birbAcquired & !doubleJumped)
            if (Input.GetKeyDown(data.keyManager.keys[Key.Jump]))
            {
                if (mana >= 2)
                {
                    doubleJumped = true;
                    state = PlayerState.DoubleJump;
                    AkSoundEngine.PostEvent(jumpSound.Id, this.gameObject);
                    SetAnimation();
                }
            }
    }
    /// <summary>Checks whether to dive</summary>
    public void DiveCheck()
    {
        if (moleAcquired)
            if (Input.GetKeyDown(data.keyManager.keys[Key.Attack]))
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
    #endregion

    private IEnumerator Dash()
    {
        Vector2 originalVelocity = rb.velocity;

        canDash = false;
        isDashing = true;
        rb.gravityScale = 0;
        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(dashTime);

        isDashing = false;
        rb.gravityScale = normalGravity;
        rb.velocity = originalVelocity;
        state = PlayerState.Walking;
        afterImage.ActivateAfterImages(false);
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
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


    /// <summary>Use the specified amount of mana</summary>
    public void UseMana(int manaUsed)
    {
        mana = uiManager.RemoveMana(manaUsed);
        particles.Emit(manaUsed*10);
    }

    /// <summary>Checks whether to hide</summary>
    private bool HideCheck()
    {
        if (hidden)
        {
            if(mana <= 0)
            {
                anim.speed = 1;
                if (invulnerable)
                    sRend.color = new Color(1, 1, 1, .5f);
                else
                    sRend.color = Color.white;
                hidden = false;
                speed *= 2;
                Physics2D.IgnoreLayerCollision(3, 7, false);
                AkSoundEngine.PostEvent(unhideSound.Id, this.gameObject);
                OnHideEnd?.Invoke();
                particles.Stop();
            }
            mana = uiManager.RemoveMana(Time.deltaTime);
            if (Input.GetKeyUp(data.keyManager.keys[Key.Hide]))
            {
                anim.speed = 1;
                if (invulnerable)
                    sRend.color = new Color(1, 1, 1, .5f);
                else
                    sRend.color = Color.white;
                hidden = false;
                speed *= 2;
                Physics2D.IgnoreLayerCollision(3, 7, false);
                AkSoundEngine.PostEvent(unhideSound.Id, this.gameObject);
                OnHideEnd?.Invoke();
                particles.Stop();
            }
            return true;
        }
        else if (Input.GetKeyDown(data.keyManager.keys[Key.Hide]))
        {
            AkSoundEngine.PostEvent(hideSound.Id, this.gameObject);
            anim.speed = .5f;
            sRend.color = Color.black;
            hidden = true;
            speed /= 2;
            Physics2D.IgnoreLayerCollision(3, 7, true);
            particles.Play();
        }
        return false;
    }
    /// <summary>Checks whether to meditate</summary>
    private bool MeditateCheck()
    {
        if (meditating)
        {
            mana = uiManager.RecoverMana(Time.deltaTime);
            if (Input.GetKeyUp(data.keyManager.keys[Key.Meditate]))
            {
                anim.speed = 1;
                sRend.color = Color.white;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                meditating = false;
                //AkSoundEngine.PostEvent(unhideSound.Id, this.gameObject);
            }
            return true;
        }
        else if (Input.GetKeyDown(data.keyManager.keys[Key.Meditate]))
        {
            //AkSoundEngine.PostEvent(hideSound.Id, this.gameObject);
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
            sRend.color = Color.magenta;
            meditating = true;
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
    AttackEnd,
    Dive,
    Dash
}
