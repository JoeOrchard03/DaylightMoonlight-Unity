using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_Enemy : MonoBehaviour
{
    public float maxHealth;
    public float health;
    public GameObject playerRef;

    private void Start()
    {
        health = maxHealth;
        playerRef = GameObject.FindGameObjectWithTag("Player");
    }

    private void ApproachPlayer()
    {
        //
    }
    
    public void TakeDamage(float damageAmount)
    {
        health -= damageAmount;
        if(health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Destroy(this.gameObject);
    }
}
