using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sRend;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator anim;
    [SerializeField] private float speed;
    [SerializeField] private float idleTime;
    [SerializeField] private float walkTime;
    private float timer;
    private EnemyState enemyState;

    void Start()
    {
        enemyState = EnemyState.Idle;
        timer = 0;
    }

    void Update()
    {
        switch (enemyState)
        {
            case EnemyState.Idle:
                timer += Time.deltaTime;
                if(timer >= idleTime)
                {
                    enemyState = EnemyState.Walking;
                    speed *= -1;
                    sRend.flipX = !sRend.flipX;
                    anim.SetBool("Walking", true);
                    timer = 0;
                    rb.velocity = new Vector2(speed, rb.velocity.y);
                }
                break;
            case EnemyState.Walking:
                timer += Time.deltaTime;
                if (timer >= walkTime)
                {
                    enemyState = EnemyState.Idle;
                    anim.SetBool("Walking", false);
                    rb.velocity = new Vector2(0, rb.velocity.y);
                    timer = 0;
                }
                break;
        }
    }

    void FixedUpdate()
    {

    }
}

public enum EnemyState
{
    Idle,
    Walking
}
