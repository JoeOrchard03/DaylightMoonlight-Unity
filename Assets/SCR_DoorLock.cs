using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_DoorLock : MonoBehaviour, INT_Interactable
{
    public GameObject diamond;
    public GameObject door;
    public GameObject door1;
    public GameObject door2;

    public void Interact(GameObject interactor)
    {
        //Return if door is already unlocked
        if(door.GetComponent<SCR_Door>().doorUnlocked) {return;}
        //Checks if the player has a key to use
        if (interactor.GetComponentInParent<SCR_PlayerController>().heldKeys >= 1)
        {
            UnlockDoor();
        }
    }
    
    public void UnlockDoor()
    {
        diamond.SetActive(true);
        Debug.Log("Unlocking door");
        door.GetComponent<SCR_Door>().doorUnlocked = true;
        Destroy(door1);
        Destroy(door2);
    }
}
