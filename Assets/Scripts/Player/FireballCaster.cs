using UnityEngine;
using System.Collections;

public class FireballCaster : MonoBehaviour
{
    [Header("Fireball Caster Settings")]
    [SerializeField] private GameObject fireballPrefab; 
    [SerializeField] private float cooldown = 3f; 
    private bool canCast = true;

    public void OnCast()
    {
        Debug.Log("Casting Fireball!");

        if (!canCast) return; 
        StartCoroutine(CastCooldown());

       
        Transform cam = Camera.main.transform;
        GameObject fireball = Instantiate(
            fireballPrefab,
            cam.position + cam.forward * 1f, 
            Quaternion.identity
        );

        // Start movement of fireball projectile
        FireballProjectile projectile = fireball.GetComponent<FireballProjectile>();
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
