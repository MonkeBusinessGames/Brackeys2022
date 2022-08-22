using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    [SerializeField] private SpriteRenderer sRend;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float speed;
    [SerializeField] private float jumpForce;
    [SerializeField] private BoxCollider2D groundCheck;
    [SerializeField] private LayerMask ground;
    private float movementX;
    private bool flip;
    private JumpState jumpState;

    void Start()
    {
        jumpState = JumpState.Grounded;
        flip = false;
    }

    void Update()
    {
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

        //Get Jump Input
        switch (jumpState)
        {
            case JumpState.Grounded:
                //Start Jumping
                if (Input.GetButtonDown("Jump"))
                    jumpState = JumpState.JumpStart;
                break;
            case JumpState.JumpStart:
                break;
            case JumpState.Jumping:
                //Short Jump
                if (Input.GetButtonUp("Jump"))
                    jumpState = JumpState.JumpStop;
                break;
            case JumpState.JumpStop:
                break;
            case JumpState.Falling:
                break;
        }
            
    }

    void FixedUpdate()
    {

        //Set Velocity
        rb.velocity = new Vector2(movementX * speed, rb.velocity.y);

        //Handle Jump Inputs
        switch (jumpState)
        {
            case JumpState.Grounded:
                break;
            case JumpState.JumpStart:
                //Initiates Jump
                rb.AddForce(new Vector2(0, jumpForce));
                jumpState = JumpState.Jumping;
                break;
            case JumpState.Jumping:
                if (rb.velocity.y <= 0)
                    jumpState = JumpState.Falling;
                break;
            case JumpState.JumpStop:
                //Cuts Jump Short
                rb.velocity = new Vector2(movementX * speed, rb.velocity.y / 2);
                jumpState = JumpState.Falling;
                break;
            case JumpState.Falling:
                if(groundCheck.IsTouchingLayers(ground))
                    jumpState = JumpState.Grounded;
                break;
        }

    }
}

public enum JumpState
{
    Grounded,
    JumpStart,
    Jumping,
    JumpStop,
    Falling,
}
