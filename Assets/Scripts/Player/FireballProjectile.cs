using UnityEngine;
using System.Collections;

public class FireballProjectile : MonoBehaviour
{
    [Header("Fireball Projectile Settings")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 10f;
    [SerializeField] private AnimationCurve arcCurve;
    [SerializeField] private float arcHeight = 1f;

    [SerializeField] private float damage = 50f; // Added: fireball damage

    public IEnumerator MoveRoutine(Vector3 direction)
    {
        Vector3 startPos = transform.position;
        Vector3 forward = direction.normalized;

        float t = 0f;
        while (t < lifetime)
        {
            Vector3 movement = forward * speed * Time.deltaTime;

            RaycastHit hit;
            if (Physics.Raycast(transform.position, forward, out hit, movement.magnitude))
            {
                Debug.Log($"Fireball hit {hit.collider.name} (Tag: {hit.collider.tag})");

                if (hit.collider.CompareTag("Enemy"))
                {
                    Debug.Log("Fireball hit an enemy!");

                    // DEAL DAMAGE
                    EnemyHealth enemyHealth = hit.collider.GetComponent<EnemyHealth>();
                    if (enemyHealth != null)
                    {
                        enemyHealth.TakeDamage(damage);
                        Debug.Log($"Dealt {damage} damage to {hit.collider.name}");
                    }
                    else
                    {
                        Debug.LogWarning("Enemy hit but no EnemyHealth component found!");
                    }
                }

                // Destroy only this fireball projectile
                Destroy(gameObject);
                yield break;
            }

            float normalizedTime = t / lifetime;

            // Move horizontally along the forward vector
            Vector3 planarMove = forward * speed * Time.deltaTime;
            transform.position += planarMove;

            // Apply vertical arc relative to the fireball’s forward distance
            float arcOffset = arcCurve.Evaluate(normalizedTime) * arcHeight;
            transform.position += Vector3.down * arcOffset;

            t += Time.deltaTime;
            yield return null;
        }

        Debug.Log("Fireball expired");
        // Destroy only this fireball projectile
        Destroy(gameObject);
    }
}
