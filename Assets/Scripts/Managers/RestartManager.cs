using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartManager : MonoBehaviour
{
    private int activeSceneNumber;
    private int previousSceneNumber;
    private PlayerHealth playerHealth;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        activeSceneNumber = SceneManager.GetActiveScene().buildIndex;
        previousSceneNumber = RestartEvents.SceneIndex;
        if(previousSceneNumber == activeSceneNumber)
        {
            playerHealth = FindFirstObjectByType<PlayerHealth>();
            playerHealth.SetHealthAtStart();
        }
        else
        {
            RestartEvents.SceneIndex = activeSceneNumber;
        }
    }


    public void RestartLevel()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
