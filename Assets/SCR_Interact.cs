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
    public GameObject objToInteractWith;
    public Canvas promptCanvas;
    
    private void Start()
    {
        //Get the interact key from the main player script to keep all the keybinds accessible
        InteractKey = gameObject.transform.root.GetComponent<SCR_PlayerController>().interactKey;
    }

    private void Update()
    {
        if (Input.GetKeyDown(InteractKey) && objToInteractWith != null)
        {
            Debug.Log("Interacting with: " + objToInteractWith.name);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag("Interactable")) { return;} 
        //If list of colliders does not contain the detected collider, add it to the list
        if(!colliders.Contains(other)) {colliders.Add(other);}

        if (colliders.Count > 1)
        {
            objToInteractWith = FindClosestCollider();
        }
        else
        {
            objToInteractWith = other.gameObject;
        }
        
        //Toggle interaction prompt
        promptCanvas.enabled = true;
    }

    //In case there are multiple colliders in range, this method finds the closest one
    private GameObject FindClosestCollider()
    {
        GameObject closestColliderOBJ;
        float minDistance = float.MaxValue;
        int closestColliderIndex = -1;

        for (int i = 0; i < colliders.Count; i++)
        {
            float distance = Vector3.Distance(transform.root.position, colliders[i].transform.position);
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

        promptCanvas.enabled = false;
    }
}
