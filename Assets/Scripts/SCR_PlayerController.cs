using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using Debug = UnityEngine.Debug;

public class SCR_PlayerController : MonoBehaviour
{
    private static readonly int IsRunning = Animator.StringToHash("IsRunning");
    private static readonly int IsWalking = Animator.StringToHash("IsWalking");
    public static readonly int IsFalling = Animator.StringToHash("IsFalling");
    private static readonly int IsJumping = Animator.StringToHash("IsJumping");
    private static readonly int IsGrounded = Animator.StringToHash("IsGrounded");
    private static readonly int Land = Animator.StringToHash("Land");

    [Header("Movement values")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 12f;
    [Tooltip("Deadzone stops the player from freezing when quickly changing directions")]
    public float movementDeadzone = 0.5f;
    private float moveSpeed;
    private string facingDirection = "right";
    private bool isMoving = false;

    [Header("Health values")]
    public float maxHealth = 100f; 
    public float currentHealth;
    
    [Header("Attack values")]
    public GameObject lightAttackHb;
    public GameObject lightAttackComboFinisherHb;
    [TooltipAttribute("Distance from the player the hit box is instantiated")]
    public float hitOriginDistance = 1.0f;
    [TooltipAttribute("How long the hitbox will stay spawned for")]
    public float hitBoxPersistenceDuration = 0.1f;
    public bool readyToAttack = true;

    [Header("Combo values")]
    public int maxLightAttackComboCount = 3;
    public float nextComboInputMaxTime = 0.75f;
    public float attackCooldown = 0.5f;
    public int comboCount = 0;
    public bool canCombo = false;
    private Coroutine comboCoroutine;
    
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

    private bool isGroundedPrevFrame;

    [Header("Animation variables")]
    public SpriteRenderer playerSpriteRenderer;
    public Animator playerAnimator;

    [Header("Camera variables")] 
    public GameObject cameraObj;
    private bool cameraFollow = true;
    
    [Header("Misc variables")]
    public GameObject gameOverScreen;
    private float startTime;

    private void Start()
    {
        moveSpeed = walkSpeed;
        currentHealth = maxHealth;
    }

    private void Update()
    {
        JumpCheck();
        Move();
        Sprint();
        CameraFollow();
        HandleAnimations();
        if(Input.GetKeyDown(lightAttackButton) && readyToAttack)
        {
            CheckComboCount();
            //Attack("lightAttack");
        }
        if (cameraFollow)
        {
            //Follow the player with the camera
            CameraFollow();
        }
    }

    private void LateUpdate()
    {
        //Detects whether the player was grounded in the last frame or not, used for landing anim
        isGroundedPrevFrame = isGrounded;
    }
    
    #region Movement

    private void HandleAnimations()
    {
        //Sets is grounded in the animator
        playerAnimator.SetBool(IsGrounded, isGrounded);

        switch (isGrounded)
        {
            //If the player is not grounded and going up
            case false when playerRb.velocity.y > 0:
                //Play jump anim
                playerAnimator.SetBool(IsJumping, true);
                //Stop falling anim
                playerAnimator.SetBool(IsFalling, false);
                break;
            
            //If the player is not grounded and going down
            case false when playerRb.velocity.y < 0:
                //Stop jump anim
                playerAnimator.SetBool(IsJumping, false);
                //Play falling anim
                playerAnimator.SetBool(IsFalling, true);
                break;
            
            //If the player is grounded
            case true:
            {
                //If the player just landed
                if (!isGroundedPrevFrame && isGrounded)
                {
                    //Play landing anim
                    playerAnimator.SetTrigger(Land);
                }
            
                //Set jumping and falling to false
                playerAnimator.SetBool(IsJumping, false);
                playerAnimator.SetBool(IsFalling, false);
                break;
            }
        }
    }
    
    private void Move()
    {
        //Get input from A and D keys
        float XInput = Input.GetAxis("Horizontal");

        //stop animation player when player is only just barely moving
        if (Mathf.Abs(XInput) > movementDeadzone)
        {
            playerRb.velocity = new Vector2(XInput * moveSpeed, playerRb.velocity.y);
            isMoving = true;
            switch (XInput)
            {
                //facingDirection is used to decide what direction to send out attacks
                case <= 0:
                    facingDirection = "left";
                    playerSpriteRenderer.flipX = true;
                    break;
                case >= 0:
                    facingDirection = "right";
                    playerSpriteRenderer.flipX = false;
                    break;
            }

            //Starts walking animation
            if (!playerAnimator.GetBool(IsWalking))
            {
                playerAnimator.SetBool(IsWalking, true);
            }
        }
        else
        {
            //Starts idle animation
            isMoving = false;
            if (!playerAnimator.GetBool(IsWalking) && !playerAnimator.GetBool(IsRunning)) return;
            playerAnimator.SetBool(IsWalking, false);
            playerAnimator.SetBool(IsRunning, false);
        }
    }
    
    //Handles jump logic
    private void JumpCheck()
    {
        if (isGrounded && Input.GetKeyDown(jumpButton))
        {
            //Initial jump force
            Debug.Log("jump animation being triggered");
            // playerAnimator.SetBool(IsJumping, true);
            playerRb.AddForce(transform.up * initialJumpForce, ForceMode2D.Impulse);
            isJumping = true;
            //If the jump has not lasted too long, player can continue to hold space to go higher
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
                //If the jump has reached max duration, this stops it
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

    //Handles sprinting logic
    private void Sprint()
    {
        if(Input.GetKey(sprintButton) && isGrounded)
        {
            moveSpeed = sprintSpeed;
            //Starts running animation
            if (!playerAnimator.GetBool(IsRunning) && isMoving)
            {
                playerAnimator.SetBool(IsRunning, true);
                playerAnimator.SetBool(IsWalking, false);
            }
        }

        //Stops walking animation and starts walking animation
        if (Input.GetKey(sprintButton) || !isGrounded) return;
        moveSpeed = walkSpeed;
        if (!playerAnimator.GetBool(IsRunning) || !isMoving) return;
        playerAnimator.SetBool(IsRunning, false);
        //playerAnimator.SetBool(IsWalking, true);
    }
    
    #endregion

    #region Combat

    private void CheckComboCount()
    {
        if (!canCombo && comboCount > 0)
        {
            comboCount = 0;
        }
        
        readyToAttack = false;
        comboCount++;
        Debug.Log("Current combo is " + comboCount);
        
        if (comboCount < maxLightAttackComboCount)
        {
            Attack(false);

            if (comboCoroutine != null)
            {
                StopCoroutine(comboCoroutine);
            }
            
            comboCoroutine = StartCoroutine(ComboInputTimer());
        }
        else
        {
            Attack(true);
            comboCount = 0;
        }

        StartCoroutine(AttackCooldown());
    }
    
    private void Attack(bool comboFinisher)
    {
        SpawnAttackHB(comboFinisher);
    }

    private void SpawnAttackHB(bool comboFinisher)
    {
        float attackPosOffset = facingDirection switch { "right" => 1.0f, "left" => -1.0f, _ => 0.0f };
        Vector2 HBSpawnPosition = new Vector2(transform.position.x, transform.position.y) + new Vector2(attackPosOffset, 0);
        GameObject spawnedHitbox;
        if (comboFinisher)
        {
            Debug.Log("Spawning combo finisher hitbox");
            spawnedHitbox = Instantiate(lightAttackComboFinisherHb, HBSpawnPosition, Quaternion.identity);  
        }
        else
        {
            Debug.Log("Spawning regular hitbox");
            spawnedHitbox = Instantiate(lightAttackHb, HBSpawnPosition, Quaternion.identity);  
        }
        //Starts timer to delete hitbox
        StartCoroutine(HitBoxDeleteTimer(spawnedHitbox));
    }
    
    private IEnumerator ComboInputTimer()
    {
        canCombo = true;
        yield return new WaitForSeconds(nextComboInputMaxTime);
        canCombo = false;
        comboCount = 0;
    }

    private IEnumerator AttackCooldown()
    {
        readyToAttack = false;
        yield return new WaitForSeconds(attackCooldown);
        readyToAttack = true;
    }
    
    private IEnumerator HitBoxDeleteTimer(GameObject hitboxToDelete)
    {
        yield return new WaitForSeconds(hitBoxPersistenceDuration);
        Destroy(hitboxToDelete);
    }
    
    //Handles deleting of hitbox
    
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (!(currentHealth <= 0)) return;
        Debug.Log("Player has died");
        Die();
    }

    //Destroys player obj and brings up the game over screen
    public void Die()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        gameOverScreen.SetActive(true);
        Destroy(this.gameObject);
    }

    #endregion
    
    //Follow the player with the camera at a consistent z distance from them
    private void CameraFollow()
    {
        Vector3 newCameraPos = new Vector3
        {
            z = cameraObj.transform.position.z,
            x = this.gameObject.transform.position.x,
            y = this.gameObject.transform.position.y
        };
        cameraObj.transform.position = newCameraPos;
    }
}
