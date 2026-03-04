using UnityEngine;
using System.Collections;

public class BossFireballProjectile : MonoBehaviour
{
    [Header("Boss Fireball Settings")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 6f;

    [Header("Arc (optional)")]
    [SerializeField] private AnimationCurve arcCurve;
    [SerializeField] private float arcHeight = 0.5f;

    [Header("Explosion")]
    [SerializeField] private float radius = 3f;
    [SerializeField] private int damage = 15;

    [Tooltip("If true, fireball explodes on any hit (walls/ground). Recommended.")]
    [SerializeField] private bool explodeOnAnyHit = true;

    // Call this after Instantiate
    public void Init(Vector3 direction)
    {
        StopAllCoroutines();
        StartCoroutine(MoveRoutine(direction));
    }

    private IEnumerator MoveRoutine(Vector3 direction)
    {
        Vector3 startPos = transform.position;
        Vector3 forward = direction.normalized;

        float t = 0f;
        float distanceTravelled = 0f;

        while (t < lifetime)
        {
            float dt = Time.deltaTime;
            float step = speed * dt;
            distanceTravelled += step;

            // Base position along forward
            Vector3 basePos = startPos + forward * distanceTravelled;

            // Arc offset (vertical)
            float normalizedTime = lifetime > 0f ? t / lifetime : 0f;
            float curveValue = (arcCurve != null) ? arcCurve.Evaluate(normalizedTime) : 0f;
            float arcOffset = curveValue * arcHeight;

            Vector3 nextPos = basePos + Vector3.up * arcOffset;

            // Raycast from current -> nextPos to avoid tunneling
            Vector3 displacement = nextPos - transform.position;
            float dist = displacement.magnitude;

            if (dist > 0f && Physics.Raycast(transform.position, displacement.normalized, out RaycastHit hit, dist))
            {
                Debug.Log($"BossFireball hit {hit.collider.name} (Tag: {hit.collider.tag})");

                // If we only want to explode on player, gate it here.
                if (explodeOnAnyHit || hit.collider.CompareTag("Player"))
                {
                    Explode(hit.point);
                    Destroy(gameObject);
                    yield break;
                }
            }

            // Apply position & face travel direction
            transform.position = nextPos;
            transform.rotation = Quaternion.LookRotation(forward, Vector3.up);

            t += dt;
            yield return null;
        }

        // Expired: optional explode or just destroy
        Destroy(gameObject);
    }

    private void Explode(Vector3 center)
    {
        // AOE damage: player only
        Collider[] hits = Physics.OverlapSphere(center, radius);
        foreach (Collider c in hits)
        {
            if (!c.CompareTag("Player")) continue;

            var ph = c.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(damage);
                Debug.Log($"BossFireball dealt {damage} to Player");
            }
        }
    }

    // Visualize explosion radius in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}