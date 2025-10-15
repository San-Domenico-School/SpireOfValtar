using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NewMonoBehaviourScript : MonoBehaviour
{
    private void Awake()
    {
        RestartLevel();
        Debug.Log("Restart?");
    }

    void RestartLevel() //restart when button is pressed
    {
        Scene currentScene = SceneManager.GetActiveScene();
        Debug.Log("Restarted");
    }
}
