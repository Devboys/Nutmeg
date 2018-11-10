﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxScript : MonoBehaviour {

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("DamageSource"))
        {
            this.GetComponentInParent<PlayerHealthHandler>().ModHealth(-1);
        }
    }

}
