﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharMover : MonoBehaviour {

    [SerializeField] private LayerMask groundMask; //which layers count should be collided with.
    [SerializeField] private int numHorizontalRays = 3;
    [SerializeField] private int numVerticalRays = 3;
    [SerializeField] public float skinWidth = 0.02f;

    private BoxCollider2D _boxCollider;

    //ray variables
    private RayCastOrigins rayOrigins;
    private float rayHeight;
    private float rayWidth;

    [HideInInspector] public Vector2 velocity;
    [HideInInspector] public bool isGrounded;

    public void Awake()
    {
        _boxCollider = this.GetComponent<BoxCollider2D>();
        rayOrigins = new RayCastOrigins();

        RecalculateDistanceBetweenRays();
    }

    public void Move(Vector3 deltaMovement, bool isJumping)
    {
        isGrounded = false;

        PrimeRayCastOrigins();
        if (deltaMovement.x != 0f)
            MoveHorizontal(ref deltaMovement);

        if (deltaMovement.y != 0f)
            MoveVertical(ref deltaMovement);

        deltaMovement.z = 0;

        transform.Translate(deltaMovement, Space.World);

        velocity = deltaMovement / Time.deltaTime;
    }

    #region secondary movement methods
    private void MoveHorizontal(ref Vector3 deltaMovement)
    {
        bool isGoingRight = deltaMovement.x > 0;
        float rayDistance = Mathf.Abs(deltaMovement.x) + skinWidth;
        Vector2 rayDirection = isGoingRight ? Vector2.right : Vector2.left;
        Vector2 initialRayOrigin = isGoingRight ? rayOrigins.bottomRight : rayOrigins.bottomLeft;

        for (int i = 0; i < numVerticalRays; i++)
        {
            Vector2 ray = initialRayOrigin;
            ray.y += rayHeight * i;

            RaycastHit2D rayHit = Physics2D.Raycast(ray, rayDirection, rayDistance, groundMask);

            if (rayHit)
            {
                deltaMovement.x = rayHit.point.x - ray.x;
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
    }

    private void MoveVertical(ref Vector3 deltaMovement)
    {
        bool isGoingUp = deltaMovement.y > 0;
        float rayDistance = Mathf.Abs(deltaMovement.y) + skinWidth;
        Vector2 rayDirection = isGoingUp ? Vector2.up : Vector2.down;
        Vector2 initialRayOrigin = isGoingUp ? rayOrigins.topLeft : rayOrigins.bottomLeft;

        initialRayOrigin.x += deltaMovement.x;

        for (int i = 0; i < numHorizontalRays; i++)
        {
            Vector2 ray = initialRayOrigin;
            ray.x += rayWidth * i;

            RaycastHit2D rayHit = Physics2D.Raycast(ray, rayDirection, rayDistance, groundMask);

            if (rayHit)
            {
                deltaMovement.y = rayHit.point.y - ray.y;
                rayDistance = Mathf.Abs(deltaMovement.y);

                if (isGoingUp)
                {
                    deltaMovement.y -= skinWidth;
                }
                else
                {
                    deltaMovement.y += skinWidth;
                    isGrounded = true;
                }

            }
        }
    }
    #endregion

    #region utility methods
    private void RecalculateDistanceBetweenRays()
    {
        float colliderUsableWidth = _boxCollider.size.x * Mathf.Abs(transform.localScale.x) - (2f * skinWidth);
        rayWidth = colliderUsableWidth / (numHorizontalRays - 1);

        float colliderUsableHeight = _boxCollider.size.y * Mathf.Abs(transform.localScale.y) - (2f * skinWidth);
        rayHeight = colliderUsableHeight / (numVerticalRays - 1);
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
    #endregion

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