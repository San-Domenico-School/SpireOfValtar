using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.VFX;

public class EvoWizardController : MonoBehaviour
{
  public enum Phase { Phase1_Fire, Phase2_Lightning, Phase3_MeleeSummon }

  [Header("Core")]
  [SerializeField] private float baseSpeed = 3.5f;
  [SerializeField] private float detectionRange = 12f;
  [SerializeField] private float attackRange = 10f;   // ranged effective range
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
  [Range(0f, 1f)][SerializeField] private float phase2At = 0.70f; // <= 70% goes phase 2? (we’ll do 70-40 is phase2)
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
  [SerializeField] private EnemySpawner wizardSpawner; // drag in spawner in scene (or put as child)
  [SerializeField] private float summonInterval = 6f;
  [SerializeField] private int maxSummonAliveFromThisBoss = 6;

  [Header("Phase Visuals")]
  private Renderer[] phaseRenderers;

  [SerializeField] private string colorProperty = "_BaseColor"; // URP Lit uses _BaseColor; Standard uses _Color
  private MaterialPropertyBlock mpb;


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

  // Slowed state (set by Freeze spell)
  private float currentSpeedMultiplier = 1f;

  void Start()
  {
    navAgent = GetComponent<NavMeshAgent>();
    enemyHealth = GetComponent<EnemyHealth>();

    var playerGO = GameObject.FindGameObjectWithTag("Player");
    if (playerGO != null) player = playerGO.transform;

    // initial nav settings
    navAgent.angularSpeed = 360f;
    navAgent.acceleration = 8f;

    // Start in phase 1
    EnterPhase(Phase.Phase1_Fire);
  }

  void Update()
  {
    if (navAgent == null || player == null || enemyHealth == null) return;

    // phase check from HP %
    float hp01 = GetHealth01();
    if (hp01 <= phase3At && currentPhase != Phase.Phase3_MeleeSummon)
      EnterPhase(Phase.Phase3_MeleeSummon);
    else if (hp01 <= phase2At && hp01 > phase3At && currentPhase != Phase.Phase2_Lightning)
      EnterPhase(Phase.Phase2_Lightning);
    else if (hp01 > phase2At && currentPhase != Phase.Phase1_Fire)
      EnterPhase(Phase.Phase1_Fire);

    // detect
    float dist = Vector3.Distance(transform.position, player.position);
    isPlayerDetected = dist <= detectionRange;

    if (!isPlayerDetected)
    {
      navAgent.SetDestination(transform.position);
      StopAllCombatRoutines();
      if (summonRoutine != null) { StopCoroutine(summonRoutine); summonRoutine = null; }
      return;
    }

    // chase
    navAgent.SetDestination(player.position);

    // phase behaviors
    if (currentPhase == Phase.Phase3_MeleeSummon)
    {
      // try dodge occasionally if not slowed too hard
      TryDodgeIfThreatened();

      // melee when close
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

      // stop ranged attacks in phase 3
      if (attackRoutine != null)
      {
        StopCoroutine(attackRoutine);
        attackRoutine = null;
      }
    }
    else
    {
      // ranged phase: attack when within attackRange
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

      // stop melee in ranged phases
      if (meleeRoutine != null)
      {
        StopCoroutine(meleeRoutine);
        meleeRoutine = null;
      }
    }

    // apply movement speed each frame
    navAgent.speed = baseSpeed * currentSpeedMultiplier * GetPhaseSpeedMultiplier();
  }

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
    // clean up old behaviors
    StopAllCombatRoutines();

    // phase-specific nav settings
    if (currentPhase == Phase.Phase3_MeleeSummon)
    {
      navAgent.stoppingDistance = 0.2f;

      if (wizardSpawner != null)
        wizardSpawner.StartSpawning();   // Start spawning little wizards

      if (summonRoutine == null)
        summonRoutine = StartCoroutine(SummonRoutine());
    }
    else
    {
      navAgent.stoppingDistance = Mathf.Min(attackRange * 0.6f, 6f);
    }

    Debug.Log($"[EvoWizard] Entered {currentPhase}");
  }

  void StopAllCombatRoutines()
  {
    if (attackRoutine != null) { StopCoroutine(attackRoutine); attackRoutine = null; }
    if (meleeRoutine != null) { StopCoroutine(meleeRoutine); meleeRoutine = null; }
    // do not stop summon routine unless leaving phase 3
    if (currentPhase != Phase.Phase3_MeleeSummon && summonRoutine != null)
    {
      StopCoroutine(summonRoutine);
      summonRoutine = null;
    }
  }

  IEnumerator RangedAttackRoutine()
  {
    float delay = 1f / Mathf.Max(0.01f, fireRate);

    while (true)
    {
      FacePlayer();

      // choose projectile based on phase
      if (currentPhase == Phase.Phase1_Fire)
      {
        ShootProjectile(fireProjectilePrefab);
      }
      else if (currentPhase == Phase.Phase2_Lightning)
      {
        CastBossLightning();
      }

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

    // Boss fireball
    var bossFireball = projGO.GetComponent<BossFireballProjectile>();
    if (bossFireball != null)
    {
      bossFireball.Init(dir);
      return;
    }

    // Player-style fireball (if you still use it somewhere)
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

    // Lock the target position (where the player was when cast started)
    Vector3 targetPos = player.position;

    // Optional: telegraph here (small VFX or decal)
    // e.g. SpawnTelegraph(targetPos);

    yield return new WaitForSeconds(lightningCastDelay);

    // Strike the locked position
    SpawnLightning(targetPos);

    // Damage only if player is still near that position
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
    var playerHealth = FindFirstObjectByType<PlayerHealth>();
    while (true)
    {
      if (playerHealth != null)
        playerHealth.TakeDamage(phase3MeleeDamage);

      yield return new WaitForSeconds(meleeDamageInterval);
    }
  }

  void TryDodgeIfThreatened()
  {
    // If slowed heavily, don’t dodge much.
    if (currentSpeedMultiplier < 0.6f) return;

    if (Time.time < lastDodgeTime + dodgeCooldown) return;

    // For prototype: probabilistic dodge when the player is aiming at boss.
    // Later you can hook this to projectile detection.
    Transform cam = Camera.main != null ? Camera.main.transform : null;
    if (cam == null) return;

    Vector3 toBoss = (transform.position - cam.position).normalized;
    float aimDot = Vector3.Dot(cam.forward, toBoss);

    // if player is aiming roughly at boss, treat as “threatened”
    if (aimDot > 0.85f && Random.value < dodgeChanceVsProjectiles)
    {
      Vector3 right = Vector3.Cross(Vector3.up, (player.position - transform.position).normalized);
      Vector3 dodgeDir = (Random.value < 0.5f) ? right : -right;

      Vector3 target = transform.position + dodgeDir * dodgeDistance;

      // Use agent move/warp for prototype dodge
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
        // Keep it simple: Try spawn until max reached (spawner already caps maxAlive globally).
        // If you want per-boss tracking, add your own counter.
        wizardSpawner.TrySpawnOne();
      }
      yield return new WaitForSeconds(summonInterval);
    }
  }

  // ======== This is what spells call for phase-based resist/vuln =========
  public void ApplyTypedDamage(float amount, DamageType type)
  {
    float final = amount;

    // Phase 1: resist fire
    if (currentPhase == Phase.Phase1_Fire && type == DamageType.Fire)
      final *= phase1FireMultiplier;

    // Phase 2: resist lightning, vulnerable to fire (i.e. fire does full or more)
    if (currentPhase == Phase.Phase2_Lightning)
    {
      if (type == DamageType.Lightning) final *= phase2LightningMultiplier;
      // vulnerable to fire: you said vulnerable, so do 1.25x for prototype:
      if (type == DamageType.Fire) final *= 1.25f;
    }

    // Phase 3: up to you. Often: no resist, just speed + summons.
    enemyHealth.TakeDamage(final);
  }

  // Freeze hook (same as WizardController)
  public void SetSpeedMultiplier(float multiplier)
  {
    currentSpeedMultiplier = Mathf.Clamp(multiplier, 0.1f, 2f);
  }

  private Coroutine freezeTimer;

  public void ApplyFreezeEffect(float slowAmount, float duration)
  {
    if (freezeTimer != null) StopCoroutine(freezeTimer);
    SetSpeedMultiplier(1f - slowAmount);
    freezeTimer = StartCoroutine(FreezeTimer(duration));
  }

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

  IEnumerator FreezeTimer(float duration)
  {
    yield return new WaitForSeconds(duration);
    SetSpeedMultiplier(1f);
  }
}