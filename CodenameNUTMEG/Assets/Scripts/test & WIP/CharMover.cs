using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharMover : MonoBehaviour {

    [SerializeField] private LayerMask groundMask;

    BoxCollider2D _boxCollider;

    RayCastOrigins rayOrigins;

    private float skinWidth = 0.02f; //how much raycastpoints should be indented into the collider.

    public void Awake()
    {
        _boxCollider = this.GetComponent<BoxCollider2D>();
        rayOrigins = new RayCastOrigins();
    }

    public void Move(Vector3 deltaMovement)
    {

        PrimeRayCastOrigins();
        if (deltaMovement.x != 0f)
            MoveHorizontal(ref deltaMovement);

        if (deltaMovement.y != 0f)
            MoveVertical(ref deltaMovement);

        deltaMovement.z = 0;

        transform.Translate(deltaMovement, Space.World);
    }

    private void MoveHorizontal(ref Vector3 deltaMovement)
    {
        bool isGoingRight = deltaMovement.x > 0;
        float rayDistance = Mathf.Abs(deltaMovement.x) + skinWidth;
        Vector2 rayDirection = isGoingRight ? Vector2.right : -Vector2.right;
        Vector2 initialRayOrigin = isGoingRight ? rayOrigins.bottomRight : rayOrigins.bottomLeft;

        Vector2 ray = initialRayOrigin;
        RaycastHit2D rayHit = Physics2D.Raycast(ray, rayDirection, rayDistance, groundMask);


        
        if (rayHit)
        {
            deltaMovement.x = rayHit.point.x - initialRayOrigin.x;
            rayDistance = Mathf.Abs(deltaMovement.x);

            if (isGoingRight)
            {
                deltaMovement.x -= skinWidth;
            }
            else
            {
                deltaMovement.x += skinWidth;
            }
        }



    }

    private void MoveVertical(ref Vector3 deltaMovement)
    {

    }

    private void PrimeRayCastOrigins()
    {
        Bounds modifiedBounds = _boxCollider.bounds;
        //shrink modified bounds by 1 skinwidth on each side.
        modifiedBounds.Expand(-2f* skinWidth);

        //every raycast origin lies on the modified bounds.
        rayOrigins.topLeft = new Vector2(modifiedBounds.min.x, modifiedBounds.max.y);
        rayOrigins.topRight = new Vector2(modifiedBounds.max.x, modifiedBounds.max.y);
        rayOrigins.bottomLeft = new Vector2(modifiedBounds.min.x, modifiedBounds.min.y);
        rayOrigins.bottomRight = new Vector2(modifiedBounds.max.x, modifiedBounds.min.y);
    }

    #region inner classes
    struct RayCastOrigins
    {
        public Vector2 topLeft;
        public Vector2 topRight;
        public Vector2 bottomRight;
        public Vector2 bottomLeft;
    }
    #endregion
}
