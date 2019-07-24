using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageTriggerScript : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        CharacterController target = collision.GetComponent<CharacterController>();
        target.ModHealth(-1, transform.position);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CharacterController target = collision.collider.GetComponent<CharacterController>();
        if (target != null) target.ModHealth(-1, collision.contacts[0].point);
    }
}
