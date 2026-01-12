using UnityEngine;
using UnityEngine.InputSystem;

/************************************
 * Handles melee attacks when F key is pressed.
 * Checks for enemies within the player's attack range and deals damage.
 * Detects all objects with the "Enemy" tag and damages them if they have EnemyHealth component.
 * Gleb
 * Version 1.0
 ************************************/
[RequireComponent(typeof(CharacterController))]
public class MeleeAttack : MonoBehaviour
{
    [Header("Melee Attack Settings")]
    [SerializeField] private float meleeDamage = 25f;
    [SerializeField] private float attackRange = 2f; // Range for melee attack detection
    [SerializeField] private float cooldown = 2f; // Cooldown between melee attacks (seconds)
    
    [Header("Animation")]
    [SerializeField] private Animator attackAnimator; // Animator for attack animations (to be assigned later)
    
    // Components
    private CharacterController characterController;
    
    // Input state
    private bool meleeAttackPressed = false;
    private float lastAttackTime = -999f; // Track when last attack happened
    
    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            Debug.LogError("CharacterController required on the Player for MeleeAttack.");
        }
        
        // Try to get animator if not assigned
        if (attackAnimator == null)
        {
            attackAnimator = GetComponent<Animator>();
        }
    }
    
    /// <summary>
    /// Checks if melee attack is available (cooldown has passed).
    /// </summary>
    public bool canAttack => Time.time >= lastAttackTime + cooldown && Time.timeScale > 0f;
    
    private void Update()
    {
        // Don't process input when game is paused
        if (Time.timeScale == 0f) return;
        
        // Check for melee attack input (from PlayerInput callback)
        if (meleeAttackPressed)
        {
            meleeAttackPressed = false; // Consume the input
            
            if (canAttack)
            {
                PerformMeleeAttack();
                lastAttackTime = Time.time; // Update last attack time
            }
        }
        
        // Fallback: Check F key directly using new Input System (if PlayerInput isn't wired up)
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            if (canAttack)
            {
                PerformMeleeAttack();
                lastAttackTime = Time.time; // Update last attack time
            }
        }
    }
    
    /// <summary>
    /// Performs a melee attack, checking for enemies within range and dealing damage.
    /// Uses the same logic as FireballProjectile for enemy detection.
    /// </summary>
    private void PerformMeleeAttack()
    {
        // Use a sphere check from the player's position (same pattern as Fireball AOE)
        Vector3 attackCenter = transform.position + characterController.center;
        
        // AOE Damage - damage all enemies within radius (copied from FireballProjectile)
        Collider[] hitColliders = Physics.OverlapSphere(attackCenter, attackRange);
        
        foreach (Collider nearby in hitColliders)
        {
            // Skip if it's the player itself
            if (nearby.gameObject == gameObject || nearby.transform == transform)
            {
                continue;
            }
            
            // Check if collider or its parent has the "Enemy" tag (same as Fireball logic)
            if (nearby.CompareTag("Enemy"))
            {
                EnemyHealth enemyHealth = nearby.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(meleeDamage);
                }
            }
            else
            {
                // Check parent objects for the Enemy tag (since tag is on parent)
                Transform parent = nearby.transform.parent;
                while (parent != null)
                {
                    if (parent.CompareTag("Enemy"))
                    {
                        EnemyHealth enemyHealth = parent.GetComponent<EnemyHealth>();
                        if (enemyHealth != null)
                        {
                            enemyHealth.TakeDamage(meleeDamage);
                        }
                        break; // Found the enemy, no need to check further up
                    }
                    parent = parent.parent;
                }
            }
        }
        
        // Trigger attack animation if animator is available
        if (attackAnimator != null)
        {
            // TODO: Set trigger for melee attack animation when animation is ready
            // attackAnimator.SetTrigger("MeleeAttack");
        }
    }
    
    /// <summary>
    /// Input callback for melee attack (invoked by PlayerInput component).
    /// </summary>
    public void OnMeleeAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            meleeAttackPressed = true;
        }
    }
    
    /// <summary>
    /// Draws gizmos in the editor to visualize the attack range.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }
        
        if (characterController != null)
        {
            Vector3 center = transform.position + characterController.center;
            
            // Draw the detection sphere in red
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(center, attackRange);
        }
    }
}
