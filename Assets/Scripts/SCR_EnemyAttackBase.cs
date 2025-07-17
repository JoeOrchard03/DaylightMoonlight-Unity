using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_EnemyAttackBase : MonoBehaviour
{
    public float damage;
    public LayerMask playerLayer;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) == 0) return;
        Debug.Log("player hit");
        other.gameObject.GetComponent<SCR_PlayerController>().TakeDamage(damage);
    }
} 
