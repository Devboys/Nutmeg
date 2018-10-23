using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamBoundTrigger : MonoBehaviour {

    public delegate void blockDirectionDelegate(bool b);
    public blockDirectionDelegate XblockDelegate;
    public blockDirectionDelegate YblockDelegate;

    private Collider2D lastXBlocker;
    private Collider2D lastYBlocker;

    private void OnCollisionEnter2D(Collision2D c)
    {
        if (c.collider.gameObject.tag == "CameraBlock")
        {
            //collision between two box-colliders will have 2 contact points.
            if (c.contactCount > 1)
            {
                Vector3 p0 = c.GetContact(0).point;
                Vector3 p1 = c.GetContact(1).point;

                //float middleX = p0.x + (p1.x - p0.x) / 2;
                //float middleY = p0.y + (p1.y - p0.y) / 2;

                //if the two contact points x-coordinate are on the same line, then either the top or bottom part of the box-collider must have collided.
                if(p0.x == p1.x)
                {
                    XblockDelegate(true);
                    lastXBlocker = c.collider;
                }

                //if the two contact points y-coordinate are on the same line, then either the left or right part of the box-collider must have collided.
                if (p0.y == p1.y)
                {
                    YblockDelegate(true);
                    lastYBlocker = c.collider;
                }
                
            }
        }
    }

    private void OnCollisionExit2D(Collision2D c)
    {
        if (c.collider.gameObject.tag == "CameraBlock")
        {
            //collision-exits have no contactPoints, so we must use the stored colliders to determine which axis to unlock.
            if (c.collider == lastXBlocker)
            {
                XblockDelegate(false);
                lastXBlocker = null;
            }
            if(c.collider == lastYBlocker)
            {
                YblockDelegate(false);
                lastYBlocker = null;
            }
        }
    }
}
