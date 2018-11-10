using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRoamerScript : MonoBehaviour {

    [SerializeField] private LayerMask whatIsGround;

    [SerializeField] private float moveSpeed = 1.5f;

    private bool movingRight;

    //components
    private Rigidbody2D _rigidbody2D;
    private Transform _transform;

    private void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _transform = GetComponent<Transform>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {

        //check if collider's layer is in the layermask
        bool layerIsInMask = (whatIsGround == (whatIsGround | (1 << collision.collider.gameObject.layer)));
        if (layerIsInMask)
        {
            Vector3 p0 = collision.GetContact(0).point;
            Vector3 p1 = collision.GetContact(1).point;

            

            //if the two contact points x-coordinates are on the same line, then either the left or right part of the box-collider must have collided.
            if (p0.x == p1.x)
            {
                //The roamer should only flip if it meets an obstacle in it's current trajectory, not if it was collided from behind.
                float dist = collision.collider.GetComponent<Transform>().position.x - _transform.position.x;

                if (dist < 0 && !movingRight)
                    Flip();
                else if (dist > 0 && movingRight)
                    Flip();
            }
        }
    }

    private void FixedUpdate()
    {
        if (movingRight)
            _rigidbody2D.velocity = new Vector2(moveSpeed, 0);
        else
            _rigidbody2D.velocity = new Vector2(-moveSpeed, 0);



    }

    private void Flip()
    {
        //flip transform;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;


        //flip movement;
        movingRight = !movingRight;
    }
}
