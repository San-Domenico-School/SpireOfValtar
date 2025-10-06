using UnityEngine;
using System.Collections;

public class FreezeCaster : MonoBehaviour
{
    [Header("Freeze Caster Settings")]
    [SerializeField] private GameObject freezePrefab; 
    [SerializeField] private float cooldown = 3f; 
    private bool canCast = true;

    public void OnCast()
    {
        Debug.Log("Casting Freeze Spell!");

        if (!canCast) return; 
        StartCoroutine(CastCooldown());

        Transform cam = Camera.main.transform;
        GameObject freezeProjectile = Instantiate(
            freezePrefab,
            cam.position + cam.forward * 1f, 
            Quaternion.identity
        );

        // Start movement of freeze projectile
        FreezeProjectile projectile = freezeProjectile.GetComponent<FreezeProjectile>();
        if (projectile != null)
        {
            projectile.StartCoroutine(projectile.MoveRoutine(cam.forward));
        }
    }

    private IEnumerator CastCooldown()
    {
        canCast = false;
        yield return new WaitForSeconds(cooldown);
        canCast = true;
    }
}
