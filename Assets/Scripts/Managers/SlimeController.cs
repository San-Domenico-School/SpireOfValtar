using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class SlimeController : MonoBehaviour
{
    [Header("Enemy Settings")]
    [SerializeField] private float baseSpeed = 6f;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Speed Modifiers")]
    [SerializeField] private float currentSpeedMultiplier = 0.2f;

    [Header("Animation")]
    [Tooltip("Animator on the slime model (often on a child object). If left empty, will auto-find in children.")]
    [SerializeField] private Animator animator;
    [Tooltip("Velocity magnitude above this counts as moving.")]
    [SerializeField] private float moveThreshold = 0.1f;
    [Tooltip("Animator float parameter name used to blend/switch states.")]
    [SerializeField] private string speedParam = "Speed";

    // Components
    private NavMeshAgent navAgent;
    private Transform player;

    // State
    private bool isPlayerDetected = false;
    private bool isAttacking = false;

    // Freeze effect management
    private Coroutine freezeTimer = null;

    // Damage player
    private PlayerHealth playerHealth;
    public int damage = 15;
    [SerializeField] private float damageInterval = 3f;
    private Coroutine damageCoroutine;
    private bool playerInDamageRange;

    [Header("Audio (optional)")]
    [SerializeField] private EnemySoundController enemySound;

    void Start()
    {
        playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth == null)
        {
            Debug.LogWarning("PlayerHealth not found in scene!");
        }

        // Get NavMeshAgent component
        navAgent = GetComponent<NavMeshAgent>();
        if (navAgent == null)
        {
            Debug.LogError("SlimeController requires a NavMeshAgent component!");
            return;
        }

        if (enemySound == null) enemySound = GetComponent<EnemySoundController>();
        if (enemySound == null) enemySound = GetComponentInParent<EnemySoundController>();
        if (enemySound == null) enemySound = GetComponentInChildren<EnemySoundController>();

        // Find Animator if not assigned (usually on the model child)
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
            if (animator == null)
            {
                Debug.LogWarning("No Animator found on Slime or its children. Animations won't play.");
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
            // Update destination to player position
            navAgent.SetDestination(player.position);

            // Check if in attack range
            if (distanceToPlayer <= attackRange && !isAttacking)
            {
                StartAttack();
            }
            else if (distanceToPlayer > attackRange && isAttacking)
            {
                StopAttack();
            }
        }
        else
        {
            // Player not detected, stop moving
            navAgent.SetDestination(transform.position);
            if (isAttacking)
            {
                StopAttack();
            }
        }

        // Update NavMeshAgent speed
        navAgent.speed = baseSpeed * currentSpeedMultiplier;

        // ---- Animation drive ----
        UpdateAnimation();

        HandleDamageRange(distanceToPlayer);
    }

    private void UpdateAnimation()
    {
        if (animator == null || navAgent == null) return;

        // NavMeshAgent velocity is the most reliable for agent-driven movement
        float speed = navAgent.velocity.magnitude;

        // Kill tiny jitters near 0 to avoid flickering states
        if (speed < moveThreshold) speed = 0f;

        animator.SetFloat(speedParam, speed);
    }

    private void StartAttack()
    {
        isAttacking = true;
        // TODO: Add attack logic here (you can also trigger attack animations)
        // Example: animator?.SetTrigger("Attack");
        if (enemySound != null) enemySound.PlayAttackSfx();
    }

    private void StopAttack()
    {
        isAttacking = false;
        // TODO: Add stop attack logic here
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

    // Enemy collision with player
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && other.gameObject != null)
        {
            Debug.Log("Enemy collided");

            if (playerHealth != null)
            {
                if (enemySound != null) enemySound.PlayAttackSfx();
                playerHealth.TakeDamage(damage); // initial instant damage
                Debug.Log($"{gameObject.name} has hit Player!");

                // Start periodic damage if not already running
                if (damageCoroutine == null)
                {
                    damageCoroutine = StartCoroutine(DamageOverTime());
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
            damageCoroutine = null;
        }
    }

    private IEnumerator DamageOverTime()
    {
        yield return new WaitForSeconds(damageInterval);

        while (true)
        {
            if (playerHealth != null)
            {
                if (enemySound != null) enemySound.PlayAttackSfx();
                playerHealth.TakeDamage(damage);
                Debug.Log($"{gameObject.name} dealt periodic damage: {damage}");
            }
            yield return new WaitForSeconds(damageInterval);
        }
    }

    private void HandleDamageRange(float distanceToPlayer)
    {
        if (player == null || playerHealth == null) return;

        bool inRange = distanceToPlayer <= attackRange;

        if (inRange != playerInDamageRange)
        {
            playerInDamageRange = inRange;

            if (inRange)
            {
                if (enemySound != null) enemySound.PlayAttackSfx();
                playerHealth.TakeDamage(damage);
                if (damageCoroutine == null)
                {
                    damageCoroutine = StartCoroutine(DamageOverTime());
                }
            }
            else if (damageCoroutine != null)
            {
                StopCoroutine(damageCoroutine);
                damageCoroutine = null;
            }
        }
        else if (inRange && damageCoroutine == null)
        {
            damageCoroutine = StartCoroutine(DamageOverTime());
        }
    }
}
