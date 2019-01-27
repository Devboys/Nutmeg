using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

[RequireComponent(typeof(CharacterMover))]
public class CharacterController : MonoBehaviour {

    #region In-Editor Variables
    [Header("Running")]
    [SerializeField] private float moveSpeed = 8f;

    [Header("Gravity & Damping")]
    [SerializeField] private float gravity = -25f;
    [SerializeField] private float maxGravity = -25f;
    [SerializeField] private float groundDamping = 20f;
    [SerializeField] private float inAirDamping = 5f;

    [Header("Jump")]
    [SerializeField] private float maxJumpHeight = 3f;
    [SerializeField] private float minJumpHeight = 0.5f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 25f;
    [SerializeField] private float dashDuration = 0.5f;
    

    [Header("Wall Slide")]
    [SerializeField] [Range(0f, 1f)] private float slideGravityDamping = 0;
    [SerializeField] private float slideJumpHeight = 0.3f;
    [SerializeField] private float slideJumpLength = 5f;
    [SerializeField] private float wallJumpDuration = 0.2f;
    [SerializeField] private float maxSlideGravity = -5f;

    [Header("Damage Knockback")]
    [SerializeField] private float knockHeight = 1f;
    [SerializeField] private float knockbackDuration = 0.5f;
    [SerializeField] private float knockbackSpeed = 1f;

    [Header("Abilities")]
    [SerializeField] private bool doubleJump;
    [SerializeField] private bool wallJump;
    [SerializeField] private bool dash;

    #endregion

    //movement variables
    private Vector3 _velocity;
    private float horizontalMove;
    private bool _goingRight
    {
        get { return horizontalMove > 0; }
    }
    private bool _goingLeft
    {
        get { return horizontalMove < 0;  }
    }


    private bool facingRight;

    //Knockback variables
    private float knockTimer;
    private float currentKnockDuration;

    //Player state
    [Header("State Variables (Read Only)")]
    [SerializeField][ReadOnly] private bool isSliding;
    [SerializeField][ReadOnly] private bool hasDoubleJumped;
    [SerializeField][ReadOnly] private bool hasDashed;

    [Header("Knockback State(Read Only")]
    [SerializeField][ReadOnly] private bool inDashKnockback;
    [SerializeField][ReadOnly] private bool inDamageKnockback;
    [SerializeField][ReadOnly] private bool inWallJumpKnockback;

    private bool inAnyKnockback
    {
        get { return inDamageKnockback || inWallJumpKnockback || inDashKnockback; }
    }

    //Component Cache
    private CharacterMover _mover;
    private PlayerHealthHandler _healthHandler;

    #region Movement Events
    public event Action OnRunStartEvent;
    public event Action OnRunEndEvent;
    public event Action OnJumpEvent;
    public event Action OnDoubleJumpEvent;
    public event Action OnWallJumpEvent;
    public event Action OnLandEvent;
    public event Action OnSlideEvent;
    #endregion

    private void Start()
    {
        _mover = this.GetComponent<CharacterMover>();
        _healthHandler = this.GetComponent<PlayerHealthHandler>();
    }

    private void Update()
    {
        bool wasRunningLastFrame = (horizontalMove != 0);

        //velocity is nulled before gravity to maintain grounded status for each frame. If check is placed after gravity, unexpected jumping behaviour occurs in game.
        if (_mover.IsGrounded && !inAnyKnockback)
            _velocity.y = 0;

        HandleGravity();
        HandleHorizontalMovement();

        HandleDash();

        //jump handled after gravity increment because jump calculation expects this.
        if (!inDashKnockback)
        {
            HandleJump();
        }

        //pass the velocity, adjusted for deltaTime, to the mover for collision detection and other physics interactions.
        _mover.Move(_velocity * Time.deltaTime);

        //apply changes to velocity
        _velocity = _mover.velocity;

        //update state after movement so that we can use post-movement collision state.
        HandleStateUpdate(wasRunningLastFrame);
    }

    public void BeginDamageKnockback(Vector2 damageSourcePosition)
    {
        inDamageKnockback= true;
        inDashKnockback = inWallJumpKnockback = false;

        bool knockRight = damageSourcePosition.x < transform.position.x;

        float knockY = Mathf.Sqrt(2f * knockHeight * -gravity);
        float knockX = knockRight ? knockbackSpeed : -knockbackSpeed;

        EnterUncontrollableState(knockbackDuration, new Vector2(knockX, knockY));
    }

    #region Control Handlers
    private void HandleHorizontalMovement()
    {
        if (!inAnyKnockback)
        {
            horizontalMove = Input.GetAxisRaw("Horizontal");

            if (horizontalMove < 0 && !facingRight)
                Flip();
            else if (horizontalMove > 0 && facingRight)
                Flip();

            //smooth horizontal movement. TODO: replace with smoothDamp.
            float dampingFactor = _mover.IsGrounded ? groundDamping : inAirDamping;
            _velocity.x = Mathf.Lerp(_velocity.x, horizontalMove * moveSpeed, Time.deltaTime * dampingFactor);
        }
        else
        {
            knockTimer += Time.deltaTime;

            if (knockTimer >= currentKnockDuration)
            {
                inDamageKnockback = inDashKnockback = inWallJumpKnockback = false;

                horizontalMove = Input.GetAxis("Horizontal");

                if(Math.Sign(horizontalMove) != Math.Sign(_velocity.x))
                {
                    _velocity.x = 0;
                }
                else
                {
                    _velocity.x = moveSpeed * horizontalMove;
                }
            }
        }
    }

    private void HandleDash()
    {
        if (Input.GetAxis("Dash") != 0 && !hasDashed && dash)
        {
            hasDashed = true;

            inDashKnockback = true;
            int sign = facingRight ? -1 : 1;

            float dashX = dashSpeed * sign;
            float dashY = 0;
            EnterUncontrollableState(dashDuration, new Vector2(dashX, dashY));
        }
    }

    private void HandleJump()
    {
        if (Input.GetButtonDown("Jump"))
        {
            //standard jump
            if (_mover.IsGrounded)
            {
                //formula derived from vf^2 = vi^2 + 2ad
                _velocity.y = Mathf.Sqrt(2f * maxJumpHeight * -gravity);

                if (OnJumpEvent != null)
                    OnJumpEvent();
            }
            //wall jump
            else if ((_mover.IsRightOfWall || _mover.IsLeftOfWall) && wallJump)
            {
                hasDoubleJumped = false;
                hasDashed = false;

                inWallJumpKnockback = true;
                int sign = (_mover.IsRightOfWall) ? -1 : 1;
                float wallJumpX = slideJumpLength * sign;
                float wallJumpY = Mathf.Sqrt(2f * slideJumpHeight * -gravity);

                EnterUncontrollableState(wallJumpDuration, new Vector2(wallJumpX, wallJumpY));

                if(OnWallJumpEvent != null)
                    OnWallJumpEvent();
            }
            //double jump
            else if (!hasDoubleJumped && doubleJump)
            {
                hasDoubleJumped = true;
                _velocity.y = Mathf.Sqrt(2f * maxJumpHeight * -gravity);

                if(OnDoubleJumpEvent != null)
                    OnDoubleJumpEvent();
            }
        }
        
        //if jump is cancelled, set player velocity to be equal to initial velocity as defined by minJumpHeight.
        if (Input.GetButtonUp("Jump")  && _velocity.y > Mathf.Sqrt(2f * minJumpHeight * -gravity))
        {
            _velocity.y = Mathf.Sqrt(2f * minJumpHeight * -gravity);
        }
    }

    private void HandleGravity()
    {
        float currentMaxGravity = isSliding ? maxSlideGravity : maxGravity;

        if (_velocity.y > currentMaxGravity && !inDashKnockback)
        {
            float yAccel = (isSliding && _velocity.y < 0) ? gravity * slideGravityDamping : gravity;
            _velocity.y += yAccel * Time.deltaTime;
        }

        if (_velocity.y < currentMaxGravity)
            _velocity.y = currentMaxGravity;

    }

    private void HandleStateUpdate(bool wasRunningLastFrame)
    {
        //Register Wall Slide
        if (!_mover.IsGrounded)
        {
            if ((_mover.CollidedLeft && _goingLeft) || (_mover.CollidedRight && _goingRight))
            {
                if (!isSliding)
                {
                    _velocity.y = (_velocity.y > maxGravity * slideGravityDamping) ? _velocity.y : maxGravity * slideGravityDamping;
                }
                isSliding = true;
            }

            else if (!_mover.HasCollidedHorizontal && isSliding)
            {

                isSliding = false;
            }
        }
        else if (isSliding)
        {
            isSliding = false;
        }

        if(inAnyKnockback && _mover.HasCollidedHorizontal)
        {
            inDashKnockback = false;
        }

        if (_mover.HasLanded)
        {
            hasDoubleJumped = false;
            hasDashed = false;
            isSliding = false;

            if (OnLandEvent != null)
                OnLandEvent();
        }

        //run-event should be called in the frame that movement goes from 0 to not-zero, as well as any frame where the player lands while moving.
        //TODO: rework sound system.
        if ((!wasRunningLastFrame || _mover.HasLanded) && horizontalMove != 0 && _mover.IsGrounded && !_mover.HasCollidedHorizontal)
            OnRunStartEvent();
        else if ((wasRunningLastFrame && horizontalMove == 0 && _mover.IsGrounded) || _mover.HasLeftGround || _mover.IsGrounded && _mover.HasCollidedHorizontal)
            OnRunEndEvent();
    }
    #endregion

    #region Internal Utility
    private void EnterUncontrollableState(float duration, Vector2 movementVector)
    {
        currentKnockDuration = duration;
        _velocity = movementVector;

        knockTimer = 0;
    }

    private void ResetKnockbackState()
    {
        inDashKnockback = inDamageKnockback = inWallJumpKnockback = false;
    }

    //flips player sprite.
    private void Flip()
    {
        //Switch the way the player is labelled as facing.
        facingRight = !facingRight;

        //Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
    #endregion

    private void OnTriggerEnter2D(Collider2D c)
    {
        if (c.gameObject.CompareTag("Pickup"))
        {
            string powerUp = c.GetComponent<I_PU_Base>().GetPowerUp();

            switch (powerUp)
            {
                case "Double Jump":
                    doubleJump = true;
                    break;
                case "Wall Jump":
                    wallJump = true;
                    break;
                case "Dash":
                    dash = true;
                    break;
            }

            c.gameObject.SetActive(false);

            Debug.Log("pickup!");
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
