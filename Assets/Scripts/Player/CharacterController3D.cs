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
    [SerializeField] private int jumpCount = 0;                               // Number of jumps performed since last grounded

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
    private Vector3 playerScale;
    private void Awake()
    {
        manager = FindObjectOfType<PlayerManager>();
        controls = new PlayerControls();
        rb = GetComponent<Rigidbody>();
        playerCamera = manager.pCam;
        playerScale = transform.localScale;
        controls.Player.Interact.performed += ctx => Interact();
    }

    private void Update()
    {
        if (IsGrounded())
        {
            lastGroundedTime = 0;
            jumpCount = 0; // Reset jump count when grounded
        }
        else
        {
            lastGroundedTime += Time.deltaTime;
        }

        if (!jumpInput)
        {
            lastJumpPressTime += Time.deltaTime;
        }
        transform.localScale = playerScale;
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
        // If the player is attached to a moving platform, override their vertical velocity.
        if (transform.parent != null && transform.parent.CompareTag("MovingPlatform"))
        {
            PlatformVelocity platformVel = transform.parent.GetComponent<PlatformVelocity>();
            if (platformVel != null)
            {
                // Override only the Y component to match the platform’s Y velocity.
                rb.velocity = new Vector3(rb.velocity.x, platformVel.CurrentVelocity.y, rb.velocity.z);
            }
        }

        // Then continue with your normal movement, jump, and fast-fall handling.
       
        
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
        // Only process jump if the jump input was registered recently.
        if (jumpInput && lastJumpPressTime < jumpBufferTime)
        {
            // Condition 1: Jump from ground (or within coyote time)
            if (IsGrounded() || lastGroundedTime < coyoteTime)
            {
                PerformJump();
                // Reset timers so the buffered jump is consumed.
                lastJumpPressTime = jumpBufferTime;
                lastGroundedTime = coyoteTime;
            }
            // Condition 2: Allow double jump if we're already in the air
            else if (jumpCount < maxJumps - 1) // subtract one if the initial jump is counted separately
            {
                PerformJump();
                jumpCount++; // Consume one jump
            }
        }
    }


    private void PerformJump()
    {

        if (transform.parent != null && transform.parent.CompareTag("MovingPlatform"))
        {
            Rigidbody platformRb = transform.parent.GetComponent<Rigidbody>();
            if (platformRb != null)
            {
                rb.velocity += platformRb.velocity;
            }
        }
        // Add jump impulse on top of the current vertical velocity.
        rb.AddForce(Vector3.up * minJumpImpulse, ForceMode.Impulse);
        isJumping = true;
        jumpHoldStartTime = Time.time;
        // Optionally, you can also reset jumpInput here to ensure the buffered jump is consumed.
        jumpInput = false;

        

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
        Vector3 platformVelocity = Vector3.zero;

        if (transform.parent != null && transform.parent.CompareTag("MovingPlatform"))
        {
            PlatformVelocity platformVel = transform.parent.GetComponent<PlatformVelocity>();
            if (platformVel != null)
            {
                platformVelocity = platformVel.CurrentVelocity;
            }
            transform.parent = null; // Unparent the player from the platform
        }

        // Subtract the platform's velocity from the player's velocity

        if (platformVelocity.y <= -1)
        {
            rb.velocity -= platformVelocity;
        } else if (platformVelocity.y >= 0)
        {
            rb.velocity += platformVelocity;
        }


            // Apply the jump force
            rb.AddForce(Vector3.up * minJumpImpulse, ForceMode.Impulse);

        isJumping = true;
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
        return Physics.Raycast(transform.position, Vector3.down, groundDistance + 0.1f, groundMask);
    }

}
