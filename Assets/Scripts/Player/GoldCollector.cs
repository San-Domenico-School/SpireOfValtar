using UnityEngine;

public class GoldCollector : MonoBehaviour
{
    public static GoldCollector Instance;

    [SerializeField] private int goldCollected = 0;

    private void Awake()
    {
        // Singleton check
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject); // Prevent duplicates
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Gold"))
        {
            goldCollected++;
            Destroy(other.gameObject);

            Debug.Log("Gold C ollected: " + goldCollected);
        }
    }

    public int GetGold()
    {
        return goldCollected;
    }
}