using UnityEngine;
using System.Collections;

public class FireballProjectile : MonoBehaviour
{
    [Header("Fireball Projectile Settings")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 10f;
    [SerializeField] private AnimationCurve arcCurve;
    [SerializeField] private float arcHeight = 1f;
    [SerializeField] private float fireballRadius = 5f; // AOE

    [SerializeField] private float damage = 35f; // Balanced for souls-like: AOE damage

    private SpellAudioController spellAudio;
    private SpellSfxId spellId = SpellSfxId.Fireball;

    public void Init(SpellAudioController audio, SpellSfxId id)
    {
        spellAudio = audio;
        spellId = id;
    }

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
                    Debug.Log("Fireball hit an enemy! Applying AOE damage...");
                    if (spellAudio != null) spellAudio.PlayHit(spellId, hit.point);

                    // AOE Damage - damage all enemies within radius
                    Collider[] hitColliders = Physics.OverlapSphere(hit.point, fireballRadius);
                    foreach (Collider nearby in hitColliders)
                    {
                        if (nearby.CompareTag("Enemy"))
                        {
                            EnemyHealth enemyHealth = nearby.GetComponent<EnemyHealth>();
                            if (enemyHealth == null)
                            {
                                enemyHealth = nearby.GetComponentInParent<EnemyHealth>();
                            }
                            if (enemyHealth != null)
                            {
                                enemyHealth.TakeDamage(damage);
                                Debug.Log($"Dealt {damage} damage to {nearby.name}");
                            }
                        }
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
