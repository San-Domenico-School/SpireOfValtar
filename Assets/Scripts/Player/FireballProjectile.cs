using UnityEngine;
using System.Collections;

public class FireballProjectile : MonoBehaviour
{
    [Header("Fireball Projectile Settings")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 10f;
    [SerializeField] private AnimationCurve arcCurve;
    [SerializeField] private float arcHeight = 1f;
    [SerializeField] private float fireballRadius = 1.5f; // AOE
    [SerializeField] private float damage = 35f;

    [Header("Impact VFX")]
    [Tooltip("Particle System prefab to spawn on impact.")]
    [SerializeField] private GameObject impactVfxPrefab;
    [Tooltip("If true, spawn impact VFX when lifetime expires too.")]
    [SerializeField] private bool spawnVfxOnExpire = false;
    [SerializeField] private float impactVfxLifetime = 3f; // safety cleanup

    private SpellAudioController spellAudio;
    private SpellSfxId spellId = SpellSfxId.Fireball;

    public void Init(SpellAudioController audio, SpellSfxId id)
    {
        spellAudio = audio;
        spellId = id;
    }

    public IEnumerator MoveRoutine(Vector3 direction)
    {
        Vector3 forward = direction.normalized;

        float t = 0f;
        while (t < lifetime)
        {
            Vector3 movement = forward * speed * Time.deltaTime;

            if (Physics.Raycast(transform.position, forward, out RaycastHit hit, movement.magnitude))
            {
                Debug.Log($"Fireball hit {hit.collider.name} (Tag: {hit.collider.tag})");

                // Always spawn impact VFX on any collision
                SpawnImpactVfx(hit.point, hit.normal);

                // Optional hit SFX (you can choose to play on any hit or only enemy hits)
                if (spellAudio != null) spellAudio.PlayHit(spellId, hit.point);

                // AOE damage on any collision
                Debug.Log("Fireball impact! Applying AOE damage...");
                Collider[] hitColliders = Physics.OverlapSphere(hit.point, fireballRadius);
                foreach (Collider nearby in hitColliders)
                {
                    EnemyHealth enemyHealth = nearby.GetComponentInParent<EnemyHealth>();
                    if (enemyHealth == null) continue;

                    enemyHealth.TakeDamage(damage);
                    Debug.Log($"Dealt {damage} damage to {nearby.name}");
                }

                Destroy(gameObject);
                yield break;
            }

            // Move
            float normalizedTime = t / lifetime;

            Vector3 planarMove = forward * speed * Time.deltaTime;
            transform.position += planarMove;

            float arcOffset = (arcCurve != null ? arcCurve.Evaluate(normalizedTime) : 0f) * arcHeight;
            transform.position += Vector3.down * arcOffset;

            t += Time.deltaTime;
            yield return null;
        }

        // Lifetime expired
        if (spawnVfxOnExpire)
            SpawnImpactVfx(transform.position, Vector3.up);

        Destroy(gameObject);
    }

    private void SpawnImpactVfx(Vector3 position, Vector3 normal)
    {
        if (impactVfxPrefab == null) return;

        // Rotate so the particle faces outward from the surface
        Quaternion rot = Quaternion.LookRotation(normal, Vector3.up);

        GameObject vfx = Instantiate(impactVfxPrefab, position, rot);

        // Safety cleanup in case particle prefab doesn't self-destroy
        Destroy(vfx, impactVfxLifetime);
    }
}