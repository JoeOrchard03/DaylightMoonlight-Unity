using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_Key : MonoBehaviour, INT_Interactable
{
    //Add a key to the player's inventory
    public void Interact(GameObject interactor)
    {
        Debug.Log("Trying to get held keys from: " + interactor.transform.root.gameObject.name);
        interactor.GetComponent<SCR_PlayerController>().heldKeys++;
        Destroy(gameObject);
    }
}
