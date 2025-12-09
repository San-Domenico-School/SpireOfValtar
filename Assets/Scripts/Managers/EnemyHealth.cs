using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("Enemy Settings")]
    public float maxHealth = 100f;
    public float currentHealth;


    private Renderer enemyRenderer;
    private Color originalColor;
    private bool isFlashing = false;

    [SerializeField] FloatingHealthBar healthBar;
    [SerializeField] private GameObject deathParticle;

    private int enemyKillsNeeded = 5;
    private int enemiesKilled = 0;

    private TeleporterActivater teleporterActivater;

    [SerializeField] public GameObject objectToActivate;

    private void Awake()
    {
        healthBar = GetComponentInChildren<FloatingHealthBar>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
        enemyRenderer = GetComponent<Renderer>();
        if (enemyRenderer != null)
            originalColor = enemyRenderer.material.color;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        healthBar.SliderUpdate(currentHealth, maxHealth);
        Debug.Log($"{gameObject.name} took {amount} damage! Remaining health: {currentHealth}");

        if (!isFlashing && enemyRenderer != null)
            StartCoroutine(FlashRed());

        if (currentHealth <= 0)
            Die();
        Debug.Log(currentHealth);
    }

    private IEnumerator FlashRed()
    {
        isFlashing = true;
        enemyRenderer.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        enemyRenderer.material.color = originalColor;
        isFlashing = false;
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} has been defeated!");
        if (deathParticle != null)
        {
            Instantiate(deathParticle, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
        EnemyKilled();
    }

    public void EnemyKilled()
    {
        enemiesKilled++;
        Debug.Log("+1 Enemy Killed");

        if (enemiesKilled >= enemyKillsNeeded)
        {
            objectToActivate.SetActive(true);
            Debug.Log("Door opened!");
        }
    }
}
