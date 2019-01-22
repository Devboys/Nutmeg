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

    [Header("Abilities")]
    [SerializeField] private bool doubleJump;
    [SerializeField] private bool wallJump;

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
    private bool inKnockback;
    private bool knockRight;

    //Player state
    [SerializeField][ReadOnly] private bool isSliding;
    [SerializeField][ReadOnly] private bool hasDoubleJumped;

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
        if (_mover.IsGrounded && !inKnockback)
            _velocity.y = 0;

        HandleGravity();
        HandleHorizontalMovement();

        //jump handled after gravity increment because jump calculation expects this.
        HandleJump();

        //pass the velocity, adjusted for deltaTime, to the mover for collision detection and other physics interactions.
        _mover.Move(_velocity * Time.deltaTime);

        //apply changes to velocity
        _velocity = _mover.velocity;

        //we update state after movement so that we can use post-movement collision state.
        HandleStateUpdate(wasRunningLastFrame);
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

    #region Control Handlers
    private void HandleHorizontalMovement()
    {
        if (!inKnockback)
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
            //if (linearKnock)
            //{
            //    _velocity = new Vector2(knockRight ? knockbackSpeed : -knockbackSpeed, knockHeight);
            //}

            knockTimer += Time.deltaTime;

            if (knockTimer >= knockbackTime)
            {
                inKnockback = false;
                knockTimer = 0;
            }
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
            else if (_mover.HasCollidedHorizontal && wallJump)
            {
                int sign = (_mover.CollidedLeft) ? 1 : -1;
                _velocity.x = slideJumpLength * sign;
                _velocity.y = Mathf.Sqrt(2f * slideJumpHeight * -gravity);
                inKnockback = true;

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
        if (_velocity.y > maxGravity)
        {
            float yAccel = (isSliding && _velocity.y < 0) ? gravity * slideGravityDamping : gravity;
            _velocity.y += yAccel * Time.deltaTime;
        }

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

            else if (!_mover.HasCollidedHorizontal)
            {
                isSliding = false;
            }
        }
        else if (isSliding)
        {
            isSliding = false;
        }

        if (_mover.HasLanded)
        {
            hasDoubleJumped = false;
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
