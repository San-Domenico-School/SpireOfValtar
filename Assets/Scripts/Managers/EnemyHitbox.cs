using UnityEngine;

/// <summary>
/// Place this on a child hitbox collider to forward damage to the parent's EnemyHealth.
/// This allows the hitbox to be on a separate object that can move independently (e.g., for jump animations).
/// </summary>
public class EnemyHitbox : MonoBehaviour
{
    private EnemyHealth parentHealth;

    void Awake()
    {
        // Find EnemyHealth on parent or ancestors
        parentHealth = GetComponentInParent<EnemyHealth>();
        
        if (parentHealth == null)
        {
            Debug.LogError($"EnemyHitbox on {gameObject.name} could not find EnemyHealth on any parent!");
        }
    }

    /// <summary>
    /// Forward damage to the parent's EnemyHealth component.
    /// This method has the same signature as EnemyHealth.TakeDamage so attack scripts can call it.
    /// </summary>
    public void TakeDamage(float amount)
    {
        if (parentHealth != null)
        {
            parentHealth.TakeDamage(amount);
        }
    }
}
