using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealthHandler : MonoBehaviour {

    [Header("Values")]
    [SerializeField] private int initHealth = 3;
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private bool invincible;

    [Header("Respawn")]
    [SerializeField] private Transform currentCheckpoint;

    public event Action<int> OnHealthChangedEvent;

    private int _currentHealth; //currenthealth-value.
    [HideInInspector] public int currentHealth //currenthealth-property
    {
        set
        {
            _currentHealth = value;
            if (OnHealthChangedEvent != null)
                OnHealthChangedEvent(_currentHealth);
        }
        get{ return _currentHealth; }
    } 

    private void Start()
    {
        currentHealth = initHealth;
    }

    public void ModHealth(int amount)
    {
        if (!invincible)
        {
            currentHealth += amount;

            if (currentHealth > maxHealth)
                currentHealth = maxHealth;

            if (currentHealth <= 0)
            {
                Die();
            }
        }
    }

    private void Die()
    {
        //temp die mechanism
        transform.position = (currentCheckpoint == null) ? Vector3.zero : currentCheckpoint.position;
        currentHealth = initHealth;
    }

    public void SetCheckpoint(Transform c)
    {
        currentCheckpoint = c;
    }

    public int GetInitHealth(){ return initHealth; }
    public int GetMaxHealth(){ return maxHealth; }
}

