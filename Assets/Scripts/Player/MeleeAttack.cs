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

    [Header("Input")]
    [SerializeField] private InputActionReference meleeAction;
    
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

        // Ensure saved keybinds are applied to the PlayerInput actions at runtime
        var playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            KeybindUtils.ApplySavedKeybinds(playerInput.actions);
        }
    }

    private void OnEnable()
    {
        InputAction action = ResolveMeleeAction();
        if (action != null)
        {
            action.performed += OnMeleeActionPerformed;
            action.Enable();
        }
    }

    private void OnDisable()
    {
        InputAction action = ResolveMeleeAction();
        if (action != null)
        {
            action.performed -= OnMeleeActionPerformed;
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
        
        // Intentionally rely on Input System bindings so rebinding works via settings.
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
                // Try to get EnemyHealth directly, or via EnemyHitbox proxy, or from parent
                EnemyHealth enemyHealth = nearby.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(meleeDamage);
                }
                else
                {
                    // Check for EnemyHitbox proxy (for child hitboxes)
                    EnemyHitbox hitbox = nearby.GetComponent<EnemyHitbox>();
                    if (hitbox != null)
                    {
                        hitbox.TakeDamage(meleeDamage);
                    }
                    else
                    {
                        // Fallback: check parent for EnemyHealth
                        enemyHealth = nearby.GetComponentInParent<EnemyHealth>();
                        if (enemyHealth != null)
                        {
                            enemyHealth.TakeDamage(meleeDamage);
                        }
                    }
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

    private void OnMeleeActionPerformed(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            meleeAttackPressed = true;
        }
    }

    private InputAction ResolveMeleeAction()
    {
        if (meleeAction != null)
        {
            return meleeAction.action;
        }

        var playerInput = GetComponent<PlayerInput>();
        if (playerInput != null && playerInput.actions != null)
        {
            return playerInput.actions.FindAction("MeleeAttack");
        }

        return null;
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
