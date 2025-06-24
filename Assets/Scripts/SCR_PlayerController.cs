using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class SCR_PlayerController : MonoBehaviour
{
    public Rigidbody2D playerRB;
    public float groundRayCastDistance = 1.25f;
    public LayerMask groundLayerMask;
    public float walkSpeed = 5f;
    public float sprintSpeed = 12f;
    public bool isGrounded = false;
    public float jumpForce = 100f;

    // Update is called once per frame
    void Update()
    {
        GroundedCheck();
        Move();
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
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            playerRB.velocity = (new Vector2(playerRB.velocity.x, jumpForce));
            isGrounded = false;
        }

        float XInput = Input.GetAxis("Horizontal");

        if(Mathf.Abs(XInput) > 0)
        {
            playerRB.velocity = new Vector2(XInput * walkSpeed, playerRB.velocity.y);
        }
    }

    private void Jump()
    {
        
    }
}
