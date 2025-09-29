using UnityEngine;
using System.Collections;

public class FireballProjectile : MonoBehaviour
{
    [Header("Fireball Projectile Settings")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 10f;
    [SerializeField] private AnimationCurve arcCurve;   
    [SerializeField] private float arcHeight = 1f;      

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
                    Debug.Log("Fireball hit an enemy!");
                    // damage here
                }
                
                // Destroy only this fireball projectile
                Destroy(gameObject);
                yield break;
            }
            
            
            transform.position += movement;

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
        // Destroy only this fireball projectile
        Destroy(gameObject);
    }
}