using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

[RequireComponent(typeof(CharMover))]
public class CharControl : MonoBehaviour {

    [Header("Running")]
    [SerializeField] private float moveSpeed = 8f;

    [Header("Jumping")]
    [SerializeField] private float gravity = -25f;
    [SerializeField] private float maxJumpHeight = 3f;
    [SerializeField] private float minJumpHeight = 0.5f;
    [SerializeField] private float inAirDamping = 5f;
    [SerializeField] private float groundDamping = 20f;
    [SerializeField] private float maxGravity = -25f;

    [Header("Wall Slide")]
    [SerializeField] [Range(0f, 1f)] private float slideGravityDamping = 0;
    [SerializeField] private float slideJumpHeight = 0.3f;
    [SerializeField] private float slideJumpLength = 5f;


    [Header("Knockback")]
    [SerializeField] private float knockHeight = 1f;
    [SerializeField] private float knockbackTime = 0.5f;
    [SerializeField] private float knockbackSpeed = 1f;
    [SerializeField] private bool linearKnock;

    [Header("Modifiers")]
    public bool doubleJump;
    public bool wallJump;

    //movement variables
    private Vector3 _velocity;

    private bool facingRight;
    private float horizontalMove;

    private float knockTimer;
    private bool inKnockback;
    private bool knockRight;

    public bool doubleJumped;

    public bool isSliding;

    //component cache
    private CharMover _mover;
    private PlayerHealthHandler _healthHandler;

    //movement events.
    public event Action OnRunStartEvent;
    public event Action OnRunEndEvent;
    public event Action OnJumpEvent;
    public event Action OnLandEvent;

    private void Start()
    {
        _mover = this.GetComponent<CharMover>();
        _healthHandler = this.GetComponent<PlayerHealthHandler>();
    }

    private void Update()
    {
        bool wasRunningLastFrame = (horizontalMove != 0);

        //velocity is nulled before gravity to maintain grounded status for each frame. If check is placed after gravity, unexpected jumping behaviour occurs in game.
        if (_mover.IsGrounded && !inKnockback)
            _velocity.y = 0;

        if (_velocity.y < 0)
        {
            //Apply gravity
            if (_velocity.y > maxGravity)
            {
                float yAccel = gravity;

                if (isSliding) yAccel = yAccel * slideGravityDamping;

                _velocity.y += yAccel * Time.deltaTime;
            }
        }
        else if(_velocity.y > maxGravity)
        {
            _velocity.y += gravity * Time.deltaTime;
        }

        
        

        if (!inKnockback)
        {
            horizontalMove = Input.GetAxisRaw("Horizontal");

            if (horizontalMove < 0 && !facingRight)
                Flip();
            else if (horizontalMove > 0 && facingRight)
                Flip();

            //jump handled after gravity increment because jump calculation expects this (putting jump before gravity will lead to a slightly lower jump)
            HandleJump();

            //smooth horizontal movement. TODO: replace with smoothDamp.
            float dampingFactor = _mover.IsGrounded ? groundDamping : inAirDamping;
            _velocity.x = Mathf.Lerp(_velocity.x, horizontalMove * moveSpeed, Time.deltaTime * dampingFactor);
        }
        else
        {
            if (linearKnock)
            {
                _velocity = new Vector2(knockRight ? knockbackSpeed : -knockbackSpeed, knockHeight);
            }

            knockTimer += Time.deltaTime;

            if(knockTimer >= knockbackTime)
            {
                inKnockback = false;
                knockTimer = 0;
            }
        }

        //pass the velocity, adjusted for deltaTime, to the mover for collision detection and other physics interactions.
        _mover.Move(_velocity * Time.deltaTime, false);

        _velocity = _mover.velocity;

        //handle wall slide
        if (_mover.HasCollidedHorizontal && !_mover.IsGrounded && !isSliding)
        {
            isSliding = true;
            _velocity.y = (_velocity.y > maxGravity * slideGravityDamping) ? _velocity.y : maxGravity * slideGravityDamping;
        }
        else if(!_mover.HasCollidedHorizontal && !_mover.IsGrounded && isSliding)
        {
            isSliding = false;
        }

        //we call current-frame events after _mover.Move() so that we can use the post-movement collision state.
        if (_mover.HasLanded)
        {
            doubleJumped = false;
            isSliding = false;

            if(OnLandEvent != null)
                OnLandEvent();
        }

        //run-event should be called in the frame that movement goes from 0 to not-zero, as well as any frame where the player lands while moving.
        if ((!wasRunningLastFrame || _mover.HasLanded) && horizontalMove != 0 && _mover.IsGrounded && !_mover.HasCollidedHorizontal)
            OnRunStartEvent();
        else if ((wasRunningLastFrame && horizontalMove == 0 && _mover.IsGrounded) || _mover.HasLeftGround || _mover.IsGrounded && _mover.HasCollidedHorizontal)
            OnRunEndEvent();
    }

    public void BeginDamageKnockback(Vector2 damageSourcePosition)
    {
        inKnockback = true;
        knockRight = damageSourcePosition.x < transform.position.x;

        if (!linearKnock)
        {
            float knockY = Mathf.Sqrt(2f * knockHeight * -gravity);
            float knockX = knockRight ? knockbackSpeed : -knockbackSpeed;
            Vector2 knockbackVector = new Vector2(knockX, knockY);
            _velocity = knockbackVector;
        }
    }

    private void HandleJump()
    {
        //if jump was initiated, calculate initial velocity so that the jump-arc will apex at height defined by maxJumpHeight (not actually that precise, it seems)
        if (Input.GetButtonDown("Jump"))
        {
            if (_mover.IsGrounded)
            {
                //formula derived from vf^2 = vi^2 + 2ad
                _velocity.y = Mathf.Sqrt(2f * maxJumpHeight * -gravity);

                //call event
                if (OnJumpEvent != null)
                    OnJumpEvent();
            }
            else if(!isSliding)
            {
                if (!doubleJumped && doubleJump)
                {
                    doubleJumped = true;
                    _velocity.y = Mathf.Sqrt(2f * maxJumpHeight * -gravity);
                    OnJumpEvent();
                }
            }
            else if (isSliding)
            {
                _velocity.x = 1f;
                _velocity.y = Mathf.Sqrt(2f * slideJumpHeight * -gravity);
            }
        }
        
        
        //if jump is cancelled, set player velocity to be equal to initial velocity as defined by minJumpHeight.
        if (Input.GetButtonUp("Jump")  && _velocity.y > Mathf.Sqrt(2f * minJumpHeight * -gravity))
        {
            _velocity.y = Mathf.Sqrt(2f * minJumpHeight * -gravity);
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

    private void OnTriggerEnter2D(Collider2D c)
    {
        if (c.gameObject.CompareTag("Pickup"))
        {
            c.gameObject.SetActive(false);
            doubleJump = true;
            Debug.Log("Pickup!");
        }

        else if (c.gameObject.CompareTag("Checkpoint"))
        {
            _healthHandler.SetCheckpoint(c.transform);
        }
    }

    //TODO: Finish
    public void ResetPlayer()
    {
        _velocity = Vector3.zero;
    }
}
