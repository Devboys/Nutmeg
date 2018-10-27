using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamBoundTriggerNew : MonoBehaviour {

    public delegate void boundDirectionDelegate(float? v);
    [HideInInspector] public boundDirectionDelegate D_SetXMin;
    [HideInInspector] public boundDirectionDelegate D_SetXMax;
    [HideInInspector] public boundDirectionDelegate D_SetYMin;
    [HideInInspector] public boundDirectionDelegate D_SetYMax;

    private Collider2D lastXBlocker;
    private Collider2D lastYBlocker;

    private void OnCollisionEnter2D(Collision2D c)
    {
        if (c.collider.gameObject.tag == "CameraBlock" && c.contactCount > 1)
        {
            //collision between two box-colliders will always(?) have 2 contact points.
            Vector3 p0 = c.GetContact(0).point;
            Vector3 p1 = c.GetContact(1).point;

            //if the two contact points x-coordinates are on the same line, then either the left or right part of the box-collider must have collided.
            if (p0.x == p1.x)
            {
                lastXBlocker = c.collider;
                float colliderSizeX = ((c.collider.GetComponent<BoxCollider2D>().size.x) / 2f);

                //determine if collision was left or right and set x bounds based on this.
                if (c.collider.transform.position.x < transform.position.x) //
                    D_SetXMin(c.collider.transform.position.x + colliderSizeX);
                else
                    D_SetXMax(c.collider.transform.position.x - colliderSizeX);
            }

            //if the two contact points y-coordinates are on the same line, then either the top or bottom part of the box-collider must have collided.
            if (p0.y == p1.y)
            {
                lastYBlocker = c.collider;
                float colliderSizeY = ((c.collider.GetComponent<BoxCollider2D>().size.y) / 2f);

                //determine if collision was top or bottom and set bounds depending on this.
                if (c.collider.transform.position.y < transform.position.y)
                    D_SetYMin(c.collider.transform.position.y + colliderSizeY);
                else
                    D_SetYMax(c.collider.transform.position.y - colliderSizeY);
            }
        }
    }

    private void OnCollisionExit2D(Collision2D c)
    {
        if (c.collider.gameObject.tag == "CameraBlock")
        {
            if (c.collider == lastXBlocker)
            {
                D_SetXMin(null);
                D_SetXMax(null);
                lastXBlocker = null;
            }

            else if (c.collider == lastYBlocker)
            {
                D_SetYMin(null);
                D_SetYMax(null);
                lastYBlocker = null;
            }
        }
    }
}
