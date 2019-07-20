using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicSpikeScript : MonoBehaviour
{

    //Trigger ------------

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Triggered spike");
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        Debug.Log("UnTriggered Spike");
    }


    //Collide -----------
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Collided spike");
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        Debug.Log("UnCollided spike");
    }
}
