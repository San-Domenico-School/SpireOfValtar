using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    [Header("Enemy Settings")]
    [SerializeField] private float baseSpeed = 6f; // Slightly faster than player walk speed (5f) - makes freeze spell valuable
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Speed Modifiers")]
    [SerializeField] private float currentSpeedMultiplier = 0.2f;


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
    public int damage = 15; // Balanced for souls-like: More punishing damage
    [SerializeField] private float damageInterval = 3f; // damage every 3 seconds while in contact
    private Coroutine damageCoroutine;

    void Start()
    {
        playerHealth = FindFirstObjectByType<PlayerHealth>();

        // Get NavMeshAgent component
        navAgent = GetComponent<NavMeshAgent>();
        if (navAgent == null)
        {
            Debug.LogError("EnemyController requires a NavMeshAgent component!");
            return;
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
    }

    private void StartAttack()
    {
        isAttacking = true;
        // TODO: Add attack logic here
    }

    private void StopAttack()
    {
        isAttacking = false;
        // TODO: Add stop attack logic here hp=0?
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
        freezeTimer = null; // Clear the timer reference
    }

    // Enemy collision with player
    private void OnTriggerEnter(Collider other)
    { 
        if (other.CompareTag("Player") && other.gameObject != null)
        {
            Debug.Log("Enemy collided");
            playerHealth.TakeDamage(damage); // initial instant damage
            Debug.Log($"{gameObject.name} has hit Player!");

            // Start periodic damage if not already running
            if (damageCoroutine == null)
            {
                damageCoroutine = StartCoroutine(DamageOverTime());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && damageCoroutine != null)
        {
            // Stop periodic damage once contact is broken
            StopCoroutine(damageCoroutine);
            damageCoroutine = null;
        }
    }

    private IEnumerator DamageOverTime()
    {
        // Wait 5 seconds of continuous contact before first periodic damage
        yield return new WaitForSeconds(damageInterval);

        // Then deal damage every 5 seconds while still in contact
        while (true)
        {
            playerHealth.TakeDamage(damage);
            Debug.Log($"{gameObject.name} dealt periodic damage: {damage}");
            yield return new WaitForSeconds(damageInterval);
        }
    }
}