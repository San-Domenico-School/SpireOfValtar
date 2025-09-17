public class PlayerMovement : UnityEngine.MonoBehaviour
{
    // --- Tunables (edit in Inspector) ---
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runMultiplier = 1.6f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = GetComponent<Rigidbody>().gravity;

    [SerializeField] private float dodgeDistance = 6f;
    [SerializeField] private float dodgeDuration = 0.18f;   // seconds
    [SerializeField] private float dodgeCooldown = 0.8f;

    [SerializeField] private float attackLungeDistance = 2.5f;
    [SerializeField] private float attackLungeDuration = 0.12f;

    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private UnityEngine.Transform groundCheck;     // put at feet
    [SerializeField] private UnityEngine.LayerMask groundLayers;

    // --- Private state ---
    UnityEngine.CharacterController controller;
    UnityEngine.Vector3 velocity; // y held here
    float lastDodgeTime = -999f;
    bool isDodging = false;
    bool isLunging = false;

    void Awake()
    {
        controller = GetComponent<UnityEngine.CharacterController>();
        if (controller == null)
        {
            UnityEngine.Debug.LogError("CharacterController required on the Player.");
        }
        if (groundCheck == null)
        {
            // fallback to this transform if none provided
            groundCheck = this.transform;
        }
    }

    void Update()
    {
        // Lock movement while dodging/lunging (movement is driven by coroutine)
        if (isDodging || isLunging)
        {
            ApplyGravityAndMove(UnityEngine.Vector3.zero, runHeld: false);
            return;
        }

        // --- Read input (works for keyboard + gamepad sticks with classic Input Manager) ---
        float h = UnityEngine.Input.GetAxisRaw("Horizontal");
        float v = UnityEngine.Input.GetAxisRaw("Vertical");
        UnityEngine.Vector3 input = new UnityEngine.Vector3(h, 0f, v);

        // 8-direction, no diagonal boost
        if (input.sqrMagnitude > 1f) input.Normalize();

        // Map input from local to world based on player facing
        UnityEngine.Vector3 move = this.transform.TransformDirection(input);

        bool runHeld = UnityEngine.Input.GetButton("Fire3"); // default: Left Shift / gamepad X button
        bool jumpPressed = UnityEngine.Input.GetButtonDown("Jump"); // Space / A
        bool dodgePressed = UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.LeftControl) || UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.JoystickButton1); // Ctrl / B (example)
        bool attackPressed = UnityEngine.Input.GetButtonDown("Fire1"); // LMB / RT

        // Jumping
        bool grounded = IsGrounded();
        if (grounded && velocity.y < 0f) velocity.y = -2f; // keep grounded
        if (grounded && jumpPressed)
        {
            // v = sqrt(2gh); gravity is negative
            velocity.y = UnityEngine.Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Dodge (roll)
        if (dodgePressed && Time.time >= lastDodgeTime + dodgeCooldown && move.sqrMagnitude > 0.01f && grounded)
        {
            StartCoroutine(DashRoutine(move.normalized, dodgeDistance, dodgeDuration, isDodge:true));
            lastDodgeTime = Time.time;
        }

        // Attack lunge (spell movement placeholder)
        if (attackPressed && grounded && move.sqrMagnitude > 0.01f)
        {
            StartCoroutine(DashRoutine(move.normalized, attackLungeDistance, attackLungeDuration, isDodge:false));
        }

        // Regular locomotion
        ApplyGravityAndMove(move, runHeld);
    }

    void ApplyGravityAndMove(UnityEngine.Vector3 moveDir, bool runHeld)
    {
        float currentSpeed = walkSpeed * (runHeld ? runMultiplier : 1f);
        UnityEngine.Vector3 planar = moveDir * currentSpeed;

        // Move: planar per-frame, gravity via velocity.y
        controller.Move(planar * UnityEngine.Time.deltaTime);

        velocity.y += gravity * UnityEngine.Time.deltaTime;
        controller.Move(new UnityEngine.Vector3(0f, velocity.y, 0f) * UnityEngine.Time.deltaTime);

        // Face movement direction if any
        UnityEngine.Vector3 face = new UnityEngine.Vector3(planar.x, 0f, planar.z);
        if (face.sqrMagnitude > 0.0001f)
        {
            this.transform.rotation = UnityEngine.Quaternion.Slerp(
                this.transform.rotation,
                UnityEngine.Quaternion.LookRotation(face, UnityEngine.Vector3.up),
                0.2f
            );
        }
    }

    bool IsGrounded()
    {
        // Optional physics check for more reliable grounding than controller.isGrounded
        bool groundHit = UnityEngine.Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayers, UnityEngine.QueryTriggerInteraction.Ignore);
        return groundHit || controller.isGrounded;
    }

    System.Collections.IEnumerator DashRoutine(UnityEngine.Vector3 dir, float distance, float duration, bool isDodgeFlag)
    {
        // Lock states
        if (isDodgeFlag) isDodging = true; else isLunging = true;

        // Optional: add i-frames here (e.g., disable hurtbox collider)
        float elapsed = 0f;
        float speed = distance / UnityEngine.Mathf.Max(0.0001f, duration);

        // Remove vertical velocity during dash/lunge
        float restoreY = velocity.y;
        velocity.y = 0f;

        while (elapsed < duration)
        {
            // Move ignoring gravity; slide along colliders
            controller.Move(dir * speed * UnityEngine.Time.deltaTime);
            elapsed += UnityEngine.Time.deltaTime;
            yield return null;
        }

        velocity.y = restoreY;

        if (isDodgeFlag) isDodging = false; else isLunging = false;
    }

    // --- Gizmos for ground check ---
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            UnityEngine.Gizmos.color = UnityEngine.Color.yellow;
            UnityEngine.Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}

