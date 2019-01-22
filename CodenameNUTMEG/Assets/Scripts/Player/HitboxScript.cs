using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxScript : MonoBehaviour {

    public float invincibleTimer;

    //Component cache
    PlayerHealthHandler healthHandler;
    CharacterController controller;

    private bool inReelingState;
    private float reelingTimer;

    private void Start()
    {
        healthHandler = GetComponentInParent<PlayerHealthHandler>();
        controller = GetComponentInParent<CharacterController>();
    }

    public void Update()
    {
        if (reelingTimer > 0)
            reelingTimer -= Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("DamageSource") && !collision.isTrigger)
        {
            if (reelingTimer <= 0)
            {
                controller.BeginDamageKnockback(collision.transform.position);
                healthHandler.ModHealth(-1);
                reelingTimer = invincibleTimer;
            }
        }
    }

}
