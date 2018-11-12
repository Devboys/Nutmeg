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
        if (targetToFollow != null && collision.GetComponent<Transform>() == targetToFollow)
        {
            targetToFollow = null;
            _rigidbody.velocity *= new Vector2(0, 1);
        }

    }

    private void Update()
    {
        if (targetToFollow != null)
        {
            _transform.LookAt(targetToFollow.position);
            transform.Rotate(new Vector3(0, -90, 0), Space.Self);

            if (Vector3.Distance(_transform.position, targetToFollow.position) > 0.05f);
            transform.Translate(new Vector3(aggroedMovespeed * Time.deltaTime, 0, 0));
        }
    }
}
