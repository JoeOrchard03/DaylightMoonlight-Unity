using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class SCR_PlayerController : MonoBehaviour
{
    [Header("Movement values")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 12f;
    private float moveSpeed;
    private string facingDirection = "right";

    [Header("Attack values")]
    public GameObject lightAttackHB;
    public float lightAttackDamage;
    [TooltipAttribute("Distance from the player the hit box is instantiated")]
    public float hitOrginDistance = 1.0f;
    [TooltipAttribute("How long the hitbox will stay spawned for")]
    public float hitBoxPersistenceDuration = 0.1f;

    [Header("Jump values")]
    public float initialJumpForce;
    public float heldJumpForce;
    public float maxJumpDuration = 0.3f;
    private bool jumpCanContinue = true;
    private float jumpTimer = 0.0f;
    private bool isJumping = false;

    [Header("Key Binds")]
    public KeyCode sprintButton;
    public KeyCode jumpButton;
    public KeyCode lightAttackButton;

    [Header("Objects")]
    public Rigidbody2D playerRB;

    [Header("Ground check variables")]
    public float groundRayCastDistance = 1.25f;
    public LayerMask groundLayerMask;
    public bool isGrounded = false;

    [Header("Misc variables")]
    private float startTime;

    private void Start()
    {
        moveSpeed = walkSpeed;
    }

    void Update()
    {
        GroundedCheck();
        JumpCheck();
        Move();
        SprintCheck();
        if(Input.GetKeyDown(lightAttackButton))
        {
            Attack("lightAttack");
        }
    }
    
    private void Attack(string attackType)
    {
        Debug.Log("Attacking with type: " + attackType);
        if (attackType == "lightAttack")
        {
            float attackPosOffset = 0.0f;
            if (facingDirection == "right") { attackPosOffset = 1.0f; }
            if (facingDirection == "left") { attackPosOffset = -1.0f; }

            Vector2 HBSpawnPosition = new Vector2(transform.position.x, transform.position.y) + new Vector2(attackPosOffset, 0);
            GameObject spawnedHitBox = Instantiate(lightAttackHB, HBSpawnPosition, Quaternion.identity);
            StartCoroutine(HitBoxDeleteTimer(spawnedHitBox));
        }
    }

    private IEnumerator HitBoxDeleteTimer(GameObject hitboxToDelete)
    {
        yield return new WaitForSeconds(hitBoxPersistenceDuration);
        DestroyHitBox(hitboxToDelete);
    }

    private void DestroyHitBox(GameObject hitboxToDelete)
    {
        Destroy(hitboxToDelete);
    }

    private void GroundedCheck()
    {
        Vector2 down = transform.TransformDirection(Vector2.down) * groundRayCastDistance;
        Debug.DrawRay(transform.position, down);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundRayCastDistance, groundLayerMask);
        if(hit)
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    private void Move()
    {
        float XInput = Input.GetAxis("Horizontal");

        if (Mathf.Abs(XInput) > 0)
        {
            playerRB.velocity = new Vector2(XInput * moveSpeed, playerRB.velocity.y);
            if(XInput <= 0)
            {
                facingDirection = "left";
            }
            else if (XInput >= 0)
            {
                facingDirection = "right";
            }
        }
    }

    private void JumpCheck()
    {
        if (isGrounded && Input.GetKeyDown(jumpButton))
        {
            //Initial jump force
            playerRB.AddForce(transform.up * initialJumpForce, ForceMode2D.Impulse);
            isJumping = true;
            jumpCanContinue = true;
            //Resets jump timer to track how long jump has lasted
            jumpTimer = 0.0f;
        }

        //if jump should continue if space bar is held
        if (isJumping && Input.GetKey(jumpButton) && jumpCanContinue)
        {
            //Increments jump timer
            jumpTimer += Time.deltaTime;
            //If the jump has not continued for too long
            if (jumpTimer < maxJumpDuration)
            {
                //Add small force to go higher
                playerRB.AddForce(transform.up * heldJumpForce, ForceMode2D.Force);
            }
            else
            {
                //If jump has reached max time, this stops it
                jumpCanContinue = false;
            } 
        }

        //Stops jump early
        if(Input.GetKeyUp(jumpButton))
        {
            jumpCanContinue = false;
        }

        //Resets after jump is finished and player has landed
        if(isGrounded && !Input.GetKey(jumpButton))
        {
            isJumping = false;
        }
    }

    private void SprintCheck()
    {
        if(Input.GetKey(sprintButton) && isGrounded)
        {
            moveSpeed = sprintSpeed;
        }
        if(!Input.GetKey(sprintButton) && isGrounded)
        {
            moveSpeed = walkSpeed;
        }
    }
}
