using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoamerEnemyScript : MonoBehaviour
{

    public float speed;
    [Range(-0.5f, 0.5f)] public float edgeCheckOffsetX;
    public LayerMask groundMask;

    [ReadOnly] private bool goingRight = true;


    //component cache
    Rigidbody2D rb;
    BoxCollider2D col;

    private void Start()
    {
        rb = this.GetComponent<Rigidbody2D>();
        col = this.GetComponent<BoxCollider2D>();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        
        int sign = goingRight ? 1 : -1 ;
        Vector3 edgeCheckPos = new Vector3(sign * (edgeCheckOffsetX + this.GetComponent<BoxCollider2D>().bounds.extents.x), 0, 0);
        edgeCheckPos += transform.position;
        Gizmos.DrawCube(edgeCheckPos, new Vector3(0.05f, 0.05f, 0.05f));
    }

    void Update()
    {
        CheckEdge();

        Vector3 direction = Vector3.right * speed * (goingRight ? 1 : -1) * Time.deltaTime;
        transform.Translate(direction);
    }

    private void CheckEdge()
    {
        int sign = goingRight ? 1 : -1 ;
        Vector2 rayOrigin = new Vector2(sign * (col.bounds.extents.x + edgeCheckOffsetX), - col.bounds.extents.y + 0.05f);
        rayOrigin += (Vector2) transform.position;

        //turn around if trying to walk over edge
        RaycastHit2D hitDown = Physics2D.Raycast(rayOrigin, Vector2.down, 0.1f, layerMask: groundMask);

        //turn around if trying to walk into wall/obstacle
        RaycastHit2D hitForward = Physics2D.Raycast(rayOrigin, Vector2.right * sign, 0.05f, layerMask: groundMask);

        if (!hitDown || hitForward)
        {
            goingRight = !goingRight;
            Flip();
        }
    }

    private void Flip()
    {
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
}
