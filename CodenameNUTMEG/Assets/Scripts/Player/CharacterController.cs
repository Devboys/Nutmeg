﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(CharacterMover))]
public class CharacterController : MonoBehaviour {

    #region In-Editor Variables

    [Header("Health")]
    public int initHealth;

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
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 0.4f;

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
    [SerializeField] private bool doubleJumpAcquired;
    [SerializeField] private bool wallJumpAcquired;
    [SerializeField] private bool dashAcquired;

    public Tilemap groundTilemap;

    //Player state
    [Header("Player State")]
    public PlayerState state;

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

    //Component Cache
    private CharacterMover _mover;
    private Animator _animator;

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
        _animator = this.GetComponent<Animator>();

        state = new PlayerState(initHealth, transform.position);

        //internal event subscribes
        OnJumpEvent += () => { _animator.SetBool("isJumping", true); }; //this is a lambda expression subscribe.
        OnLandEvent += () => { _animator.SetBool("isJumping", false); };
    }

    private void Update()
    {
        bool wasRunningLastFrame = (horizontalMove != 0);

        //velocity is nulled before gravity to maintain grounded status for each frame. If check is placed after gravity, unexpected jumping behaviour occurs in game.
        if (_mover.IsGrounded && !state.isReeling)
            _velocity.y = 0;

        HandleGravity();
        HandleHorizontalMovement();
        HandleDash();
        HandleJump(); //jump handled after gravity increment because jump calculation expects this.
        TickTimers(Time.deltaTime);

        //pass the velocity, adjusted for deltaTime, to the mover for collision detection and other physics interactions.
        _mover.Move(_velocity * Time.deltaTime);

        //apply changes to velocity
        _velocity = _mover.velocity;

        //update state after movement so that we can use post-movement collision state.
        HandleStateUpdate(wasRunningLastFrame);
    }

    #region Control Handlers
    private void HandleHorizontalMovement()
    {
        if (!state.isReeling)
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
    }

    private void HandleDash()
    {
        if (!state.isReeling)
        {
            if (Input.GetAxis("Dash") != 0 && !state.hasDashed && state.dashCooldownTimer <= 0 && dashAcquired)
            {
                state.hasDashed = true;

                int directionSign = facingRight ? -1 : 1;
                float dashX = dashSpeed * directionSign;
                float dashY = 0;

                state.dashCooldownTimer = dashCooldown;

                EnterReelingState(dashDuration, new Vector2(dashX, dashY), true);
            }
        }
    }

    private void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && !state.isReeling)
        {
            //standard jump
            if (_mover.IsGrounded)
            {
                //formula derived from vf^2 = vi^2 + 2ad
                _velocity.y = Mathf.Sqrt(2f * maxJumpHeight * -gravity);

                OnJumpEvent?.Invoke();
            }
            //wall jump
            else if ((_mover.IsRightOfWall || _mover.IsLeftOfWall) && wallJumpAcquired)
            {
                state.ResetAbilityUsage();

                int sign = (_mover.IsRightOfWall) ? -1 : 1;
                float wallJumpX = slideJumpLength * sign;
                float wallJumpY = Mathf.Sqrt(2f * slideJumpHeight * -gravity);

                EnterReelingState(wallJumpDuration, new Vector2(wallJumpX, wallJumpY), false);

                OnWallJumpEvent?.Invoke();
            }
            //double jump
            else if (!state.hasDoubleJumped && doubleJumpAcquired)
            {
                state.hasDoubleJumped = true;
                _velocity.y = Mathf.Sqrt(2f * maxJumpHeight * -gravity);

                OnDoubleJumpEvent?.Invoke();
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
        float currentMaxGravity = state.isSliding ? maxSlideGravity : maxGravity;

        if (_velocity.y > currentMaxGravity && !state.gravityDisabled)
        {
            float yAccel = (state.isSliding && _velocity.y < 0) ? gravity * slideGravityDamping : gravity;
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
                if (!state.isSliding)
                {
                    _velocity.y = (_velocity.y > maxGravity * slideGravityDamping) ? _velocity.y : maxGravity * slideGravityDamping;
                }
                state.isSliding = true;
            }

            else if (!_mover.HasCollidedHorizontal && state.isSliding)
            {
                state.isSliding = false;
            }
        }
        else if (state.isSliding)
        {
            state.isSliding = false;
        }

        if(state.isReeling && _mover.HasCollidedHorizontal)
        {
            state.isReeling = false;
            state.gravityDisabled = false;
        }

        if (_mover.HasLanded)
        {
            state.ResetAbilityUsage();
            state.isSliding = false;

            OnLandEvent?.Invoke();
        }

        //run-event should be called in the frame that movement goes from 0 to not-zero, as well as any frame where the player lands while moving.
        //TODO: rework sound system.
        if ((!wasRunningLastFrame || _mover.HasLanded) && horizontalMove != 0 && _mover.IsGrounded && !_mover.HasCollidedHorizontal)
        {
            OnRunStartEvent?.Invoke();
        }
        else if ((wasRunningLastFrame && horizontalMove == 0 && _mover.IsGrounded) || _mover.HasLeftGround || _mover.IsGrounded && _mover.HasCollidedHorizontal)
        {
            OnRunEndEvent?.Invoke();
        }
    }
    #endregion

    #region Timer methods
    private void TickTimers(float tickTime)
    {
        TickReelTimer(tickTime);
        TickDashCoolDown(tickTime);
    }

    private void TickReelTimer(float tickTime)
    {
        if (state.isReeling)
        {
            state.reelTimer -= tickTime;

            if (state.reelTimer <= 0)
            {
                state.ResetKnockbackState();

                horizontalMove = Input.GetAxis("Horizontal");

                if (Math.Sign(horizontalMove) != Math.Sign(_velocity.x))
                    _velocity.x = 0;

                else
                    _velocity.x = moveSpeed * horizontalMove;
            }
        }
    }

    private void TickDashCoolDown(float tickTime)
    {
        if(state.dashCooldownTimer > 0)
        {
            state.dashCooldownTimer -= tickTime;
        }
    }
    #endregion

    private void EnterReelingState(float duration, Vector2 movementVector, bool disableGravity)
    {
        state.isReeling = true;
        state.gravityDisabled = disableGravity;

        state.reelTimer = duration;
        _velocity = movementVector;
    }

    private void StartDamageKnockback(Vector2 direction)
    {
        //Vector2 knockbackVector = new Vector2(knockbackSpeed, Mathf.Sqrt(2f * knockHeight * -gravity));
        EnterReelingState(knockbackDuration, direction, false);
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

    private void OnTriggerEnter2D(Collider2D c)
    {
        if (c.gameObject.CompareTag("Pickup"))
        {
            string powerUp = c.GetComponent<I_PU_Base>().GetPowerUp();

            switch (powerUp)
            {
                case "Double Jump":
                    doubleJumpAcquired = true;
                    break;
                case "Wall Jump":
                    wallJumpAcquired = true;
                    break;
                case "Dash":
                    dashAcquired = true;
                    break;
            }

            c.gameObject.SetActive(false);
        }
    }

    public void ResetPlayer()
    {
        state.ResetAll();
        Grid groundGrid = groundTilemap.layoutGrid;
        Vector3Int cellIndex = groundGrid.WorldToCell(_mover.lastGroundedPosition);

        //check neighbouring tiles so we can place the player 1 tile away from potential ledge.
        bool leftTile  = (groundTilemap.GetTile(cellIndex + new Vector3Int(-1, -1, 0))) != null && (groundTilemap.GetTile(cellIndex + new Vector3Int(-1, 0, 0))) == null;
        bool rightTile = (groundTilemap.GetTile(cellIndex + new Vector3Int(+1, -1, 0))) != null && (groundTilemap.GetTile(cellIndex + new Vector3Int(+1, 0, 0))) == null;

        if (leftTile != rightTile)
        {
            if (rightTile) cellIndex = cellIndex + new Vector3Int(+1, 0, 0);
            if (leftTile)  cellIndex = cellIndex + new Vector3Int(-1, 0, 0);
        }

        transform.position = groundGrid.GetCellCenterWorld(cellIndex);
    }

    public void Die()
    {
        state.ResetAll(true);
        _velocity = Vector3.zero;
        transform.position = state.checkpoint;

    }

    public void ModHealth(int healthMod, Vector2? origin = null)
    {
        state.health += healthMod;

        if (state.health <= 0)
            Die();

        //we assume that only damage that causes knockback has an origin.
        else if (origin.HasValue)
        {
            Vector2 distance = (Vector3)origin - transform.position;
            Vector2 direction = distance / distance.magnitude;
            Vector2 knockbackVector = new Vector2(-knockbackSpeed * Mathf.Sign(direction.x), Mathf.Sqrt(2f * knockHeight * -gravity));
            StartDamageKnockback(knockbackVector);
        }
    }

    [System.Serializable]
    public class PlayerState
    {
        [Header("Health")]
        [ReadOnly] public int health;
        [HideInInspector] private int initHealth;

        [Header("Checkpoint")]
        [ReadOnly] public Vector2 checkpoint;

        [Header("Movement State (Read Only)")]
        [ReadOnly] public bool isSliding;
        [ReadOnly] public bool hasDoubleJumped;
        [ReadOnly] public bool hasDashed;

        [Header("Reeling State (Read Only")]
        [ReadOnly] public bool isReeling;
        [ReadOnly] public bool gravityDisabled;

        //Timers
        [HideInInspector] public float reelTimer;
        [HideInInspector] public float dashCooldownTimer;
        

        public PlayerState(int startHealth, Vector2 initCheckpoint)
        {
            health = initHealth = startHealth;
            checkpoint = initCheckpoint;
        }

        #region resets
        public void ResetKnockbackState()
        {
            isReeling = gravityDisabled = false;
        }

        public void ResetAbilityUsage()
        {
            hasDashed = hasDoubleJumped = false;
        }

        public void ResetTimers()
        {
            reelTimer = dashCooldownTimer = 0;
        }

        public void ResetAll(bool resetHealth = false)
        {
            ResetKnockbackState();
            ResetAbilityUsage();
            ResetTimers();

            isSliding = false;
            if(resetHealth) health = initHealth;
        }
        #endregion
    }
}
