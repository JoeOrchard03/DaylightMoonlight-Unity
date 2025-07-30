using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Scripting.APIUpdating;
using Debug = UnityEngine.Debug;

public class SCR_PlayerController : MonoBehaviour
{
    [Header("Public Movement variables")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 12f;
    [Tooltip("Deadzone stops the player from freezing when quickly changing directions")]
    public float movementDeadzone = 0.5f;
    
    [Header("Private Movement variables")]
    private float moveSpeed;
    private enum FacingDirection {Left, Right, Up, Down}
    private enum DodgeDirection {Left, Right}
    private DodgeDirection dodgeDirection = DodgeDirection.Right;
    private FacingDirection facingDirection = FacingDirection.Right;
    private bool isMoving = false;
    
    [Header("Health variables")]
    public float maxHealth = 100f; 
    public float currentHealth;

    [Header("Public Dodge variables")] 
    public float IFrameDuration = 0.5f;
    public float dodgeStrength = 10.0f;
    public float dodgeDuration = 0.2f;
    public float dodgeCooldownTime = 1.0f;
    public LayerMask enemyLayer;
    public LayerMask nothingLayer;
    
    [Header("Private Dodge variables")]
    private bool canDodge = true;
    private bool dodging = false;
    private bool dodgedInAir = false;
    private bool resetDodgeOnLand = false;
    private bool playerInvincible = false;
    
    [Header("Public Attack variables")]
    public GameObject lightAttackHb;
    public GameObject lightAttackComboFinisherHb;
    [TooltipAttribute("Distance from the player the hit box is instantiated")]
    public float hitOriginDistance = 1.0f;
    [TooltipAttribute("How long the hitbox will stay spawned for")]
    public float hitBoxPersistenceDuration = 0.1f;
    public int maxLightAttackComboCount = 3;
    public float nextComboInputMaxTime = 0.75f;
    public float attackCooldown = 0.2f;
    public float comboFinisherAttackCooldown = 0.5f;

    [Header("Private Attack variables")]
    private Vector2 HBSpawnPosition;
    private Quaternion HBSpawnRotation;
    private bool readyToAttack = true;
    private float attackPosOffset;
    private int comboCount = 0;
    private bool canCombo = false;
    private Coroutine comboCoroutine;
    
    [Header("Public Jump variables")]
    public float initialJumpForce;
    public float heldJumpForce;
    public float maxJumpDuration = 0.3f;
    
    [Header("Private Jump variables")]
    private bool jumpCanContinue = true;
    private float jumpTimer = 0.0f;
    private bool isJumping = false;
    
    [Header("Key Binds")]
    public KeyCode sprintButton;
    public KeyCode jumpButton;
    public KeyCode lightAttackButton;
    public KeyCode dodgeButton = KeyCode.LeftControl;

    [Header("Objects")]
    public Rigidbody2D playerRb;

    [Header("Ground check variables")]
    [HideInInspector]
    public bool isGrounded = false;
    private bool isGroundedPrevFrame;
    
    [Header("Public Animation variables")]
    public SpriteRenderer playerSpriteRenderer;
    public Animator playerAnimator;
    
    [Header("Private Animator variables")]
    private static readonly int IsRunning = Animator.StringToHash("IsRunning");
    private static readonly int IsWalking = Animator.StringToHash("IsWalking");
    public static readonly int IsFalling = Animator.StringToHash("IsFalling");
    private static readonly int IsJumping = Animator.StringToHash("IsJumping");
    private static readonly int IsGrounded = Animator.StringToHash("IsGrounded");
    private static readonly int Land = Animator.StringToHash("Land");

    [Header("Camera variables")] 
    public GameObject cameraObj;
    private bool cameraFollow = true;
    
    [Header("UI variables")]
    public GameObject gameOverScreen;
    
    [Header("Misc variables")]
    private Color spriteRendererColor;
    private float startTime;

    private void Start()
    {
        spriteRendererColor = GetComponent<SpriteRenderer>().color;
        moveSpeed = walkSpeed;
        currentHealth = maxHealth;
    }

    private void Update()
    {
        SetFacingDirection();
        JumpCheck();
        Dodge();
        Move();
        Sprint();
        HandleAnimations();
        if(Input.GetKeyDown(lightAttackButton) && readyToAttack)
        {
            GetAttackOffset();
            CheckComboCount();
        }
        if (cameraFollow)
        {
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
                    //Reset dodge cooldown if it reset while airborne
                    if (resetDodgeOnLand) { canDodge = true; dodgedInAir = false; }
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

    private void SetFacingDirection()
    {
        if(Input.GetKey(KeyCode.A)){ facingDirection = FacingDirection.Left; dodgeDirection = DodgeDirection.Left; }
        else if(Input.GetKey(KeyCode.D)){ facingDirection = FacingDirection.Right; dodgeDirection = DodgeDirection.Right; }
        else if(Input.GetKey(KeyCode.W)){ facingDirection = FacingDirection.Up; }
        else if(Input.GetKey(KeyCode.S)){ facingDirection = FacingDirection.Down; }
    }

    private void Dodge()
    {
        if (!Input.GetKeyDown(dodgeButton) || !canDodge) return;
        if (!isGrounded) { dodgedInAir = true; }
        dodging = true;
        playerInvincible = true;
        canDodge = false;
        Debug.Log("Attempting dodge");
        Vector2 forceDirection;
        switch (dodgeDirection)
        {
            case DodgeDirection.Left:
                forceDirection = new Vector2(-1, 0);
                break;
            case DodgeDirection.Right:
                forceDirection = new Vector2(1.0f, 0f);
                break;
            default:
                Debug.LogError("DodgeDirection not being set properly");
                forceDirection = Vector2.zero;
                break;
        }
            
        Debug.Log("Dodging " + forceDirection);
        playerRb.AddForce(forceDirection * dodgeStrength, ForceMode2D.Impulse);
        StartCoroutine(DodgeDuration());
        StartCoroutine(PlayerIFrameDuration());
    }

    private IEnumerator DodgeDuration()
    {
        yield return new WaitForSeconds(dodgeDuration);
        dodging = false;
        StartCoroutine(DodgeCooldownTimer());
    }
    
    private IEnumerator DodgeCooldownTimer()
    {
        yield return new WaitForSeconds(dodgeCooldownTime);
        if(isGrounded) {canDodge = true;}
        else { resetDodgeOnLand = true; }
    }

    private IEnumerator PlayerIFrameDuration()
    {
        GetComponent<SpriteRenderer>().color = Color.blue;
        GetComponent<CapsuleCollider2D>().excludeLayers = enemyLayer;
        yield return new WaitForSeconds(IFrameDuration);
        playerInvincible = false;
        GetComponent<SpriteRenderer>().color = spriteRendererColor;
        GetComponent<CapsuleCollider2D>().excludeLayers = nothingLayer;
    }
    
    private void Move()
    {
        if(dodging) {return;}
        
        //Get input from A and D keys
        float XInput = Input.GetAxis("Horizontal");

        //stop animation player when player is only just barely moving
        if (Mathf.Abs(XInput) > movementDeadzone)
        {
            playerRb.velocity = new Vector2(XInput * moveSpeed, playerRb.velocity.y);
            isMoving = true;
            switch (XInput)
            {
                case <= 0:
                    playerSpriteRenderer.flipX = true;
                    break;
                case >= 0:
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

        //Cancel jump if the player releases the key early
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

    private void GetAttackOffset()
    {
        //Gets the offset to positon the attack hitbox at
        attackPosOffset = facingDirection switch
        {
            FacingDirection.Right => 1.0f,
            FacingDirection.Left => -1.0f,
            FacingDirection.Up => 2.0f,
            FacingDirection.Down => -2.0f,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    //Checks what the current combo is
    private void CheckComboCount()
    {
        //If the player cannot combo anymore, reset it to 0
        if (!canCombo && comboCount > 0)
        {
            comboCount = 0;
        }
        
        //Stops player inputting anymore attacks while handling the current one
        readyToAttack = false;
        //Increment combo count
        comboCount++;
        
        //If the combo has not reached the end, continue attack chain
        if (comboCount < maxLightAttackComboCount)
        {
            //Spawn the non-combo finisher version of the attack hitbox
            SetHbPosAndRot(false);

            //If the combo timer is currently running, stop it
            if (comboCoroutine != null)
            {
                StopCoroutine(comboCoroutine);
            }
            
            //Start/reset the combo input timer
            comboCoroutine = StartCoroutine(ComboInputTimer());
            //Start a quicker version of the attack cooldown
            StartCoroutine(AttackCooldown(false));
        }
        //If the combo has reached the end
        else
        {
            //Spawn the combo finisher of the attack hb
            SetHbPosAndRot(true);
            //Reset the combo count
            comboCount = 0;
            //Start a longer version of the attack cooldown
            StartCoroutine(AttackCooldown(true));
        }
    }

    private void SetHbPosAndRot(bool comboFinisher)
    {
        switch (facingDirection)
        {
            case FacingDirection.Right:
            case FacingDirection.Left:
                //Set the hb spawn pos and rot adding the attack offset
                HBSpawnPosition = new Vector2(transform.position.x, transform.position.y) + new Vector2(attackPosOffset, 0);
                HBSpawnRotation = Quaternion.identity;
                //Handle the spawning
                SpawnAttackHb(HBSpawnPosition, HBSpawnRotation, comboFinisher);
                break;
            case FacingDirection.Up:
            case FacingDirection.Down when isGrounded == false:
                HBSpawnPosition = new Vector2(transform.position.x, transform.position.y) + new Vector2(0, attackPosOffset);
                HBSpawnRotation = Quaternion.Euler(0,0,90);
                SpawnAttackHb(HBSpawnPosition, HBSpawnRotation, comboFinisher);
                break;
            default:
                Debug.Log("Cannot attack, may be trying to attack down while grounded?");
                break;
        }
    }
    
    //Handles spawning of the appropriate attack hitbox
    private void SpawnAttackHb(Vector2 hbSpawnPos, Quaternion hbSpawnRot, bool comboFinisher)
    {
        //Instantiate the appropriate attack hb
        GameObject spawnedHitbox = Instantiate(comboFinisher ? lightAttackComboFinisherHb : lightAttackHb, hbSpawnPos, hbSpawnRot);
        //Starts timer to delete hitbox
        StartCoroutine(HitBoxDeleteTimer(spawnedHitbox));
    }
    
    //Timer to detect if the next input to continue the combo comes through in time
    private IEnumerator ComboInputTimer()
    {
        canCombo = true;
        yield return new WaitForSeconds(nextComboInputMaxTime);
        canCombo = false;
        comboCount = 0;
    }

    //Handles attack cooldowns
    private IEnumerator AttackCooldown(bool comboFinisher)
    {
        //Stops player inputting more attacks
        readyToAttack = false;
        //If the combo finisher was used, run longer cooldown
        if (comboFinisher) { yield return new WaitForSeconds(comboFinisherAttackCooldown); }
        //If the combo finisher was not used run a shorter cooldown
        else { yield return new WaitForSeconds(attackCooldown); }
        //Mark player as ready to attack once cooldown is over
        readyToAttack = true;
    }
    
    //Handles deleting the attack hitboxes after a short delay
    private IEnumerator HitBoxDeleteTimer(GameObject hitboxToDelete)
    {
        yield return new WaitForSeconds(hitBoxPersistenceDuration);
        Destroy(hitboxToDelete);
    }
    
    public void TakeDamage(float damage)
    {
        if(playerInvincible) {Debug.Log("Player invincible, no damage taken"); return;}
        currentHealth -= damage;
        if (!(currentHealth <= 0)) return;
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
