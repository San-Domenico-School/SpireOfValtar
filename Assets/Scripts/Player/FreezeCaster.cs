using UnityEngine;
using System.Collections;

public class FreezeCaster : MonoBehaviour
{
    [Header("Freeze Caster Settings")]
    [SerializeField] private GameObject freezePrefab;
    [SerializeField] private float cooldown = 3f;
    [SerializeField] private float spawnDistance = 1f;

    private bool canCast = true;

    public void OnCast()
    {
        if (!canCast) return;
        StartCoroutine(CastCooldown());

        Transform cam = Camera.main.transform;

        // Spawn in front of the camera, facing the camera's forward
        Vector3 spawnPos = cam.position + cam.forward * spawnDistance;
        Quaternion spawnRot = Quaternion.LookRotation(cam.forward, Vector3.up);

        GameObject instance = Instantiate(freezePrefab, spawnPos, spawnRot);

        // Find the projectile script (root or child)
        FreezeProjectile projectile = instance.GetComponentInChildren<FreezeProjectile>();
        if (projectile != null)
        {
            projectile.Launch(cam.forward);
        }
        else
        {
            Debug.LogError("FreezePrefab has no FreezeProjectile component on it or its children!");
        }
    }

    private IEnumerator CastCooldown()
    {
        canCast = false;
        yield return new WaitForSeconds(cooldown);
        canCast = true;
    }
}
