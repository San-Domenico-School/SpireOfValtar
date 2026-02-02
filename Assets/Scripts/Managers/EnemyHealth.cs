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

  [SerializeField] private int enemyKillsNeeded = 1;
  [SerializeField] private int enemiesKilled = 0;


  private Stairs stairs;

  private void Awake()
  {
    healthBar = GetComponentInChildren<FloatingHealthBar>();

    GameObject stairsGO = GameObject.Find("Great Hall new Materials_001");
    if (stairsGO != null)
    {
      stairs = stairsGO.GetComponent<Stairs>();
      if (stairs == null)
        Debug.LogWarning("Found Great Hall new Materials_001 but it has no Stairs component.", this);
    }
    else
    {
      Debug.LogWarning("Could not find 'Great Hall new Materials_001' in this scene.", this);
    }
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
      if (stairs != null)
      {
        stairs.Progress();
        Debug.Log("Stairs lowered, teleporter active");
      }
      else
      {
        Debug.LogWarning("No Stairs found in this scene — skipping Progress().", this);
      }
    }
  }

  void OnDisable()
  {
    if (!Application.isPlaying) return;

    UnityEngine.Debug.LogWarning(
        $"{name} ({GetType().Name}) was disabled\n{StackTraceUtility.ExtractStackTrace()}",
        this
    );
  }
}