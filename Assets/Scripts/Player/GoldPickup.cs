// v1 Gleb created
// Attach to CoinsPlaceholder prefab.
// Works with CharacterController players by polling proximity each frame.
// OnTriggerEnter is a bonus fallback in case a trigger overlap fires too.

using UnityEngine;

public class GoldPickup : MonoBehaviour
{
    private bool collected = false;
    private Transform playerTransform;
    private const float pickupRadius = 1.2f;

    private void Start()
    {
        var player = GameObject.FindWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    private void Update()
    {
        if (collected || playerTransform == null) return;

        if (Vector3.Distance(transform.position, playerTransform.position) <= pickupRadius)
            Collect();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collected) return;
        if (other.CompareTag("Player")) Collect();
    }

    private void Collect()
    {
        collected = true;
        if (GoldCollector.Instance != null)
            GoldCollector.Instance.AddGold(1);
        Destroy(gameObject);
    }
}
