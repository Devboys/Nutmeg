using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFollowerScript : MonoBehaviour {

    [SerializeField] private float aggroedMovespeed;

    //cached target
    Transform targetToFollow = null;

    //component cache.
    private Transform _transform;
    private Rigidbody2D _rigidbody;

    void Start()
    {
        _rigidbody = this.GetComponent<Rigidbody2D>();
        _transform = this.GetComponent<Transform>();
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
            targetToFollow = collision.GetComponent<Transform>();
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (targetToFollow != null && collision == targetToFollow)
        {
            targetToFollow = null;
            _rigidbody.velocity = Vector2.zero;
        }

    }

    private void FixedUpdate()
    {
        if (targetToFollow != null)
        {
            float distToTargetX = targetToFollow.position.x - _transform.position.x;
            Vector2 vectorToTarget = new Vector2(distToTargetX, 0);
            
            if(Mathf.Abs(distToTargetX) > 0.05)
            vectorToTarget.Normalize();

            vectorToTarget.x *= aggroedMovespeed;


            _rigidbody.velocity = vectorToTarget;
        }
    }
}
