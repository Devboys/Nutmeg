using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

[RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D))]
public class CharControl : MonoBehaviour {

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float maxJumpHeight = 3f;
    [SerializeField] private float minJumpHeight = 0.5f;
    [SerializeField] private float gravity = -25f;

    [SerializeField] private float inAirDamping = 5f;
    [SerializeField] private float groundDamping = 20f;

    //movement variables
    private Vector3 _velocity;
    private bool facingRight;
    private float horizontalMove;

    //component cache
    private CharMover _mover;

    //movement events.
    public event Action OnMoveStartEvent; //yet to be implemented. Requires "was-moving-last-frame" indicator.
    public event Action OnMoveEndEvent; //yet to be implemented. Requires "was-moving-last-frame" indicator.
    public event Action OnJumpEvent;
    public event Action OnLandEvent; //yet to be implemented. Requires "was-in-air-last-frame" indicator.

    private void Start()
    {
        _mover = this.GetComponent<CharMover>();
    }

    private void Update()
    {

        if (_mover.isGrounded)
            _velocity.y = 0;

        horizontalMove = Input.GetAxisRaw("Horizontal");

        if (horizontalMove < 0 && !facingRight)
            Flip();
        else if (horizontalMove > 0 && facingRight)
            Flip();

        //Apply gravity
        _velocity.y += gravity * Time.deltaTime;

        HandleJump();

        //smooth horizontal movement. TODO: replace with smoothDamp TODO: apply horizontal in-air damping.
        float dampingFactor = _mover.isGrounded ? groundDamping : inAirDamping;
        _velocity.x = Mathf.Lerp(_velocity.x, horizontalMove * moveSpeed, Time.deltaTime * dampingFactor);

        //pass the velocity, adjusted for deltaTime, to the mover for collision detection and other physics interactions.
        _mover.Move(_velocity * Time.deltaTime, false);

        _velocity = _mover.velocity;

    }

    private void HandleJump()
    {

        //if jump was initiated, calculate initial velocity so that the jump-arc will apex at height defined by maxJumpHeight;
        if (Input.GetButtonDown("Jump") && _mover.isGrounded)
        {
            //formula derived from vf^2 = vi^2 + 2ad
            _velocity.y = Mathf.Sqrt(2f * maxJumpHeight * -gravity);

            //call event
            OnJumpEvent();
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
}
