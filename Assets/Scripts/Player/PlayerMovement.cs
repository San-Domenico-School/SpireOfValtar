using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    // --- Tunables (edit in Inspector) ---
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runMultiplier = 2f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -20f; // negative accel (m/s^2)

    // Sprint / Stamina
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float sprintStaminaCostPerSecond = 25f;
    [SerializeField] private float staminaRecoveryPerSecond = 15f;
    [SerializeField] private float staminaRecoveryDelay = 0.5f;

    [SerializeField] private float dodgeDistance = 6f;
    [SerializeField] private float dodgeDuration = 0.18f;   // seconds
    [SerializeField] private float dodgeCooldown = 0.8f;

    [SerializeField] private float attackLungeDistance = 2.5f;
    [SerializeField] private float attackLungeDuration = 0.12f;
    [SerializeField] private float turnSensitivity = 10f;
    [SerializeField] private Transform cameraPivot; // assign the Camera or a pivot under the player
    [SerializeField] private float maxPitchAngle = 60f; // clamp pitch to ±max
    [SerializeField] private float ceilingRestitution = 0.2f; // 0=no bounce, 1=elastic
    [SerializeField] private float minCeilingBounceSpeed = 3f; // minimum downward speed after head hit

    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private Transform groundCheck;     // put at feet
    [SerializeField] private LayerMask groundLayers;

    // --- Private state ---
    CharacterController controller;
    Vector3 velocity; // y held here
    float lastDodgeTime = -999f;
    bool isDodging = false;
    bool isLunging = false;

    float currentStamina;
    float lastSprintTime = -999f;
    float cameraPitchDegrees = 0f;

    // Input System state (set by PlayerInput callbacks)
    Vector2 moveInput;
    Vector2 turnInput;
    bool jumpPressedFrame;
    bool attackPressedFrame;
    bool crouchPressedFrame;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (controller == null)
            Debug.LogError("CharacterController required on the Player.");

        if (groundCheck == null)
            groundCheck = this.transform; // fallback
        gravity = Physics.gravity.y;

        currentStamina = maxStamina;
        // Cursor state managed by UI managers
    }

    void Update()
    {
        // Don't process input when game is paused (Time.timeScale == 0)
        if (Time.timeScale == 0f) return;
        
        ApplyTurnFromInput();
        // Lock movement while dodging/lunging (movement is driven by coroutine)
        if (isDodging || isLunging)
        {
            ApplyGravityAndMove(Vector3.zero, runHeld: false);
            return;
        }

        // --- Input System helpers & callbacks ---
        static bool Consume(ref bool flag)
        {
            bool was = flag;
            flag = false;
            return was;
        }

        // --- Read input (Unity Input System) ---
        Vector3 inputLocal = new Vector3(moveInput.x, 0f, moveInput.y);
        if (inputLocal.sqrMagnitude > 1f) inputLocal.Normalize();

        // Convert to world space relative to player's facing, flattened on XZ to ignore camera pitch
        Vector3 move;
        {
            Vector3 fwd = transform.forward; fwd.y = 0f; fwd.Normalize();
            Vector3 right = transform.right; right.y = 0f; right.Normalize();
            move = (fwd * inputLocal.z) + (right * inputLocal.x);
            if (move.sqrMagnitude > 1f) move.Normalize();
        }

        bool jumpPressed   = Consume(ref jumpPressedFrame);
        bool dodgePressed  = Consume(ref crouchPressedFrame);
        bool attackPressed = Consume(ref attackPressedFrame);

        // Grounding & jump
        bool grounded = IsGrounded();
        if (grounded && velocity.y < 0f) velocity.y = -2f; // keep stuck to ground
        if (grounded && jumpPressed)
        {
            // v = sqrt(2 g h); gravity is negative
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Dodge (roll)
        if (dodgePressed && Time.time >= lastDodgeTime + dodgeCooldown && move.sqrMagnitude > 0.01f && grounded)
        {
            StartCoroutine(DashRoutine(move.normalized, dodgeDistance, dodgeDuration, isDodge: true));
            lastDodgeTime = Time.time;
        }

        // Attack lunge
        if (attackPressed && grounded && move.sqrMagnitude > 0.01f)
        {
            StartCoroutine(DashRoutine(move.normalized, attackLungeDistance, attackLungeDuration, isDodge: false));
        }

        // Stamina / sprint gating
        bool isMovingPlanar = move.sqrMagnitude > 0.0001f;
        bool runHeld = false;
        if (isMovingPlanar && grounded && currentStamina > 0.01f)
        {
            runHeld = true;
            currentStamina -= sprintStaminaCostPerSecond * Time.deltaTime;
            if (currentStamina < 0f) currentStamina = 0f;
            lastSprintTime = Time.time;
        }
        else
        {
            runHeld = false;
            if (Time.time >= lastSprintTime + staminaRecoveryDelay)
            {
                currentStamina += staminaRecoveryPerSecond * Time.deltaTime;
                if (currentStamina > maxStamina) currentStamina = maxStamina;
            }
        }

        // Regular locomotion
        ApplyGravityAndMove(move, runHeld);
    }

    void ApplyTurnFromInput()
    {
        // Don't process camera turn when game is paused
        if (Time.timeScale == 0f) return;
        if (turnInput.sqrMagnitude <= 0.000001f) return;
        float yawDegrees = turnInput.x * turnSensitivity;
        transform.Rotate(0f, yawDegrees, 0f, Space.Self);

        if (cameraPivot != null)
        {
            cameraPitchDegrees -= turnInput.y * turnSensitivity; // invert Y for natural look
            cameraPitchDegrees = Mathf.Clamp(cameraPitchDegrees, -maxPitchAngle, maxPitchAngle);
            cameraPivot.localRotation = Quaternion.Euler(cameraPitchDegrees, 0f, 0f);
        }
        turnInput = Vector2.zero;
    }
    
    void ApplyGravityAndMove(Vector3 moveDir, bool runHeld)
    {
        float currentSpeed = walkSpeed * (runHeld ? runMultiplier : 1f);
        Vector3 planar = new Vector3(moveDir.x, 0f, moveDir.z) * currentSpeed;

        CollisionFlags flags = controller.Move(planar * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        flags |= controller.Move(new Vector3(0f, velocity.y, 0f) * Time.deltaTime);

        // If we hit the ceiling while moving upward, apply a downward bounce
        if ((flags & CollisionFlags.Above) != 0 && velocity.y > 0f)
        {
            float bounceSpeed = Mathf.Max(minCeilingBounceSpeed, velocity.y * ceilingRestitution);
            velocity.y = -bounceSpeed;
        }


    }
    
	// These are invoked by a PlayerInput component of unity event
	public void OnMove(InputAction.CallbackContext context)

	{
		moveInput = context.ReadValue<Vector2>();
	}

	public void OnTurn(InputAction.CallbackContext context)
	{
		turnInput = context.ReadValue<Vector2>();
        //Debug.Log("Turn input: " + turnInput);     //use this to check if its getting the mouse input
	}

	public void OnJump(InputAction.CallbackContext context)
	{
		if (context.performed) jumpPressedFrame = true;
	}

	public void OnAttack(InputAction.CallbackContext context)
	{
		if (context.performed) attackPressedFrame = true;
	}

	public void OnCrouch(InputAction.CallbackContext context)
	{
		// Repurpose crouch as dodge trigger per current design
		if (context.performed) crouchPressedFrame = true;
	}
    

    bool IsGrounded()
    {
        bool groundHit = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayers, QueryTriggerInteraction.Ignore);
        return groundHit || controller.isGrounded;
    }

    IEnumerator DashRoutine(Vector3 dir, float distance, float duration, bool isDodge)
    {
        if (isDodge) isDodging = true; else isLunging = true;

        float elapsed = 0f;
        float speed = distance / Mathf.Max(0.0001f, duration);

        // suspend vertical motion during dash/lunge
        float restoreY = velocity.y;
        velocity.y = 0f;

        while (elapsed < duration)
        {
            controller.Move(dir * speed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        velocity.y = restoreY;

        if (isDodge) isDodging = false; else isLunging = false;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
