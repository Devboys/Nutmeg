using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealthHandler : MonoBehaviour {

    [SerializeField] private int initHealth = 3;
    [SerializeField] private int maxHealth = 3;

    public event Action<int> onHealthChangedEvent;


    [HideInInspector] public int currentHealth; 

    private void Start()
    {

        currentHealth = initHealth;
        
    }

    public void ModHealth(int amount)
    {
        currentHealth += amount;

        if (currentHealth > maxHealth)
            currentHealth = maxHealth;

        if(onHealthChangedEvent != null)
        {
            onHealthChangedEvent(currentHealth);
        }

        if(currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        //temporary die
        transform.position = Vector3.zero;
        ModHealth(initHealth);
    }

    public int GetInitHealth() { return initHealth; }
    public int GetMaxHealth()  { return maxHealth; }
}

