using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    // --- Tunables (edit in Inspector) ---
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runMultiplier = 1.6f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -20f; // negative accel (m/s^2)

    [SerializeField] private float dodgeDistance = 6f;
    [SerializeField] private float dodgeDuration = 0.18f;   // seconds
    [SerializeField] private float dodgeCooldown = 0.8f;

    [SerializeField] private float attackLungeDistance = 2.5f;
    [SerializeField] private float attackLungeDuration = 0.12f;

    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private Transform groundCheck;     // put at feet
    [SerializeField] private LayerMask groundLayers;

    // --- Private state ---
    CharacterController controller;
    Vector3 velocity; // y held here
    float lastDodgeTime = -999f;
    bool isDodging = false;
    bool isLunging = false;

    // Input System state (set by PlayerInput callbacks)
    Vector2 moveInput;
    bool sprintHeldInput;
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
    }

    void Update()
    {
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
        Vector3 input = new Vector3(moveInput.x, 0f, moveInput.y);
        if (input.sqrMagnitude > 1f) input.Normalize();

        // Map local to world based on facing
        Vector3 move = transform.TransformDirection(input);

        bool runHeld       = sprintHeldInput;
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

        // Regular locomotion
        ApplyGravityAndMove(move, runHeld);
    }

    void ApplyGravityAndMove(Vector3 moveDir, bool runHeld)
    {
        float currentSpeed = walkSpeed * (runHeld ? runMultiplier : 1f);
        Vector3 planar = moveDir * currentSpeed;

        controller.Move(planar * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(new Vector3(0f, velocity.y, 0f) * Time.deltaTime);

        // Face move direction
        Vector3 face = new Vector3(planar.x, 0f, planar.z);
        if (face.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(face, Vector3.up),
                0.2f
            );
        }

	// These are invoked by a PlayerInput component set to "Send Messages"
	void OnMove(InputAction.CallbackContext context)
	{
		moveInput = context.ReadValue<Vector2>();
	}

	void OnSprint(InputAction.CallbackContext context)
	{
		if (context.performed) sprintHeldInput = true;
		if (context.canceled) sprintHeldInput = false;
	}

	void OnJump(InputAction.CallbackContext context)
	{
		if (context.performed) jumpPressedFrame = true;
	}

	void OnAttack(InputAction.CallbackContext context)
	{
		if (context.performed) attackPressedFrame = true;
	}

	void OnCrouch(InputAction.CallbackContext context)
	{
		// Repurpose crouch as dodge trigger per current design
		if (context.performed) crouchPressedFrame = true;
	}
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
