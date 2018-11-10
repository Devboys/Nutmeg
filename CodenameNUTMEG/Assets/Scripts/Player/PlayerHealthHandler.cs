using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealthHandler : MonoBehaviour {

    [SerializeField] private int initHealth = 3;
    [SerializeField] private int maxHealth = 3;

    public event Action<int> onHealthChangedEvent;

    private int _currentHealth; //currenthealth value.
    [HideInInspector] public int currentHealth //currenthealth-property
    {
        set
        {
            _currentHealth = value;
            if (onHealthChangedEvent != null)
                onHealthChangedEvent(_currentHealth);
        }
        get{ return _currentHealth; }
    } 

    private void Start()
    {

        currentHealth = initHealth;
        
    }

    public void ModHealth(int amount)
    {
        currentHealth += amount;

        if (currentHealth > maxHealth)
            currentHealth = maxHealth;

        if(currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        //temp die mechanism
        transform.position = Vector3.zero;
        currentHealth = initHealth;
    }

    public int GetInitHealth(){ return initHealth; }
    public int GetMaxHealth(){ return maxHealth; }
}

