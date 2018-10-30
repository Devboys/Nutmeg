using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

[RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D))]
public class CharControl : MonoBehaviour {

    //editor fields
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpHeight = 3f;
    [SerializeField] private float gravity = -25f;
    [SerializeField] private float jumpTime = 1f; //unused
    [SerializeField] private float minimumJumpTime = 0.1f;

    [SerializeField] private float inAirDamping = 5f;
    [SerializeField] private float groundDamping = 20f;

    private CharMover _mover;

    private Vector3 _velocity;
    private bool facingRight;

    //movement variables
    private float horizontalMove;

    private bool jump;
    private float currJumpTimer;
    private float currJumpSpeed;
    private bool lastJumpFrame;

    private bool jumpCancelled;
    private float totalJumpTime;

    //debug
    private float jumpInitHeight;
    private float jumpMaxHeight = 0;

    private void Awake()
    {
        _mover = this.GetComponent<CharMover>();
    }

    private void Update()
    {
        jump = false;

        if (_mover.isGrounded)
            _velocity.y = 0;

        horizontalMove = Input.GetAxisRaw("Horizontal");

        if (horizontalMove < 0 && !facingRight)
            Flip();
        else if (horizontalMove > 0 && facingRight)
            Flip();

        //jump
        if (_mover.isGrounded && Input.GetButtonDown("Jump"))
            jump = true;


        //Apply gravity
        _velocity.y += gravity * Time.deltaTime;

        HandleJump(jump);


        //smooth horizontal movement. TODO: replace with smoothDamp TODO: apply horizontal in-air damping.
        float dampingFactor = _mover.isGrounded ? groundDamping : inAirDamping;
        _velocity.x = Mathf.Lerp(_velocity.x, horizontalMove * moveSpeed, Time.deltaTime * dampingFactor);



        //pass the velocity, adjusted for deltaTime, to the mover for collision detection and other physics interactions.
        _mover.Move(_velocity * Time.deltaTime, false);
    }


    //Standard jump with minimum jump-height. Cancel still needs to be made accurate.
    private void HandleJump(bool jump)
    {
        //make jump cancelable
        if (_velocity.y > 0 & Input.GetButtonUp("Jump"))
        {
            jumpCancelled = true;
        }

        if (currJumpTimer > 0)
        {
            //Only stop jump if minimum jump time has passed.
            if (jumpCancelled && (totalJumpTime - currJumpTimer) >= minimumJumpTime)
            {
                _velocity.y = 0;
                currJumpTimer = 0;
                jumpCancelled = false;
            }

            currJumpTimer -= Time.deltaTime;

        }

        if (jump)
        {
            //set upward speed to cancel out with gravity at jumpHeight;
            _velocity.y = Mathf.Sqrt(2f * jumpHeight * -gravity);

            //calculate the total time it will take for the jump to reach it's peak, and set the jumpTimer accordingly.
            totalJumpTime = (_velocity.y) / Mathf.Abs(gravity);
            currJumpTimer = totalJumpTime;
        }
    }

    //Half-finished per-frame controlled jump method
    private void HandleFramePerfectJump(bool jump)
    {

        if (jump)
        {
            currJumpTimer = jumpTime;
            currJumpSpeed = jumpHeight / jumpTime;

            jumpInitHeight = transform.position.y;
        }

        

        if(currJumpTimer > 0)
        {
            _velocity.y = ((currJumpTimer - Time.deltaTime) < 0) ? (currJumpSpeed * currJumpTimer) / Time.deltaTime : currJumpSpeed;
            currJumpTimer -= Time.deltaTime;

            if(currJumpTimer <= 0)
            {
                //Debug.Log(currJumpTimer);
                lastJumpFrame = true;
            }

        }

        else if (lastJumpFrame)
        {
            _velocity.y = 0;
            lastJumpFrame = false;
        }

        //debug
        float heightDif = transform.position.y - jumpInitHeight;

        if ((heightDif > jumpMaxHeight))
        {
            jumpMaxHeight = heightDif;
            Debug.Log(jumpMaxHeight);
        }

    }

    private void Flip()
    {
        //Switch the way the player is labelled as facing.
        facingRight = !facingRight;

        //Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
}
