using UnityEngine;
using System.Collections;
using UnityEngine.VFX; // for VisualEffect

public class LightningSpell : MonoBehaviour
{
    [Header("Lightning Settings")]
    public float range = 20f;
    public float cooldown = 3f;
    public float damage = 35f; // Balanced for souls-like: High single-target burst damage

    [Header("VFX")]
    public VisualEffect lightningVFXPrefab; // assign your LightningStrike prefab here
    public float boltHeight = 6f;           // used if your graph exposes Start/Target
    public float vfxLifetime = 2f;          // safety cleanup if Stop Action != Destroy

    private bool isOnCooldown = false;

    [Header("Audio (optional)")]
    [SerializeField] private SpellAudioController spellAudio;

    public bool canCast => !isOnCooldown && Time.timeScale > 0f;

    private void Awake()
    {
        if (spellAudio == null) spellAudio = GetComponentInParent<SpellAudioController>();
    }

    // This is the method called by PlayerAbilityController
    public void OnCast()
    {
        if (!canCast)
        {
            Debug.Log("Lightning is on cooldown or game is paused!");
            return;
        }

        FireRaycast();
        StartCoroutine(CooldownCoroutine());
    }

    private void FireRaycast()
    {
        Transform cam = Camera.main.transform;
        Vector3 origin = cam.position;
        Vector3 direction = cam.forward;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, range))
        {
            Debug.Log($"Lightning hit {hit.collider.name}!");

            // Spawn vertical VFX exactly at the hit point
            SpawnLightning(hit.point);

            // Apply damage if the hit object is an enemy
            if (hit.collider.CompareTag("Enemy"))
            {
                if (spellAudio != null) spellAudio.PlayHit(SpellSfxId.Lightning, hit.point);
                
                // Try to get EnemyHealth directly, or via EnemyHitbox proxy, or from parent
                EnemyHealth enemyHealth = hit.collider.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(damage);
                }
                else
                {
                    // Check for EnemyHitbox proxy (for child hitboxes)
                    EnemyHitbox hitbox = hit.collider.GetComponent<EnemyHitbox>();
                    if (hitbox != null)
                    {
                        hitbox.TakeDamage(damage);
                    }
                    else
                    {
                        // Fallback: check parent for EnemyHealth
                        enemyHealth = hit.collider.GetComponentInParent<EnemyHealth>();
                        if (enemyHealth != null)
                        {
                            enemyHealth.TakeDamage(damage);
                        }
                        else
                        {
                            Debug.LogWarning("Enemy hit but no EnemyHealth component found!");
                        }
                    }
                }
            }
        }
        else
        {
            Debug.Log("Lightning missed everything.");
        }
    }

    private void SpawnLightning(Vector3 hitPoint)
    {
        if (lightningVFXPrefab == null)
        {
            Debug.LogWarning("Lightning VFX Prefab is not assigned.");
            return;
        }

        // Instantiate exactly at the hit point; start with identity rotation
        VisualEffect vfx = Instantiate(lightningVFXPrefab, hitPoint, Quaternion.identity);

        // Keep it perfectly vertical no matter what
        vfx.transform.up = Vector3.up;

        // If your graph exposes positions, set a vertical segment from hitPoint upwards
        if (vfx.HasVector3("StartPosition"))  vfx.SetVector3("StartPosition", hitPoint);
        if (vfx.HasVector3("TargetPosition")) vfx.SetVector3("TargetPosition", hitPoint + Vector3.up * boltHeight);

        // If your prefab’s bolt is authored along +Z instead of +Y, uncomment this line:
        // vfx.transform.rotation = Quaternion.FromToRotation(Vector3.forward, Vector3.up);

        vfx.Play();

        // Safety cleanup (use Stop Action = Destroy on the Visual Effect for best results)
        Destroy(vfx.gameObject, vfxLifetime);
    }

    private IEnumerator CooldownCoroutine()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(cooldown);
        isOnCooldown = false;
        Debug.Log("Lightning ready!");
    }
}