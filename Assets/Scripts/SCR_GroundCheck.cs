using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_GroundCheck : MonoBehaviour
{
    public LayerMask groundLayer;
    public SCR_PlayerController playerScriptRef;

    private void Start()
    {
        playerScriptRef = gameObject.transform.root.gameObject.GetComponent<SCR_PlayerController>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & groundLayer) == 0) return;
        Debug.Log("Grounded");
        playerScriptRef.isGrounded = true;
        //Triggers landing animation after player becomes grounded
        //playerScriptRef.LandingTrigger();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & groundLayer) == 0) return;
        Debug.Log("Not grounded");
        playerScriptRef.isGrounded = false;
        //Triggers falling animation after player is no longer grounded
        //playerScriptRef.FallingTrigger();
    }
}