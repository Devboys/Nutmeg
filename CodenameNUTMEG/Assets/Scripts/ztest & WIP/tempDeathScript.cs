﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tempDeathScript : MonoBehaviour {

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.name == "Player")
        {
            other.transform.position = Vector3.zero;
        }
    }
}