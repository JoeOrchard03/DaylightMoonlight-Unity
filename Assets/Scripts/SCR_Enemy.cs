using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class SCR_Enemy : MonoBehaviour
{
    [Header("Health variables")]
    public float maxHealth;
    public float health;
    
    [Header("Navigation variables")]
    public GameObject playerRef;
    public NavMeshAgent navAgentRef;
    private bool canMove = true;
    public string facingDirection;
    private Vector2 lastPos = Vector2.zero;

    [Header("Attack variables")] 
    [SerializeField] private float attackWindUpTime;
    [SerializeField] private float attackCooldown;
    [SerializeField] private bool attackOnCooldown = false;
    [SerializeField] private string attackDirection;
    private bool isAttacking = false;   
    
    [Header("Sub enemy scripts")]
    public SCR_Soldier soldierScriptRef;
    
    [Header("Sprite variables")]
    private SpriteRenderer spriteRenderer;
    public Color enemyColor;
    public Color enemyAttackingColor;

    private void Start()
    {
        health = maxHealth;
        playerRef = GameObject.FindGameObjectWithTag("Player");
        navAgentRef.updateRotation = false;
        navAgentRef.updateUpAxis = false;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if(!canMove) return;
        ApproachPlayer();
    }
    
    private void ApproachPlayer()
    {
        navAgentRef.destination = playerRef.transform.position;
        Vector2 currentPos = transform.position;
        Vector2 travellingDirection = (currentPos - lastPos).normalized;

        lastPos = currentPos;
        facingDirection = travellingDirection.x >= 0 ? "right" : "left";
        
        Vector2 dirToPlayer = (playerRef.transform.position - transform.position).normalized;
        attackDirection = dirToPlayer.x >= 0 ? "right" : "left";
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

    private IEnumerator AttackWindUp()
    {
        isAttacking = true;
        StartCoroutine(AttackCooldown());
        spriteRenderer.color = enemyAttackingColor;
        canMove = false;
        yield return new WaitForSecondsRealtime(attackWindUpTime);
        spriteRenderer.color = enemyColor;
        if(soldierScriptRef) {soldierScriptRef.Attack(attackDirection);}
        isAttacking = false;
    }

    private IEnumerator AttackCooldown()
    {
        attackOnCooldown = true;
        yield return new WaitForSecondsRealtime(attackCooldown);
        attackOnCooldown = false;
        canMove = true;
    }
    
    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        Debug.Log("Player has entered attack range");
        if(attackOnCooldown || isAttacking) {return;}
        StartCoroutine(AttackWindUp());
    }
}
