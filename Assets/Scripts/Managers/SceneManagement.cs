using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagement : MonoBehaviour
{
    [SerializeField] private string sceneToLoad;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene(sceneToLoad);
            Debug.Log("Player progressed to next scene");
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("is this method running?");
        GameObject spawnPoint = GameObject.Find("Spawnpoint");

        if (spawnPoint != null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            player.transform.position = spawnPoint.transform.position;
            Debug.Log("Player put to spawnpoint");
        }
        else
        {
            Debug.LogWarning("Spawnpoint not found in scene!");
        }
    }
}
