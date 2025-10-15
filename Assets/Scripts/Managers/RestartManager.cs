using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NewMonoBehaviourScript : MonoBehaviour
{
    private void Start()
    {
        RestartLevel();
        Debug.Log("Restart?");
    }
    void RestartLevel() //restart when button is pressed
    {
        Time.timeScale = 1;
        Scene currentScene = SceneManager.GetActiveScene();
        Debug.Log("Restarted");
    }
}
