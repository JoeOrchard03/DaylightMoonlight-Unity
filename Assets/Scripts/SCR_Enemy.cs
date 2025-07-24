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
    private bool stunned = false;
    public float stunDuration;
    
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
    public bool isAttacking = false;
    public bool playerInRange = false;
    private Coroutine attackWindUpCoroutine;
    
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
        if (!playerRef) return;
        ApproachPlayer();
    }

    #region Movement
    
    private void ApproachPlayer()
    {
        navAgentRef.destination = playerRef.transform.position;
        Vector2 currentPos = transform.position;
        Vector2 travellingDirection = (currentPos - lastPos).normalized;

        lastPos = currentPos;
        facingDirection = travellingDirection.x >= 0 ? "right" : "left";
        
        //Gets the direction towards the player for handling attack offsets
        Vector2 dirToPlayer = (playerRef.transform.position - transform.position).normalized;
        attackDirection = dirToPlayer.x >= 0 ? "right" : "left";
    }

    #endregion
    
    //When the player enters a circle around the enemy they will launch an attack
    private void OnTriggerStay2D(Collider2D other)
    {
        //Makes sure the enemy is trying to attack a player
        if (!other.gameObject.transform.root.gameObject.CompareTag("Player")) return;
        playerInRange = true;
        //Only attack if the enemy is not already attacking/attack is on cooldown
        if(attackOnCooldown || isAttacking || stunned) {return;}
        attackWindUpCoroutine = StartCoroutine(AttackWindUp());
    }
    
    //Attack windup (currently just changing color) to warn player of attack before it happens
    private IEnumerator AttackWindUp()
    {
        isAttacking = true;
        //Enemy should stay still while performing an attack
        //canMove = false;
        //Change color to give indication attack is incoming
        spriteRenderer.color = enemyAttackingColor;
        yield return new WaitForSecondsRealtime(attackWindUpTime);
        //Change color back once attack launches
        spriteRenderer.color = enemyColor;
        //Uses attack from appropriate enemy type script
        if(soldierScriptRef) {soldierScriptRef.Attack(attackDirection);}
        isAttacking = false;
        StartCoroutine(AttackCooldown());
    }

    private IEnumerator AttackCooldown()
    {
        attackOnCooldown = true;
        canMove = false;
        //Wait for the attack cooldown
        yield return new WaitForSecondsRealtime(attackCooldown);
        //Enable movement after attack is finished
        attackOnCooldown = false;
        canMove = true;
    }
    
    //Makes sure the enemy does not continuously attack after the player leaves the collider
    private void OnTriggerExit2D(Collider2D other)
    {
        playerInRange = false;
    }
    
    //Takes damage from player attacks
    public void TakeDamage(float damageAmount)
    {
        health -= damageAmount;
        if(health <= 0)
        {
            Die();
        }
        else
        {
            Stun();
        }
    }

    private void Stun()
    {
        spriteRenderer.color = Color.grey;
        stunned = true;
        navAgentRef.isStopped = true;
        canMove = false;
        Debug.Log("Enemy stunned");

        if (attackWindUpCoroutine != null) 
        { 
            Debug.Log("Enemy attack interrupted"); 
            StopCoroutine(attackWindUpCoroutine);
        }

        // Reset attack flags to prevent stuck states
        attackOnCooldown = false;
        isAttacking = false;

        CancelInvoke(nameof(StunTimer));
        Invoke(nameof(StunTimer), stunDuration);
    }

    private void StunTimer()
    {
        stunned = false;
        spriteRenderer.color = enemyColor;
        canMove = true;
        navAgentRef.isStopped = false;
        if (playerInRange && !attackOnCooldown)
        {
            attackWindUpCoroutine = StartCoroutine(AttackWindUp());
        }
        Debug.Log("Enemy recovered from stun");
    }

    //Destroy the enemy after their health is less than or equal to 0
    private void Die()
    {
        Destroy(this.gameObject);
    }
}
