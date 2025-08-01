using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SCR_Interact : MonoBehaviour
{
    private KeyCode InteractKey;

    private List<Collider2D> colliders = new List<Collider2D>();
    public List<Collider2D> GetColliders () { return colliders; }
    
    private void Start()
    {
        InteractKey = gameObject.transform.root.GetComponent<SCR_PlayerController>().interactKey;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag("Interactable")) { return;} 
        //If list of colliders does not contain the detected collider, add it to the list
        if(!colliders.Contains(other)) {colliders.Add(other);}

        if (colliders.Count > 1)
        {
            FindClosestCollider();
        }
        
        Debug.Log("Activate pick up prompt on nearby object");
        
    }

    private GameObject FindClosestCollider()
    {
        GameObject closestColliderOBJ;
        float minDistance = float.MaxValue;
        int closestColliderIndex = -1;

        for (int i = 0; i < colliders.Count; i++)
        {
            float distance = Vector3.Distance(transform.position, colliders[i].transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestColliderIndex = i;
            }
        }

        if (closestColliderIndex != -1)
        {
            closestColliderOBJ = colliders[closestColliderIndex].gameObject;
        }
        else
        {
            Debug.Log("Find closest collider failed");
            closestColliderOBJ = null;
        }

        Debug.Log("Closest Collider is: " + closestColliderOBJ);
        return closestColliderOBJ;
    }

    private void EnableInteractPromptOnOBJ(GameObject promptsObj)
    {
        // TODO - Implement method for enabling interact prompt on given object
    }

    private void DisableInteractPromptOnOBJ(GameObject promptsObj)
    {
        // TODO - Implement method for disabling interact prompt on given object
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Interactable"))
        {
            colliders.Remove(other);
        }
    }
}
