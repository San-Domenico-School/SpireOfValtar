using UnityEngine;
/* Sebastian 2/24/26
 * This script is used to collect gold.
 * It is a singleton and will persist across scenes.
 * It will destroy the gold object when it is collected.
 * It will add the gold to the player's gold count.
 * It will update the gold count in the UI.
 * It will save the gold count to the player's data.
 * It will load the gold count from the player's data.
 */
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

            Debug.Log("Gold C ollected: " + goldCollected);
        }
    }

    public int GetGold()
    {
        return goldCollected;
    }

    public void AddGold(int amount)
    {
        goldCollected += amount;
        Debug.Log("Gold Collected: " + goldCollected);
    }

    // Returns true and deducts the amount if the player can afford it.
    public bool SpendGold(int amount)
    {
        if (goldCollected < amount) return false;
        goldCollected -= amount;
        Debug.Log($"[GoldCollector] Spent {amount} gold. Remaining: {goldCollected}");
        return true;
    }
}