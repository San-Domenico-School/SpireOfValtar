using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class WizardController : MonoBehaviour
{
  [Header("Enemy Settings")]
  [SerializeField] private float baseSpeed = 3.5f;
  [SerializeField] private float detectionRange = 10f;
  [SerializeField] private float attackRange = 2f;
  [SerializeField] private float rotationSpeed = 5f;

  [Header("Ranged Attack Settings")]
  [SerializeField] private GameObject projectilePrefab;
  [SerializeField] private Transform firePoint;
  [SerializeField] private float fireRate = 1f;
  [SerializeField] private int projectileDamage = 10;

  [Header("Speed Modifiers")]
  [SerializeField] private float currentSpeedMultiplier = 1f;

  private NavMeshAgent navAgent;
  private Transform player;

  private bool isPlayerDetected = false;
  private bool isAttacking = false;

  private Coroutine freezeTimer = null;
  private Coroutine attackRoutine;

  private PlayerHealth playerHealth;
  public int damage = 10;

  [Header("Audio (optional)")]
  [SerializeField] private EnemySoundController enemySound;

  void Start()
  {
    playerHealth = FindFirstObjectByType<PlayerHealth>();

    navAgent = GetComponent<NavMeshAgent>();
    if (enemySound == null) enemySound = GetComponent<EnemySoundController>();
    if (enemySound == null) enemySound = GetComponentInParent<EnemySoundController>();
    if (enemySound == null) enemySound = GetComponentInChildren<EnemySoundController>();

    GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
    if (playerObject != null)
      player = playerObject.transform;

    navAgent.speed = baseSpeed * currentSpeedMultiplier;
    navAgent.angularSpeed = 360f;
    navAgent.acceleration = 8f;
    navAgent.stoppingDistance = attackRange;
    navAgent.SetDestination(transform.position);
  }

  void Update()
  {
    if (navAgent == null || player == null) return;

    float distanceToPlayer = Vector3.Distance(transform.position, player.position);
    isPlayerDetected = distanceToPlayer <= detectionRange;

    if (isPlayerDetected)
    {
      navAgent.SetDestination(player.position);

      if (distanceToPlayer <= attackRange && !isAttacking)
        StartAttack();
      else if (distanceToPlayer > attackRange && isAttacking)
        StopAttack();
    }
    else
    {
      navAgent.SetDestination(transform.position);
      if (isAttacking)
        StopAttack();
    }

    navAgent.speed = baseSpeed * currentSpeedMultiplier;
  }

  private void StartAttack()
  {
    isAttacking = true;
    if (attackRoutine == null)
      attackRoutine = StartCoroutine(AttackRoutine());
  }

  private void StopAttack()
  {
    isAttacking = false;
    if (attackRoutine != null)
    {
      StopCoroutine(attackRoutine);
      attackRoutine = null;
    }
  }

  private IEnumerator AttackRoutine()
  {
    float delay = 1f / fireRate;

    while (isAttacking)
    {
      FacePlayer();
      ShootProjectile();
      yield return new WaitForSeconds(delay);
    }
  }

  private void FacePlayer()
  {
    Vector3 dir = player.position - transform.position;
    dir.y = 0f;

    if (dir.sqrMagnitude > 0.01f)
    {
      Quaternion targetRot = Quaternion.LookRotation(dir);
      transform.rotation = Quaternion.Slerp(
          transform.rotation,
          targetRot,
          rotationSpeed * Time.deltaTime
      );
    }
  }

  private void ShootProjectile()
  {
    if (projectilePrefab == null || player == null) return;

    if (enemySound != null) enemySound.PlayAttackSfx();

    Transform spawnPoint = firePoint != null ? firePoint : transform;

    GameObject projGO = Instantiate(
        projectilePrefab,
        spawnPoint.position,
        spawnPoint.rotation
    );

    WizardProjectile proj = projGO.GetComponent<WizardProjectile>();
    if (proj != null)
    {
      Vector3 direction = player.position - spawnPoint.position;
      proj.damage = projectileDamage;
      proj.Init(direction);
    }
  }

  public void SetSpeedMultiplier(float multiplier)
  {
    float oldMultiplier = currentSpeedMultiplier;
    currentSpeedMultiplier = Mathf.Clamp(multiplier, 0.1f, 2f);

    if (oldMultiplier != currentSpeedMultiplier)
    {
      float newSpeed = baseSpeed * currentSpeedMultiplier;

      if (currentSpeedMultiplier < 1f)
        Debug.Log($"❄️ {gameObject.name} SLOWED! Speed: {newSpeed:F2}");
      else if (currentSpeedMultiplier == 1f && oldMultiplier < 1f)
        Debug.Log($"🔥 {gameObject.name} speed RESTORED! Speed: {newSpeed:F2}");
    }
  }

  public void ApplyFreezeEffect(float slowAmount, float duration)
  {
    if (freezeTimer != null)
      StopCoroutine(freezeTimer);

    SetSpeedMultiplier(1f - slowAmount);
    freezeTimer = StartCoroutine(FreezeTimer(duration));
  }

  private IEnumerator FreezeTimer(float duration)
  {
    yield return new WaitForSeconds(duration);
    SetSpeedMultiplier(1f);
    freezeTimer = null;
  }
}
