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
            //collision between two box-colliders will always(?) have 2 contact points.
            if (c.contactCount > 1)
            {
                Vector3 p0 = c.GetContact(0).point;
                Vector3 p1 = c.GetContact(1).point;

                //if the two contact points x-coordinate are on the same line, then either the top or bottom part of the box-collider must have collided.
                if(p0.x == p1.x)
                {
                    //call delegate to block x-axis and record collider for use in collisionExit.
                    XblockDelegate(true);
                    lastXBlocker = c.collider;

                    //force camera into bounds.
                    MoveScreenIntoBoundsX(c.collider);
                }

                //if the two contact points y-coordinate are on the same line, then either the left or right part of the box-collider must have collided.
                if (p0.y == p1.y)
                {
                    //call delegate to block y-axis and record collider for use in collisionExit.
                    YblockDelegate(true);
                    lastYBlocker = c.collider;

                    //force camera into bounds.
                    MoveScreenIntoBoundsY(c.collider);
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

    private void MoveScreenIntoBoundsY(Collider2D c)
    {
        float colliderSize = ((c.GetComponent<BoxCollider2D>().size.y) / 2);
        float camVertSize = Camera.main.orthographicSize;

        //check if top or bottom
        if (c.transform.position.y > this.transform.position.y)
        {
            camVertSize *= -1;
            colliderSize *= -1;
        }

        float destY = c.transform.position.y + colliderSize + camVertSize;
        Vector3 camPos = Camera.main.transform.position;
        Vector3 destination = new Vector3(camPos.x, destY, camPos.z);

        Camera.main.transform.position = destination;
    }

    private void MoveScreenIntoBoundsX(Collider2D c)
    {
        float colliderSize = ((c.GetComponent<BoxCollider2D>().size.x) / 2);
        float camHoriSize = Camera.main.orthographicSize * Screen.width / Screen.height;

        if (c.transform.position.x > this.transform.position.x)
        {
            camHoriSize *= -1;
            colliderSize *= -1;
        }

        float destX = c.transform.position.x + colliderSize + camHoriSize;
        Vector3 camPos = Camera.main.transform.position;
        Vector3 destination = new Vector3(destX, camPos.y, camPos.z);

        Camera.main.transform.position = destination;
    }
}
