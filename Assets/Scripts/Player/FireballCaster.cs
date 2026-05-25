using UnityEngine;
using System.Collections;

public class FireballCaster : MonoBehaviour
{
    [Header("Fireball Caster Settings")]
    [SerializeField] private GameObject fireballPrefab; 
    [SerializeField] private float cooldown = 3f; 
    private bool isOnCooldown = false;

    [Header("Audio (optional)")]
    [SerializeField] private SpellAudioController spellAudio;

    public bool canCast => !isOnCooldown && Time.timeScale > 0f;

    private void Awake()
    {
        if (spellAudio == null) spellAudio = GetComponentInParent<SpellAudioController>();
    }

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
            projectile.Init(spellAudio, SpellSfxId.Fireball);
            projectile.StartCoroutine(projectile.MoveRoutine(cam.forward));
        }
    }

    private IEnumerator CastCooldown()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(cooldown);
        isOnCooldown = false;
    }


}
