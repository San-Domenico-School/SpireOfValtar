using UnityEngine;
using System.Collections;

public class FreezeProjectile : MonoBehaviour
{
    [Header("Freeze Projectile Settings")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 10f;
    [SerializeField] private AnimationCurve arcCurve;   
    [SerializeField] private float arcHeight = 1f;
    [SerializeField] private float freezeSlowAmount = 0.2f; // 20% slower (0.8x speed)
    [SerializeField] private float freezeDuration = 5f; // How long the freeze effect lasts

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
                Debug.Log($"Freeze spell hit {hit.collider.name} (Tag: {hit.collider.tag})");
                
                if (hit.collider.CompareTag("Enemy"))
                {
                    Debug.Log("Freeze spell hit an enemy!");
                    
                    // Get the enemy's EnemyController and apply freeze effect
                    EnemyController enemyController = hit.collider.GetComponent<EnemyController>();
                    if (enemyController != null)
                    {
                        // Apply freeze effect with timer management
                        enemyController.ApplyFreezeEffect(freezeSlowAmount, freezeDuration);
                    }
                }
                
                // Destroy the freeze projectile
                Destroy(gameObject);
                yield break;
            }
            
            transform.position += movement;

            float normalizedTime = t / lifetime;
            float arcOffset = arcCurve.Evaluate(normalizedTime) * arcHeight;
            transform.position = new Vector3(
                transform.position.x,
                startPos.y + arcOffset,
                transform.position.z
            );

            t += Time.deltaTime;
            yield return null;
        }

        Debug.Log("Freeze spell expired");
        Destroy(gameObject);
    }
    
}