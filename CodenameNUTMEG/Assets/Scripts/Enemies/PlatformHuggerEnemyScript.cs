using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlatformHuggerEnemyScript : MonoBehaviour
{
    public float speed;
    [Range(-0.5f, 0.5f)] public float edgeCheckOffset;
    public LayerMask groundMask; //grid can contain more than one tilemap on different layers, so groundmask is separate.
    public Tilemap groundTilemap;

    [SerializeField][ReadOnly] private bool goingRight = true;
    [SerializeField][ReadOnly] private bool goingHorizontal = true;

    private Vector2 currentDirection = new Vector2(1, 0);
    private Vector2 orthogVector;

    //component cache
    Rigidbody2D rb;
    BoxCollider2D col;
    private Grid groundGrid;

    private void Start()
    {
        rb = this.GetComponent<Rigidbody2D>();
        col = this.GetComponent<BoxCollider2D>();
        groundGrid = groundTilemap.layoutGrid;
    }

    void Update()
    {
        CheckEdge();

        Vector3 direction = currentDirection * speed * Time.deltaTime;
        transform.Translate(direction, Space.World);
    }

    private void CheckEdge()
    {
        int sign = goingRight ? 1 : -1;

        //rotate also flips bounds, so we must account for this.
        float boundX = goingHorizontal ? col.bounds.extents.x : col.bounds.extents.y;
        float boundY = goingHorizontal ? col.bounds.extents.y : col.bounds.extents.x;

        //Vector2 rayOrigin = new Vector2(sign * (col.bounds.extents.x + edgeCheckOffset), -col.bounds.extents.y + 0.05f);
        Vector2 rayOrigin = (Vector2)transform.position + (currentDirection * boundX);

        if (goingRight) {
            orthogVector = new Vector2(currentDirection.y, -currentDirection.x);
        }
        else
        {
            orthogVector = new Vector2(-currentDirection.y, currentDirection.x);
        }

        //debug
        Debug.DrawRay(rayOrigin, orthogVector, Color.red);
        Debug.DrawRay(rayOrigin, currentDirection, Color.white);

        //turn around if trying to walk over edge
        RaycastHit2D hitDown = Physics2D.Raycast(rayOrigin, orthogVector, boundY + 0.1f, layerMask: groundMask);

        //turn around if trying to walk into wall/obstacle
        RaycastHit2D hitForward = Physics2D.Raycast(rayOrigin, currentDirection * sign, 0.05f, layerMask: groundMask);

        if (hitForward)
        {
            goingRight = !goingRight;
            currentDirection = -currentDirection;
            Flip();
        }
        else if (!hitDown)
        {
            //query grid for new edge to walk on.
            Vector3Int cellBelow = groundGrid.WorldToCell(transform.position + (Vector3)orthogVector);
            
            if (groundTilemap.GetTile(cellBelow) != null)
            {
                Vector3 pos = new Vector3 (0, 0, 0);
                pos.x = (currentDirection.x == 0) ? transform.position.x + (edgeCheckOffset * orthogVector.x) : groundGrid.GetCellCenterWorld(cellBelow).x + (groundGrid.cellSize.x / 2 + boundY) *  currentDirection.x;
                pos.y = (currentDirection.x == 0) ? groundGrid.GetCellCenterWorld(cellBelow).y + (groundGrid.cellSize.y /2 + boundY) * currentDirection.y : transform.position.y + (edgeCheckOffset * orthogVector.y);
                transform.position = pos;

                Quaternion fromAngle = transform.rotation;
                Quaternion toAngle = Quaternion.Euler(transform.eulerAngles + new Vector3(0, 0, goingRight ? -90 : 90));
                transform.rotation = toAngle;

                goingHorizontal = !goingHorizontal;

                currentDirection = orthogVector;
            }

        }
    }

    private void Flip()
    {
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
}
