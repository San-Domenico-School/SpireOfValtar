using UnityEngine;
using System.Collections;

public class LightningSpell : MonoBehaviour
{
    [Header("Lightning Settings")]
    public float range = 20f;
    public float cooldown = 3f;
    public float damage = 100f; // Added: damage value

    private bool isOnCooldown = false;

    public bool canCast => !isOnCooldown && Time.timeScale > 0f;

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
        Transform playerCamera = Camera.main.transform;
        Vector3 origin = playerCamera.position;
        Vector3 direction = playerCamera.forward;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, range))
        {
            Debug.Log($"Lightning hit {hit.collider.name}!");

            if (hit.collider.CompareTag("Enemy"))
            {
                Debug.Log("Hit an enemy!");

                // Added: damage handling
                EnemyHealth enemyHealth = hit.collider.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(damage);
                }
                else
                {
                    Debug.LogWarning("Enemy hit but no EnemyHealth component found!");
                }
            }
            else
            {
                Debug.Log("Hit something else.");
            }
        }
        else
        {
            Debug.Log("Lightning missed everything.");
        }
    }

    private IEnumerator CooldownCoroutine()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(cooldown);
        isOnCooldown = false;
        Debug.Log("Lightning ready!");
    }
}
