using UnityEngine;
using System.Collections;

public class FireballSpell : MonoBehaviour
{
    [Header("Fireball Settings")]
    [SerializeField] private GameObject fireballPrefab; // prefab with sphere collider, particles, etc.
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 10f;
    [SerializeField] private AnimationCurve arcCurve;   // controls vertical arc
    [SerializeField] private float arcHeight = 1f;      // max height multiplier
    [SerializeField] private float cooldown = 3f; // how many seconds between casts
    private bool canCast = true;

    public void OnCast()
    {
        Debug.Log("Casting Fireball!");

        if (!canCast) return; // if still cooling down, do nothing
        StartCoroutine(CastCooldown());

        // Spawn the fireball in front of player’s camera
        Transform cam = Camera.main.transform;
        GameObject fireball = Instantiate(
            fireballPrefab,
            cam.position + cam.forward * 1f, // a little in front of camera
            Quaternion.identity
        );

        // Start movement coroutine
        fireball.GetComponent<FireballSpell>().StartCoroutine(
            fireball.GetComponent<FireballSpell>().MoveRoutine(cam.forward)
        );
    }

    private IEnumerator CastCooldown()
    {
        canCast = false;
        yield return new WaitForSeconds(cooldown);
        canCast = true;
    }

    private IEnumerator MoveRoutine(Vector3 direction)
    {
        Vector3 startPos = transform.position;
        Vector3 forward = direction.normalized;

        float t = 0f;
        while (t < lifetime)
        {
            // Calculate movement for this frame
            Vector3 movement = forward * speed * Time.deltaTime;
            
            // Check for collision using raycast before moving
            RaycastHit hit;
            if (Physics.Raycast(transform.position, forward, out hit, movement.magnitude))
            {
                Debug.Log($"Fireball hit {hit.collider.name} (Tag: {hit.collider.tag})");
                
                if (hit.collider.CompareTag("Enemy"))
                {
                    Debug.Log("Fireball hit an enemy!");
                    // TODO: Add damage dealing logic here
                }
                
                Destroy(gameObject);
                yield break;
            }
            
            // Move fireball forward
              transform.position += forward * speed * Time.deltaTime;

            // Add arc (arcCurve is evaluated 0→1 across lifetime)
            float normalizedTime = t / lifetime;
            float arcOffset = arcCurve.Evaluate(normalizedTime) * arcHeight;
            transform.position = new Vector3(
                transform.position.x,
                startPos.y + arcOffset,
                transform.position.z
            );

            t += Time.deltaTime;
            yield return null;
        }

        Debug.Log("Fireball expired");
        Destroy(gameObject);
    }

}
