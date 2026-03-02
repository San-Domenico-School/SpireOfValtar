// v1 Gleb created
// Attach to CoinsPlaceholder (or any gold drop prefab).
// When the player walks into the trigger, adds 1 gold to GoldCollector and destroys this object.

using UnityEngine;

public class GoldPickup : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (GoldCollector.Instance != null)
        {
            GoldCollector.Instance.AddGold(1);
        }

        Destroy(gameObject);
    }
}
