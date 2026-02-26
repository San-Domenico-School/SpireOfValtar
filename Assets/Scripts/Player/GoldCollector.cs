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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Gold"))
        {
            goldCollected++;
            Destroy(collision.gameObject);

            Debug.Log("Gold Collected: " + goldCollected);
        }
    }

    public int GetGold()
    {
        return goldCollected;
    }
}