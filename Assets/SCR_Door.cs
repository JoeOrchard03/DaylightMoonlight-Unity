using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_Door : MonoBehaviour, INT_Interactable
{
    public bool doorUnlocked = false;

    public void Interact(GameObject interactor)
    {
        Debug.Log("Interacting with door");

        if (doorUnlocked)
        {
            Enter();
            return;
        }
        else
        {
            Debug.Log("Door locked");
        }
    }
    
    public void Enter()
    {
        Debug.Log("Entering door");
    }
}
