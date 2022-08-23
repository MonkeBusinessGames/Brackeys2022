using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sRend;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator anim;
    [SerializeField] private float speed = 3;
    [SerializeField] private float idleTime = 1;
    [SerializeField] private float walkTime = 4;
    [SerializeField] private float knockBackForce = 5;
    [SerializeField] private float health = 10;
    [SerializeField] private float recoveryTime = .5f;
    private float timer;
    private EnemyState state;

    void Start()
    {
        state = EnemyState.Idle;
        timer = 0;
    }

    void Update()
    {
        switch (state)
        {
            case EnemyState.Idle:
                timer += Time.deltaTime;
                if(timer >= idleTime)
                {
                    state = EnemyState.Walking;
                    speed *= -1;
                    sRend.flipX = !sRend.flipX;
                    SetAnimation();
                    timer = 0;
                    rb.velocity = new Vector2(speed, rb.velocity.y);
                }
                break;
            case EnemyState.Walking:
                timer += Time.deltaTime;
                if (timer >= walkTime)
                {
                    state = EnemyState.Idle;
                    SetAnimation();
                    rb.velocity = new Vector2(0, rb.velocity.y);
                    timer = 0;
                }
                break;
            case EnemyState.Hit:
                timer += Time.deltaTime;
                if (timer >= recoveryTime)
                {
                    state = EnemyState.Idle;
                    SetAnimation();
                    timer = 0;
                }
                return;
                break;
        }
    }

    void FixedUpdate()
    {

    }

    public void TakeDamage(float damageDealt, Vector2 playerPosition)
    {
        state = EnemyState.Hit;
        rb.velocity = Vector2.zero;
        rb.AddForce(((Vector2)transform.position - playerPosition).normalized * knockBackForce, ForceMode2D.Impulse);
        health -= damageDealt;
        timer = 0;
        if(health <= 0)
            state = EnemyState.Die;
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
            case EnemyState.Hit:
                anim.SetInteger("State", 2);
                break;
/*            case EnemyState.JumpStart:
                anim.SetInteger("State", 3);
                break*/;
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

}

public enum EnemyState
{
    Idle,
    Walking,
    Hit,
    Die,
    Attack
}
