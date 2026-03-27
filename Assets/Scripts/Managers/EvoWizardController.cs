using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.VFX;

public class EvoWizardController : MonoBehaviour
{
  public enum Phase { Phase1_Fire, Phase2_Lightning, Phase3_MeleeSummon }

  [Header("Core")]
  [SerializeField] private float baseSpeed = 3.5f;
  [SerializeField] private float detectionRange = 20f;
  [SerializeField] private float attackRange = 4f;   // ranged effective range
  [SerializeField] private float meleeRange = 2.0f;   // phase 3 melee distance
  [SerializeField] private float rotationSpeed = 8f;

  [Header("Projectiles")]
  [SerializeField] private GameObject fireProjectilePrefab;
  [SerializeField] private VisualEffect lightningVFXPrefab;
  [SerializeField] private float lightningRange = 20f;
  [SerializeField] private float lightningDamage = 35f;
  [SerializeField] private float boltHeight = 6f;
  [SerializeField] private float vfxLifetime = 2f;
  [SerializeField] private Transform firePoint;
  [SerializeField] private float fireRate = 1f;
  [SerializeField] private int projectileDamage = 12;

  [Header("Boss Lightning (Delayed)")]
  [SerializeField] private float lightningCastDelay = 1.0f;
  [SerializeField] private float lightningHitRadius = 1.0f;
  [SerializeField] private float lightningCooldown = 2.0f;
  private bool isCastingLightning = false;

  [Header("Phase Thresholds (percent)")]
  [Range(0f, 1f)][SerializeField] private float phase2At = 0.70f;
  [Range(0f, 1f)][SerializeField] private float phase3At = 0.40f;

  [Header("Resist / Vulnerability")]
  [Tooltip("Damage multiplier applied to incoming FIRE in Phase 1")]
  [SerializeField] private float phase1FireMultiplier = 0.5f;
  [Tooltip("Damage multiplier applied to incoming LIGHTNING in Phase 2")]
  [SerializeField] private float phase2LightningMultiplier = 0.5f;

  [Header("Phase 3 Buffs")]
  [SerializeField] private float phase3SpeedMultiplier = 1.5f;
  [SerializeField] private int phase3MeleeDamage = 20;
  [SerializeField] private float meleeDamageInterval = 1.25f;

  [Header("Dodging (Phase 3)")]
  [SerializeField] private float dodgeCooldown = 1.2f;
  [SerializeField] private float dodgeDistance = 3.5f;
  [SerializeField] private float dodgeChanceVsProjectiles = 0.65f;

  [Header("Summoning (Phase 3)")]
  [SerializeField] private EnemySpawner wizardSpawner;
  [SerializeField] private float summonInterval = 6f;
  [SerializeField] private int maxSummonAliveFromThisBoss = 6;

  [Header("Phase Visuals")]
  private Renderer[] phaseRenderers;
  [SerializeField] private string colorProperty = "_BaseColor";
  private MaterialPropertyBlock mpb;

  [Header("Animation")]
  [SerializeField] private Animator animator;
  // How long to wait for the phase-switch animation before resuming combat
  [SerializeField] private float phaseSwitchAnimDuration = 1.5f;
  // Set to true while a phase-switch animation is playing so we don't act
  private bool isPhaseTransitioning = false;

  // Components
  private NavMeshAgent navAgent;
  private Transform player;
  private EnemyHealth enemyHealth;

  // State
  public Phase currentPhase { get; private set; } = Phase.Phase1_Fire;
  private bool isPlayerDetected;
  private Coroutine attackRoutine;
  private Coroutine summonRoutine;
  private Coroutine meleeRoutine;
  private float lastDodgeTime = -999f;
  private bool isDead = false;
  private bool hasInitialized = false;

  // Slowed state (set by Freeze spell)
  private float currentSpeedMultiplier = 1f;

  void Start()
  {
    navAgent = GetComponent<NavMeshAgent>();
    enemyHealth = GetComponent<EnemyHealth>();

    // Auto-find animator if not assigned in Inspector
    if (animator == null)
      animator = GetComponentInChildren<Animator>();

    var playerGO = GameObject.FindGameObjectWithTag("Player");
    if (playerGO != null) player = playerGO.transform;

    navAgent.angularSpeed = 360f;
    navAgent.acceleration = 8f;
    navAgent.updateRotation = false;

    EnterPhase(Phase.Phase1_Fire);
    hasInitialized = true;
  }

  void Update()
  {
    if (isDead) return;
    if (navAgent == null || player == null || enemyHealth == null) return;

    // Don't do anything while the phase-switch animation is playing
    if (isPhaseTransitioning) return;

    // ---------- Phase check from HP % ----------
    float hp01 = GetHealth01();

    // Death check
    if (hp01 <= 0f)
    {
      Die();
      return;
    }

    if (hp01 <= phase3At && currentPhase != Phase.Phase3_MeleeSummon)
      EnterPhase(Phase.Phase3_MeleeSummon);
    else if (hp01 <= phase2At && hp01 > phase3At && currentPhase != Phase.Phase2_Lightning)
      EnterPhase(Phase.Phase2_Lightning);
    else if (hp01 > phase2At && currentPhase != Phase.Phase1_Fire)
      EnterPhase(Phase.Phase1_Fire);

    // ---------- Detection ----------
    float dist = Vector3.Distance(transform.position, player.position);
    isPlayerDetected = dist <= detectionRange;

    if (!isPlayerDetected)
    {
      navAgent.SetDestination(transform.position);
      StopAllCombatRoutines();
      if (summonRoutine != null) { StopCoroutine(summonRoutine); summonRoutine = null; }

      // Tell animator we're idle (not moving, not attacking)
      SetAnimMoving(false);
      return;
    }

    // ---------- Chase ----------
    navAgent.SetDestination(player.position);
    FacePlayer();

    // Update movement animation based on whether the agent is actually moving
    bool isMoving = navAgent.velocity.sqrMagnitude > 0.1f;
    SetAnimMoving(isMoving);

    // ---------- Phase behaviors ----------
    if (currentPhase == Phase.Phase3_MeleeSummon)
    {
      TryDodgeIfThreatened();

      if (dist <= meleeRange)
      {
        if (meleeRoutine == null)
          meleeRoutine = StartCoroutine(MeleeDamageRoutine());
      }
      else
      {
        if (meleeRoutine != null)
        {
          StopCoroutine(meleeRoutine);
          meleeRoutine = null;
        }
      }

      if (attackRoutine != null)
      {
        StopCoroutine(attackRoutine);
        attackRoutine = null;
      }
    }
    else
    {
      if (dist <= attackRange)
      {
        if (attackRoutine == null)
          attackRoutine = StartCoroutine(RangedAttackRoutine());
      }
      else
      {
        if (attackRoutine != null)
        {
          StopCoroutine(attackRoutine);
          attackRoutine = null;
        }
      }

      if (meleeRoutine != null)
      {
        StopCoroutine(meleeRoutine);
        meleeRoutine = null;
      }
    }

    navAgent.speed = baseSpeed * currentSpeedMultiplier * GetPhaseSpeedMultiplier();
  }

  // ==================== ANIMATION HELPERS ====================

  void SetAnimMoving(bool moving)
  {
    if (animator == null) return;
    animator.SetBool("isMoving", moving);
  }

  void TriggerAttackAnim()
  {
    if (animator == null) return;
    animator.SetTrigger("Attack");
  }

  void TriggerPhaseSwitch()
  {
    if (animator == null) return;
    animator.SetTrigger("PhaseSwitch");
  }

  void TriggerDeath()
  {
    if (animator == null) return;
    animator.SetTrigger("Die");
  }

  // ==================== DEATH ====================

  void Die()
{
    if (isDead) return;
    isDead = true;

    navAgent.isStopped = true;
    navAgent.velocity = Vector3.zero;
    StopAllCombatRoutines();
    if (summonRoutine != null) { StopCoroutine(summonRoutine); summonRoutine = null; }

    TriggerDeath();

    foreach (var col in GetComponentsInChildren<Collider>())
        col.enabled = false;

    // Also disable the NavMeshAgent so it doesn't interfere
    navAgent.enabled = false;

    Debug.Log("[EvoWizard] Boss died!");

    // Match this to your actual death animation clip length
    // Check the clip duration in the Animation tab of your model import
    Destroy(gameObject, 4f);
}

  // ==================== PHASE MANAGEMENT ====================

  float GetHealth01()
  {
    return Mathf.Clamp01(enemyHealth.currentHealth / enemyHealth.maxHealth);
  }

  float GetPhaseSpeedMultiplier()
  {
    return currentPhase == Phase.Phase3_MeleeSummon ? phase3SpeedMultiplier : 1f;
  }

  void EnterPhase(Phase newPhase)
  {
    currentPhase = newPhase;
    ApplyPhaseVisuals(newPhase);
    StopAllCombatRoutines();

    // Play the phase-switch animation and pause combat briefly
    if (hasInitialized)
    {
      TriggerPhaseSwitch();
      StartCoroutine(PhaseTransitionPause());
    }

    if (currentPhase == Phase.Phase3_MeleeSummon)
    {
      navAgent.stoppingDistance = 0.2f;

      if (wizardSpawner != null)
        wizardSpawner.StartSpawning();

      if (summonRoutine == null)
        summonRoutine = StartCoroutine(SummonRoutine());
    }
    else
    {
      navAgent.stoppingDistance = Mathf.Min(attackRange * 0.6f, 6f);
    }

    // Tell the animator which phase we're in (0, 1, 2) in case you want
    // different idle/attack blend trees per phase later
    animator?.SetInteger("Phase", (int)currentPhase);

    Debug.Log($"[EvoWizard] Entered {currentPhase}");
  }

  IEnumerator PhaseTransitionPause()
  {
    isPhaseTransitioning = true;
    navAgent.isStopped = true;

    yield return new WaitForSeconds(phaseSwitchAnimDuration);

    navAgent.isStopped = false;
    isPhaseTransitioning = false;
  }

  void StopAllCombatRoutines()
  {
    if (attackRoutine != null) { StopCoroutine(attackRoutine); attackRoutine = null; }
    if (meleeRoutine != null) { StopCoroutine(meleeRoutine); meleeRoutine = null; }
    if (currentPhase != Phase.Phase3_MeleeSummon && summonRoutine != null)
    {
      StopCoroutine(summonRoutine);
      summonRoutine = null;
    }
  }

  // ==================== COMBAT ====================

  IEnumerator RangedAttackRoutine()
  {
    float delay = 1f / Mathf.Max(0.01f, fireRate);

    while (true)
    {
      FacePlayer();
      TriggerAttackAnim();
      yield return new WaitForSeconds(delay);
    }
  }

  void FacePlayer()
  {
    Vector3 dir = player.position - transform.position;
    dir.y = 0f;
    if (dir.sqrMagnitude < 0.01f) return;

    Quaternion targetRot = Quaternion.LookRotation(dir);
    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
  }

  void ShootProjectile(GameObject prefab)
  {
    if (prefab == null || player == null) return;

    Transform spawnPoint = firePoint != null ? firePoint : transform;
    GameObject projGO = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);

    Vector3 dir = (player.position - spawnPoint.position).normalized;

    var bossFireball = projGO.GetComponent<BossFireballProjectile>();
    if (bossFireball != null)
    {
      bossFireball.Init(dir);
      return;
    }

    var fireball = projGO.GetComponent<FireballProjectile>();
    if (fireball != null)
    {
      fireball.StartCoroutine(fireball.MoveRoutine(dir));
      return;
    }

    Debug.LogWarning($"Spawned projectile {prefab.name} but it has no recognized projectile script.");
  }

  private void CastBossLightning()
  {
    if (isCastingLightning) return;
    StartCoroutine(DelayedLightningRoutine());
  }

  private IEnumerator DelayedLightningRoutine()
  {
    isCastingLightning = true;

    Vector3 targetPos = player.position;

    yield return new WaitForSeconds(lightningCastDelay);

    SpawnLightning(targetPos);

    if (Vector3.Distance(player.position, targetPos) <= lightningHitRadius)
    {
      var playerHealth = player.GetComponent<PlayerHealth>();
      if (playerHealth != null)
        playerHealth.TakeDamage((int)lightningDamage);
    }

    yield return new WaitForSeconds(lightningCooldown);
    isCastingLightning = false;
  }

  private void SpawnLightning(Vector3 hitPoint)
  {
    if (lightningVFXPrefab == null) return;

    VisualEffect vfx = Instantiate(lightningVFXPrefab, hitPoint, Quaternion.identity);
    vfx.transform.up = Vector3.up;

    if (vfx.HasVector3("StartPosition")) vfx.SetVector3("StartPosition", hitPoint);
    if (vfx.HasVector3("TargetPosition")) vfx.SetVector3("TargetPosition", hitPoint + Vector3.up * boltHeight);

    vfx.Play();
    Destroy(vfx.gameObject, vfxLifetime);
  }

  IEnumerator MeleeDamageRoutine()
  {
    while (true)
    {
      navAgent.isStopped = true;
      SetAnimMoving(false);

      TriggerAttackAnim();

      yield return new WaitForSeconds(meleeDamageInterval);

      navAgent.isStopped = false;
    }
  }

  void TryDodgeIfThreatened()
  {
    if (currentSpeedMultiplier < 0.6f) return;
    if (Time.time < lastDodgeTime + dodgeCooldown) return;

    Transform cam = Camera.main != null ? Camera.main.transform : null;
    if (cam == null) return;

    Vector3 toBoss = (transform.position - cam.position).normalized;
    float aimDot = Vector3.Dot(cam.forward, toBoss);

    if (aimDot > 0.85f && Random.value < dodgeChanceVsProjectiles)
    {
      Vector3 right = Vector3.Cross(Vector3.up, (player.position - transform.position).normalized);
      Vector3 dodgeDir = (Random.value < 0.5f) ? right : -right;

      Vector3 target = transform.position + dodgeDir * dodgeDistance;

      navAgent.Warp(target);
      lastDodgeTime = Time.time;
    }
  }

  IEnumerator SummonRoutine()
  {
    while (currentPhase == Phase.Phase3_MeleeSummon)
    {
      if (wizardSpawner != null)
      {
        wizardSpawner.TrySpawnOne();
      }
      yield return new WaitForSeconds(summonInterval);
    }
  }

  // ==================== DAMAGE / RESIST ====================

  public void ApplyTypedDamage(float amount, DamageType type)
  {
    float final_amount = amount;

    if (currentPhase == Phase.Phase1_Fire && type == DamageType.Fire)
      final_amount *= phase1FireMultiplier;

    if (currentPhase == Phase.Phase2_Lightning)
    {
      if (type == DamageType.Lightning) final_amount *= phase2LightningMultiplier;
      if (type == DamageType.Fire) final_amount *= 1.25f;
    }

    enemyHealth.TakeDamage(final_amount);
  }

  // ==================== FREEZE ====================

  public void SetSpeedMultiplier(float multiplier)
  {
    currentSpeedMultiplier = Mathf.Clamp(multiplier, 0.1f, 2f);

    // Slow the animation speed to match movement slowdown
    if (animator != null)
      animator.speed = currentSpeedMultiplier;
  }

  private Coroutine freezeTimer;

  public void ApplyFreezeEffect(float slowAmount, float duration)
  {
    Debug.Log($"[EvoWizard] Freeze applied! phase={currentPhase} slow={slowAmount} duration={duration}", this);

    if (freezeTimer != null) StopCoroutine(freezeTimer);
    SetSpeedMultiplier(1f - slowAmount);
    freezeTimer = StartCoroutine(FreezeTimer(duration));
  }

  IEnumerator FreezeTimer(float duration)
  {
    yield return new WaitForSeconds(duration);
    SetSpeedMultiplier(1f);
  }

  // ==================== VISUALS ====================

  private void ApplyPhaseVisuals(Phase phase)
  {
    if (phaseRenderers == null || phaseRenderers.Length == 0)
      phaseRenderers = GetComponentsInChildren<Renderer>(true);

    if (mpb == null) mpb = new MaterialPropertyBlock();

    Color c = new Color(1f, 0.4f, 0.2f);
    switch (phase)
    {
      case Phase.Phase1_Fire: c = new Color(1f, 0.4f, 0.2f); break;
      case Phase.Phase2_Lightning: c = new Color(0.7f, 0.8f, 1f); break;
      case Phase.Phase3_MeleeSummon: c = new Color(0.6f, 1f, 0.6f); break;
    }

    foreach (var r in phaseRenderers)
    {
      if (r == null) continue;
      r.GetPropertyBlock(mpb);
      mpb.SetColor(colorProperty, c);
      r.SetPropertyBlock(mpb);
    }
  }

  // Called by Animation Event on the attack clip
  public void OnAttackHit()
  {
    if (isDead || isPhaseTransitioning) return;

    if (currentPhase == Phase.Phase1_Fire)
    {
      ShootProjectile(fireProjectilePrefab);
    }
    else if (currentPhase == Phase.Phase2_Lightning)
    {
      CastBossLightning();
    }
    else if (currentPhase == Phase.Phase3_MeleeSummon)
    {
      var playerHealth = FindFirstObjectByType<PlayerHealth>();
      float dist = Vector3.Distance(transform.position, player.position);
      if (playerHealth != null && dist <= meleeRange)
        playerHealth.TakeDamage(phase3MeleeDamage);
    }
  }
}