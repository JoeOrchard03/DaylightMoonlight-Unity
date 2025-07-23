using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_PlayerAttackBase : MonoBehaviour
{
    public float damage;
    public LayerMask enemyLayer;

    //Deals damage to any enemies hit
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & enemyLayer) == 0) return;
        Debug.Log("enemy hit, dealing " + damage + " damage");
        other.gameObject.GetComponent<SCR_Enemy>().TakeDamage(damage);
    }
} 
