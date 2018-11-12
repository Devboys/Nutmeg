using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowerAggroboxScript : MonoBehaviour {

    //parent component cache
    EnemyFollowerScript parentScript;

    void Start()
    {
        parentScript = GetComponentInParent<EnemyFollowerScript>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        parentScript.OnTriggerEnter2D(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        parentScript.OnTriggerExit2D(collision);

        Debug.Log("exit");
    }
}
