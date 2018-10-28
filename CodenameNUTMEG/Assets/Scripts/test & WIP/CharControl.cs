using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D))]
public class CharControl : MonoBehaviour {

    //editor-set fields
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpHeight = 3f;
    [SerializeField] private float gravity = -25f;


    private CharacterController2D _mover;

    float horizontalMove;
    private Vector3 _velocity;

    private bool facingRight;

    private void Awake()
    {
        _mover = this.GetComponent<CharacterController2D>();
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

        //jump
        if (_mover.isGrounded && Input.GetButtonDown("Jump"))
            _velocity.y = Mathf.Sqrt(2f * jumpHeight * -gravity);


        if (_velocity.y > 0 & Input.GetButtonUp("Jump"))
        {
            _velocity.y = 0;
        }

        //smooth horizontal movement.
        _velocity.x = Mathf.Lerp(_velocity.x, horizontalMove * moveSpeed, Time.deltaTime * 20f);

        //Apply gravity
        _velocity.y += gravity * Time.deltaTime;

        //pass the velocity, adjusted for deltaTime, to the mover for collision detection and other physics interactions.
        _mover.move(_velocity * Time.deltaTime);

        _velocity = _mover.velocity;
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
