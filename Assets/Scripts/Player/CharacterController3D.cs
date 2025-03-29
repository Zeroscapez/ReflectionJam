using Cinemachine.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterController3D : MonoBehaviour
{
    [Header("Movement Settings")]
    public string Name = string.Empty;
    public int ID = 000;

    private Rigidbody rb;

    [SerializeField]
    public float maxSpeed = 5f;
    [SerializeField]
    private float movementForce = 1f;

    [Header("Jump Settings")]
    [SerializeField] private float maxJumpHeight = 10f;     // Desired maximum jump height in units
    [SerializeField] private float minJumpImpulse = 5f;       // Initial upward velocity when jump is triggered
    [SerializeField] private float maxJumpHoldTime = 0.3f;    // How long extra upward force can be applied
    [SerializeField] private float coyoteTime = 0.1f;         // Time after leaving the ground when a jump is still allowed
    [SerializeField] private float jumpBufferTime = 0.1f;     // Time before landing that jump input is buffered
    [SerializeField] private int maxJumps = 2;                // Maximum number of jumps (allows double jump)
    private int jumpCount = 0;                               // Number of jumps performed since last grounded

    // Timers and state variables for jump buffering & coyote time
    private float lastGroundedTime = Mathf.Infinity; // Time since last on ground
    private float lastJumpPressTime = Mathf.Infinity; // Time since jump was pressed
    private float jumpHoldStartTime;
    private bool isJumping;
    private bool isHoldingJump;
    private bool jumpInput;

    [Header("Fast Fall Settings")]

    [SerializeField] private float maxFallSpeed = 20f;          // Maximum downward speed
    [SerializeField] private float fastFallAcceleration = 10f; // Additional downward acceleration (gradual fast fall)
   
    [SerializeField] private float airControlMultiplier = 0.5f;  // Multiplier for horizontal movement when in the air

    [Header("Ground Check")]
    public float groundDistance = 0.2f;
    [SerializeField] private Transform groundCheck;           // Assign a child object at the character's feet
    [SerializeField] private float groundRadius = 0.3f;         // Radius for sphere check
    [SerializeField] private LayerMask groundMask;              // Layer mask for ground

    private Vector3 forceDirection = Vector3.zero;

    public PlayerControls controls;
    public PlayerManager manager;

    [SerializeField]
    private Camera playerCamera;
    private InputAction move;

    private void Awake()
    {
        manager = FindObjectOfType<PlayerManager>();
        controls = new PlayerControls();
        rb = GetComponent<Rigidbody>();
        playerCamera = manager.pCam;

        controls.Player.Interact.performed += ctx => Interact();
    }

    private void Update()
    {
        // Update grounded timer: if on ground, reset; otherwise, accumulate.
        if (IsGrounded())
        {
            lastGroundedTime = 0;
            jumpCount = 0;  // Reset jump count when grounded.
        }
        else
        {
            lastGroundedTime += Time.deltaTime;
        }

        // Only accumulate jump press time when jump input is not active.
        if (!jumpInput)
        {
            lastJumpPressTime += Time.deltaTime;
        }
    }


    void OnEnable()
    {
        controls.Player.Jump.performed += OnJumpPerformed;
        controls.Player.Jump.canceled += OnJumpCanceled;
        move = controls.Player.Move;
        controls.Player.Enable();
    }
    void OnDisable()
    {
        controls.Player.Jump.performed -= OnJumpPerformed;
        controls.Player.Jump.canceled -= OnJumpCanceled;
        controls.Player.Disable();
    }

    private void FixedUpdate()
    {
        HandleBufferedJump();
        HandleJumpHold();
        HandleMovement();
        ApplyFastFall();
    }

    private void HandleMovement()
    {
        Vector2 moveInput = move.ReadValue<Vector2>();
        float controlMultiplier = IsGrounded() ? 1f : airControlMultiplier; // Use air control if not grounded
        forceDirection += moveInput.x * GetCameraRight(playerCamera) * movementForce * controlMultiplier;
        forceDirection += moveInput.y * GetCameraForward(playerCamera) * movementForce * controlMultiplier;

        rb.AddForce(forceDirection, ForceMode.Impulse);
        forceDirection = Vector3.zero;

        // Clamp horizontal velocity as before.
        Vector3 horizontalVelocity = rb.velocity;
        horizontalVelocity.y = 0;
        if (horizontalVelocity.sqrMagnitude > maxSpeed * maxSpeed)
        {
            rb.velocity = horizontalVelocity.normalized * maxSpeed + Vector3.up * rb.velocity.y;
        }
        LookAt();
    }


    private void LookAt()
    {
        Vector3 direction = rb.velocity;
        direction.y = 0f;
        if (move.ReadValue<Vector2>().sqrMagnitude > 0.1f && direction.sqrMagnitude > 0.1f)
        {
            rb.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }
        else
        {
            rb.angularVelocity = Vector3.zero;
        }
    }

    private Vector3 GetCameraForward(Camera cam)
    {
        Vector3 forward = cam.transform.forward;
        forward.y = 0;
        return forward.normalized;
    }

    private Vector3 GetCameraRight(Camera cam)
    {
        Vector3 right = cam.transform.right;
        right.y = 0;
        return right.normalized;
    }

    // Check for buffered jump input, coyote time, and allow double jump.
    private void HandleBufferedJump()
    {
        // Allow a jump if the jump was pressed recently and either:
        // the character is grounded (or within coyote time) or a double jump is available.
        if (lastJumpPressTime < jumpBufferTime && (IsGrounded() || lastGroundedTime < coyoteTime || jumpCount < maxJumps))
        {
            float maxJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(Physics.gravity.y) * maxJumpHeight);
            // Initiate jump: reset vertical velocity to ensure a consistent jump.
            rb.velocity = new Vector3(rb.velocity.x, minJumpImpulse, rb.velocity.z);
            isJumping = true;
            jumpHoldStartTime = Time.time;
            jumpCount++;  // Count this jump.
                          // Consume the buffered jump and coyote time.
            lastJumpPressTime = jumpBufferTime;
            lastGroundedTime = coyoteTime;
        }
    }


    // While the jump button is held and within the max hold time, add extra upward velocity.
    private void HandleJumpHold()
    {
        if (isJumping && isHoldingJump)
        {
            float maxJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(Physics.gravity.y) * maxJumpHeight);
            float holdDuration = Time.time - jumpHoldStartTime;
            if (holdDuration < maxJumpHoldTime)
            {
                float extraAcceleration = (maxJumpVelocity - minJumpImpulse) / maxJumpHoldTime;
                rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y + extraAcceleration * Time.fixedDeltaTime, rb.velocity.z);
                rb.velocity = new Vector3(rb.velocity.x, Mathf.Min(rb.velocity.y, maxJumpVelocity), rb.velocity.z);
            }
        }
    }

    // Increase falling speed when falling until a maximum fall speed is reached.
    private void ApplyFastFall()
    {
        if (rb.velocity.y < 0)
        {
            // Gradually add extra downward acceleration.
            float newVelocityY = rb.velocity.y - fastFallAcceleration * Time.fixedDeltaTime;
            // Clamp the downward velocity to not exceed -maxFallSpeed.
            newVelocityY = Mathf.Max(newVelocityY, -maxFallSpeed);
            rb.velocity = new Vector3(rb.velocity.x, newVelocityY, rb.velocity.z);
        }
    }


    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        // Allow jump if the player hasn't used all available jumps.
        if (jumpCount >= maxJumps)
        {
            return; // Prevent further jumps if already used up.
        }
        jumpInput = true;
        lastJumpPressTime = 0;
        jumpHoldStartTime = Time.time;
        isHoldingJump = true;
    }


    private void OnJumpCanceled(InputAction.CallbackContext context)
    {
        jumpInput = false;
        isHoldingJump = false;
        // End the jump hold.
        isJumping = false;
    }

    void Interact()
    {
        Debug.Log(Name + " " + ID.ToString() + " Interact Button Pressed");
        Ray ray = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 3f))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.OnInteract();
            }
        }
    }

    // Robust ground check using a sphere.
    private bool IsGrounded()
    {
        return Physics.CheckSphere(groundCheck.position, groundRadius, groundMask);
    }
}
