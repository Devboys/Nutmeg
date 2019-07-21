using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageTriggerScript : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        CharacterController target = collision.GetComponent<CharacterController>();
        target.ModHealth(-1);
    }
}
