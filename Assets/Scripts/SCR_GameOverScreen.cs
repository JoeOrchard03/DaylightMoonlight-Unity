using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SCR_GameOverScreen : MonoBehaviour
{
    public void Respawn()
    {
        //Reloads the scene
        Debug.Log("Respawning");
        var currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
