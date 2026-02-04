using UnityEngine;
using UnityEngine.SceneManagement;


public class SceneManagement : MonoBehaviour
{
    [SerializeField] private string sceneToLoad;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player progressed to next scene: " + sceneToLoad);
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
