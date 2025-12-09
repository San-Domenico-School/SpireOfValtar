using UnityEngine;

public class WizardProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 10f;
    public int damage = 10;
    public float lifeTime = 5f;

    private Vector3 direction;

    public void Init(Vector3 dir)
    {
        direction = dir.normalized;
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only care about the player
        if (other.CompareTag("Player"))
        {
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }

            // Destroy the projectile after it hits
            Destroy(gameObject);
        }
        else if (!other.isTrigger)
        {
            // Hit a wall / environment – destroy projectile
            Destroy(gameObject);
        }
    }
}
