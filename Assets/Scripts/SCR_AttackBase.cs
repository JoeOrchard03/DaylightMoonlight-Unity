using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_AttackBase : MonoBehaviour
{
    public float damage;
    public LayerMask enemyLayer;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & enemyLayer) != 0)
        {
            Debug.Log("enemy hit");
            other.gameObject.GetComponent<SCR_Enemy>().TakeDamage(damage);
        }
    }
} 
