using UnityEngine;
using System.Collections;

public class FreezeCaster : MonoBehaviour
{
    [Header("Freeze Caster Settings")]
    [SerializeField] private GameObject freezePrefab;
    [SerializeField] private float cooldown = 3f;
    [SerializeField] private float spawnDistance = 1f;

    private bool isOnCooldown = false;

    public bool canCast => !isOnCooldown && Time.timeScale > 0f;

    public void OnCast()
    {
        if (!canCast) return;
        StartCoroutine(CastCooldown());
        
        Transform cam = Camera.main.transform;

        // Spawn in front of the camera, facing the camera's forward
        Vector3 spawnPos = cam.position + cam.forward * spawnDistance;
        Quaternion spawnRot = Quaternion.LookRotation(cam.forward, Vector3.up);

        GameObject instance = Instantiate(freezePrefab, spawnPos, spawnRot);

        // The projectile should handle its own movement/behavior
        // If FreezeProjectile needs initialization, add a method there
    }

    private IEnumerator CastCooldown()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(cooldown);
        isOnCooldown = false;
    }
}
