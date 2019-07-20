using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicSpikeScript : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        CharacterController target = collision.GetComponent<CharacterController>();

        if(target != null)
        {
            target.ModHealth(-1);
            target.ResetPlayer();
        }

    }
}
