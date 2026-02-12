using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class SkeletonMeleeController : MonoBehaviour
{
    [Header("Enemy Settings")]
    [SerializeField] private float baseSpeed = 6f;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float attackRange = 1.05f;
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Speed Modifiers")]
    [SerializeField] private float currentSpeedMultiplier = 0.2f;

    [Header("Animation")]
    [Tooltip("Animator on the skeleton model (often on a child object). If left empty, will auto-find in children.")]
    [SerializeField] private Animator animator;
    [Tooltip("Velocity magnitude above this counts as moving.")]
    [SerializeField] private float moveThreshold = 0.1f;
    [Tooltip("Animator float parameter name used to blend/switch states.")]
    [SerializeField] private string speedParam = "Speed";

    [Header("Melee Hitbox")]
    [Tooltip("Weapon hitbox script (usually on the sword). If empty, will auto-find in children.")]
    [SerializeField] private SkeletonMeleeHitbox hitbox;

    [Header("Melee Damage")]
    [SerializeField] private int damage = 15;
    [Tooltip("Cooldown between hits while the sword overlaps the player.")]
    [SerializeField] private float damageInterval = 0.5f;
    [SerializeField] private string playerTag = "Player";

    [Header("Audio (optional)")]
    [SerializeField] private EnemySoundController enemySound;

    // Components
    private NavMeshAgent navAgent;
    private Transform player;

    // State
    private bool isPlayerDetected = false;
    private bool isAttacking = false;

    // Freeze effect management
    private Coroutine freezeTimer = null;

    private readonly System.Collections.Generic.Dictionary<Collider, float> nextDamageTime =
        new System.Collections.Generic.Dictionary<Collider, float>();

    void Start()
    {
        // Get NavMeshAgent component
        navAgent = GetComponent<NavMeshAgent>();
        if (navAgent == null)
        {
            Debug.LogError("SkeletonMeleeController requires a NavMeshAgent component!");
            return;
        }

        if (enemySound == null) enemySound = GetComponent<EnemySoundController>();
        if (enemySound == null) enemySound = GetComponentInParent<EnemySoundController>();
        if (enemySound == null) enemySound = GetComponentInChildren<EnemySoundController>();

        if (hitbox == null) hitbox = GetComponentInChildren<SkeletonMeleeHitbox>(true);

        // Find Animator if not assigned (usually on the model child)
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
            if (animator == null)
            {
                Debug.LogWarning("No Animator found on Skeleton or its children. Animations won't play.");
            }
        }

        // Find player by tag
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogWarning("No object with 'Player' tag found!");
        }

        // Initialize NavMeshAgent settings
        navAgent.speed = baseSpeed * currentSpeedMultiplier;
        navAgent.angularSpeed = 360f;
        navAgent.acceleration = 8f;
        navAgent.stoppingDistance = attackRange;

        // Set initial destination to current position
        navAgent.SetDestination(transform.position);
    }

    void Update()
    {
        if (navAgent == null || player == null) return;

        // Check if player is within detection range
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        isPlayerDetected = distanceToPlayer <= detectionRange;

        if (isPlayerDetected)
        {
            if (distanceToPlayer <= attackRange)
            {
                if (!isAttacking) StartAttack();
                navAgent.isStopped = true;
                FacePlayer();
            }
            else
            {
                if (isAttacking) StopAttack();
                navAgent.isStopped = false;
                navAgent.SetDestination(player.position);
            }
        }
        else
        {
            navAgent.isStopped = true;
            navAgent.ResetPath();
            if (isAttacking) StopAttack();
        }

        // Update NavMeshAgent speed
        navAgent.speed = baseSpeed * currentSpeedMultiplier;

        // ---- Animation drive ----
        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        if (animator == null || navAgent == null) return;

        // NavMeshAgent velocity is the most reliable for agent-driven movement
        float speed = navAgent.velocity.magnitude;

        // The current animator uses Speed to switch between Walk and Downward_slash.
        // Keep slash only when actually attacking; otherwise force Walk even if standing still.
        if (isAttacking)
        {
            speed = 0f; // triggers Downward_slash state
        }
        else if (!isPlayerDetected)
        {
            // Force Walk (idle proxy) so we don't play the slash when idle.
            speed = Mathf.Max(speed, 0.11f);
        }
        else
        {
            // Kill tiny jitters near 0 to avoid flickering states
            if (speed < moveThreshold) speed = 0f;
        }

        animator.SetFloat(speedParam, speed);
    }

    private void FacePlayer()
    {
        if (player == null) return;

        Vector3 dir = player.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
    }

    private void StartAttack()
    {
        isAttacking = true;
    }

    private void StopAttack()
    {
        isAttacking = false;
        // Intentionally do not disable hitbox here.
    }

    // ===== Animation Events =====
    // Add these events to the sword swing animation clip (Downward_slash)
    // at the start/end of the contact window.
    public void AnimAttackStart()
    {
        if (enemySound != null) enemySound.PlayAttackSfx();
        if (hitbox != null) hitbox.BeginAttack();
    }

    public void AnimAttackEnd()
    {
        // Intentionally left blank so the hitbox stays active until we leave attack range.
    }

    // Public method for external speed modification (for spells, etc.)
    public void SetSpeedMultiplier(float multiplier)
    {
        float oldMultiplier = currentSpeedMultiplier;
        currentSpeedMultiplier = Mathf.Clamp(multiplier, 0.1f, 2f);

        // Debug speed changes
        if (oldMultiplier != currentSpeedMultiplier)
        {
            float newSpeed = baseSpeed * currentSpeedMultiplier;
            if (currentSpeedMultiplier < 1f)
            {
                Debug.Log($"❄️ {gameObject.name} SLOWED! Speed: {newSpeed:F2} (Multiplier: {currentSpeedMultiplier:F2})");
            }
            else if (currentSpeedMultiplier == 1f && oldMultiplier < 1f)
            {
                Debug.Log($"🔥 {gameObject.name} speed RESTORED! Speed: {newSpeed:F2} (Multiplier: {currentSpeedMultiplier:F2})");
            }
        }
    }

    // Method to apply freeze effect with timer management
    public void ApplyFreezeEffect(float slowAmount, float duration)
    {
        // Stop existing freeze timer if one is running
        if (freezeTimer != null)
        {
            StopCoroutine(freezeTimer);
            Debug.Log("🔄 Freeze timer RESTARTED!");
        }
        else
        {
            Debug.Log("❄️ New freeze effect applied!");
        }

        // Apply slow effect
        SetSpeedMultiplier(1f - slowAmount);

        // Start new freeze timer
        freezeTimer = StartCoroutine(FreezeTimer(duration));
    }

    private IEnumerator FreezeTimer(float duration)
    {
        yield return new WaitForSeconds(duration);

        // Restore speed to normal
        SetSpeedMultiplier(1f);
        freezeTimer = null;
    }

    private void OnDisable()
    {
        if (hitbox != null) hitbox.EndAttack();
    }

    public void TryDamage(Collider other)
    {
        if (other == null || !other.CompareTag(playerTag)) return;

        if (damageInterval > 0f)
        {
            if (nextDamageTime.TryGetValue(other, out float nextTime) && Time.time < nextTime) return;
            nextDamageTime[other] = Time.time + damageInterval;
        }

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth == null) playerHealth = other.GetComponentInParent<PlayerHealth>();
        if (playerHealth != null) playerHealth.TakeDamage(damage);
    }

    public void ClearDamageTarget(Collider other)
    {
        if (other == null)
        {
            nextDamageTime.Clear();
            return;
        }
        nextDamageTime.Remove(other);
    }
}
