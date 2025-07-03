using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class SCR_PlayerController : MonoBehaviour
{
    [Header("Movement values")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 12f;
    [Tooltip("Deadzone stops the player from freezing when quickly changing directions")]
    public float movementDeadzone = 0.5f;
    private float moveSpeed;
    private string facingDirection = "right";
    private bool isMoving = false;

    [Header("Attack values")]
    public GameObject lightAttackHb;
    public float lightAttackDamage;
    [TooltipAttribute("Distance from the player the hit box is instantiated")]
    public float hitOriginDistance = 1.0f;
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
    public Rigidbody2D playerRb;

    [Header("Ground check variables")]
    public bool isGrounded = false;

    [Header("Animation variables")]
    public SpriteRenderer playerSpriteRenderer;
    public Animator playerAnimator;

    [Header("Misc variables")]
    private float startTime;

    private void Start()
    {
        moveSpeed = walkSpeed;
    }

    void Update()
    {
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
        if (attackType != "lightAttack") return;
        float attackPosOffset = 0.0f;
        if (facingDirection == "right") { attackPosOffset = 1.0f; }
        if (facingDirection == "left") { attackPosOffset = -1.0f; }

        Vector2 HBSpawnPosition = new Vector2(transform.position.x, transform.position.y) + new Vector2(attackPosOffset, 0);
        GameObject spawnedHitBox = Instantiate(lightAttackHb, HBSpawnPosition, Quaternion.identity);
        StartCoroutine(HitBoxDeleteTimer(spawnedHitBox));
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

    private void Move()
    {
        float XInput = Input.GetAxis("Horizontal");

        if (Mathf.Abs(XInput) > movementDeadzone)
        {
            playerRb.velocity = new Vector2(XInput * moveSpeed, playerRb.velocity.y);
            isMoving = true;
            if(XInput <= 0)
            {
                facingDirection = "left";
                playerSpriteRenderer.flipX = true;
            }
            else if (XInput >= 0)
            {
                facingDirection = "right";
                playerSpriteRenderer.flipX = false;
            }

            if (!playerAnimator.GetBool("IsWalking"))
            {
                playerAnimator.SetBool("IsWalking", true);
            }
        }
        else
        {
            isMoving = false;
            if (playerAnimator.GetBool("IsWalking") || playerAnimator.GetBool("IsRunning"))
            {
                playerAnimator.SetBool("IsWalking", false);
                playerAnimator.SetBool("IsRunning", false);
            }
        }
    }

    private void JumpCheck()
    {
        if (isGrounded && Input.GetKeyDown(jumpButton))
        {
            //Initial jump force
            playerRb.AddForce(transform.up * initialJumpForce, ForceMode2D.Impulse);
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
                playerRb.AddForce(transform.up * heldJumpForce, ForceMode2D.Force);
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
            if (!playerAnimator.GetBool("IsRunning") && isMoving)
            {
                playerAnimator.SetBool("IsRunning", true);
                playerAnimator.SetBool("IsWalking", false);
            }
        }
        if(!Input.GetKey(sprintButton) && isGrounded)
        {
            moveSpeed = walkSpeed;
            if (playerAnimator.GetBool("IsRunning") && isMoving)
            {
                playerAnimator.SetBool("IsRunning", false);
                playerAnimator.SetBool("IsWalking", true);
            }
        }
    }
}
