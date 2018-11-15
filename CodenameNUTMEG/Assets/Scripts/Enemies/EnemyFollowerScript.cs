using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFollowerScript : MonoBehaviour {

    [SerializeField] private float aggroedMovespeed;
    [SerializeField] private LayerMask groundMask;

    //cached target
    public Transform targetToFollow = null;

    //component cache.
    private Transform _transform;
    private Rigidbody2D _rigidbody;

    //movement
    private bool goingRight;

    void Start()
    {
        _rigidbody = this.GetComponent<Rigidbody2D>();
        _transform = this.GetComponent<Transform>();
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Transform targetTransform = collision.GetComponent<Transform>();

            float rayDistance = Vector2.Distance(targetTransform.position, _transform.position);
            Vector2 rayDirection = (targetTransform.position - _transform.position);
            rayDirection.Normalize();

            //cast ray to player to check if player is standing behind some obstacle.
            RaycastHit2D rayHit = Physics2D.Raycast(_transform.position, rayDirection, rayDistance, groundMask);

            if(!rayHit) //only move toward players that are visible(no ray hits)
                targetToFollow = targetTransform;
        }
    }

    public void OnTriggerStay2D(Collider2D collision)
    {
        if (targetToFollow == null && collision.CompareTag("Player"))
        {
            Transform targetTransform = collision.GetComponent<Transform>();

            float rayDistance = Vector2.Distance(targetTransform.position, _transform.position);
            Vector2 rayDirection = (targetTransform.position - _transform.position);
            rayDirection.Normalize();

            //cast ray to player to check if player is standing behind some obstacle.
            RaycastHit2D rayHit = Physics2D.Raycast(_transform.position, rayDirection, rayDistance, groundMask);

            if (!rayHit) //only move toward players that are visible(no ray hits)
                targetToFollow = targetTransform;
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (targetToFollow != null && collision.GetComponent<Transform>() == targetToFollow)
        {
            targetToFollow = null;
            _rigidbody.velocity *= new Vector2(0, 1);
        }

    }

    private void FixedUpdate()
    {
        if (targetToFollow != null)
        {
            //_transform.LookAt(targetToFollow.position);
            //transform.Rotate(new Vector3(0, -90, 0), Space.Self);

            float distX = _transform.position.x - targetToFollow.position.x;

            if (distX > 0 && !goingRight)
            {
                goingRight = true;
                aggroedMovespeed *= -1;

            }
            if (distX < 0 && goingRight)
            {
                goingRight = false;
                aggroedMovespeed *= -1;
            }


            if (Mathf.Abs(distX) > 0.05f)
                transform.Translate(new Vector3(aggroedMovespeed*Time.fixedDeltaTime, 0, 0));

        }
    }
}
