using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface INT_Interactable
{
    //All objects that use this interface must have an interact method
    void Interact(GameObject interactor);
}
