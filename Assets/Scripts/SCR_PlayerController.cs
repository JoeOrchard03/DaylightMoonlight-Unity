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
        }
    }

    private void JumpCheck()
    {
        if (isGrounded && Input.GetKeyDown(jumpButton))
        {
            playerRB.AddForce(transform.up * initialJumpForce, ForceMode2D.Impulse);
            isJumping = true;
            jumpCanContinue = true;
            jumpTimer = 0.0f;
        }

        if (isJumping && Input.GetKey(jumpButton) && jumpCanContinue)
        {
            jumpTimer += Time.deltaTime;
            if (jumpTimer < maxJumpDuration)
            {
                playerRB.AddForce(transform.up * heldJumpForce, ForceMode2D.Force);
            }
            else
            {
                jumpCanContinue = false;
            } 
        }

        if(Input.GetKeyUp(jumpButton))
        {
            jumpCanContinue = false;
        }

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
