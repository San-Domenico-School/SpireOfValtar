using UnityEngine;

public class LootDropper : MonoBehaviour
{
    [Header("Drop Settings")]
    [SerializeField] private GameObject dropPrefab;
    [SerializeField] private int minDrop = 1;
    [SerializeField] private int maxDrop = 2;
    [SerializeField] private float dropSpreadRadius = 1.5f;

    private EnemyHealth health;

    private void Awake()
    {
        health = GetComponent<EnemyHealth>();
    }

    private void OnEnable()
    {
        if (health != null)
            health.OnDeath += HandleDeath;
    }

    private void OnDisable()
    {
        if (health != null)
            health.OnDeath -= HandleDeath;
    }

    private void HandleDeath()
    {
        if (dropPrefab == null) return;

        int amountToDrop = Random.Range(minDrop, maxDrop + 1);

        for (int i = 0; i < amountToDrop; i++)
        {
            Vector3 randomOffset = new Vector3(
                Random.Range(-dropSpreadRadius, dropSpreadRadius),
                0.5f,
                Random.Range(-dropSpreadRadius, dropSpreadRadius)
            );

            Instantiate(dropPrefab, transform.position + randomOffset, Quaternion.identity);
        }
    }
}