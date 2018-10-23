using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamBoundTrigger : MonoBehaviour {

    public delegate void blockDirectionDelegate(bool b, Collider2D c);
    public blockDirectionDelegate blockDelegate;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "CameraBlock")
        {
			blockDelegate(true, collision);
        }

    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "CameraBlock")
        {
			blockDelegate(false, collision);
        }
    }
}
