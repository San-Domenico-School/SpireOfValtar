using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class HealthPack : MonoBehaviour
{
    [SerializeField] private int healAmount = 50;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool destroyOnUse = true;

    public event Action<HealthPack> OnConsumed;

    private Collider cachedCollider;
    private bool consumed;

    private void Awake()
    {
        cachedCollider = GetComponent<Collider>();
        if (cachedCollider != null)
        {
            cachedCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (consumed)
        {
            return;
        }

        if (!other.CompareTag(playerTag))
        {
            return;
        }

        var playerHealth = other.GetComponentInParent<PlayerHealth>();
        if (playerHealth == null)
        {
            return;
        }

        if (playerHealth.TryHeal(healAmount))
        {
            Consume();
        }
    }

    private void Consume()
    {
        consumed = true;
        OnConsumed?.Invoke(this);

        if (destroyOnUse)
        {
            Destroy(gameObject);
        }
    }

    private void Reset()
    {
        cachedCollider = GetComponent<Collider>();
        if (cachedCollider != null)
        {
            cachedCollider.isTrigger = true;
        }
    }
}
