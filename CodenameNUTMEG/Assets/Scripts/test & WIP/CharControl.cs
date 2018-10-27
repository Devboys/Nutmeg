using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D))]
public class CharControl : MonoBehaviour {

    //editor-set fields
    [SerializeField] private float moveSpeed = 0f;
    //[SerializeField] private float jumpHeight = 0f;
    //[SerializeField] private float gravity = 0f;
    //[SerializeField][Range(0, .3f)] private float movementSmoothing = .05f;	// How much to smooth out the movement

    //private fields, not accessible by editor.
    //private BoxCollider2D _collider;
    //private Rigidbody2D _rigidbody;
    private CharMover _mover;

    float horizontalMove;
   // bool jump;
    private Vector3 _velocity;

    private bool facingRight;

    private void Awake()
    {
        //_collider = this.GetComponent<BoxCollider2D>();
        //_rigidbody = this.GetComponent<Rigidbody2D>();
        _mover = this.GetComponent<CharMover>();
    }

    private void Update()
    {
        //jump = false;

        horizontalMove = Input.GetAxisRaw("Horizontal");

        if (horizontalMove < 0 && !facingRight)
            Flip();
        else if (horizontalMove > 0 && facingRight)
            Flip();



        //if (Input.GetButton("Jump"))
        //{
        //    _velocity.y = Mathf.Sqrt(2f * jumpHeight * -gravity);
        //}

        Vector3 smoothVelocity = Vector3.zero;

        //smooth horizontal movement.
       // Vector2 targetVelocity = new Vector2(horizontalMove * moveSpeed, _velocity.y);
        _velocity.x = Mathf.Lerp(_velocity.x, horizontalMove * moveSpeed, Time.deltaTime * 20f);
            //Vector3.SmoothDamp(_velocity, targetVelocity, ref smoothVelocity, movementSmoothing).x; //this might be problematic, dont know how it works.

        //apply gravity
        //_velocity.y += gravity * Time.deltaTime;

        //pass the velocity, adjusted for deltaTime, to the mover for collision detection and other physics interactions.
        _mover.Move(_velocity * Time.deltaTime);


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
