using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_GroundCheck : MonoBehaviour
{
    public LayerMask groundLayer;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            Debug.Log("Grounded");
            gameObject.transform.root.gameObject.GetComponent<SCR_PlayerController>().isGrounded = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            Debug.Log("Not grounded");
            gameObject.transform.root.gameObject.GetComponent<SCR_PlayerController>().isGrounded = false;
        }
    }
}