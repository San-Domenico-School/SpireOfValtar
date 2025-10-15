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
    [SerializeField] private float turnSensitivity = 1f;

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
    Vector2 turnInput;
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

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
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

        // Convert to world space relative to player's facing
        Vector3 move = transform.TransformDirection(inputLocal);

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

    void ApplyTurnFromInput()
    {
        if (turnInput.sqrMagnitude <= 0.000001f) return;
        float yawDegrees = turnInput.x * turnSensitivity;
        float pitchDegrees = turnInput.y * turnSensitivity;
        transform.Rotate(0f, yawDegrees, 0f, Space.World);
        transform.Rotate(-pitchDegrees, 0f, 0f, Space.Self);
        turnInput = Vector2.zero;
    }
    
    void ApplyGravityAndMove(Vector3 moveDir, bool runHeld)
    {
        float currentSpeed = walkSpeed * (runHeld ? runMultiplier : 1f);
        Vector3 planar = moveDir * currentSpeed;

        controller.Move(planar * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(new Vector3(0f, velocity.y, 0f) * Time.deltaTime);


    }
    
	// These are invoked by a PlayerInput component of unity event
	public void OnMove(InputAction.CallbackContext context)

	{
		moveInput = context.ReadValue<Vector2>();
	}

	public void OnTurn(InputAction.CallbackContext context)
	{
		turnInput = context.ReadValue<Vector2>();
        //Debug.Log("Turn input: " + turnInput);
	}

	public void OnSprint(InputAction.CallbackContext context)
	{
		if (context.performed) sprintHeldInput = true;
		if (context.canceled) sprintHeldInput = false;
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
