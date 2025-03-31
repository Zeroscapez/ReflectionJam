using Cinemachine.Utility;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
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

    [SerializeField] private float jumpheight = 6f;       // Initial upward velocity when jump is triggered

    [SerializeField] private float coyoteTime = 0.1f;         // Time after leaving the ground when a jump is still allowed
    [SerializeField] private float coyoteTimeCounter;
    [SerializeField] private float jumpBufferTime = 0.1f;     // Time before landing that jump input is buffered
    [SerializeField] private float jumpBufferCounter;
    [SerializeField] private int maxJumps = 2;                // Maximum number of jumps (allows double jump)
    [SerializeField] private int jumpCount = 0;                               // Number of jumps performed since last grounded

    // Timers and state variables for jump buffering & coyote time
    private float lastGroundedTime = Mathf.Infinity; // Time since last on ground
    private float lastJumpPressTime = Mathf.Infinity; // Time since jump was pressed
    private float jumpHoldStartTime;
    private bool isJumping;
    private bool isHoldingJump;
    [SerializeField] private bool doubleJump;
    
  
    public bool jumpInput;

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

    public int extrajump;

    public bool grounded;
    public bool wasGrounded;
    private void Awake()
    {
        manager = FindObjectOfType<PlayerManager>();
        controls = new PlayerControls();
        rb = GetComponent<Rigidbody>();
        playerCamera = manager.pCam;
        playerScale = transform.localScale;
        controls.Player.Interact.performed += ctx => Interact();
        grounded = false;
        
    }

    private void Update()
    {
        bool wasPreviouslyGrounded = grounded; // Store the previous grounded state before checking again
        IsGrounded(); // Update grounded state

        // Jump buffering: Store the jump input for a short time
        if (jumpInput)
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else if (jumpBufferCounter > 0)
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        // Coyote time logic (allow jumping slightly after leaving ground)
        if (grounded)
        {
            coyoteTimeCounter = coyoteTime;

            // Reset jump count only when first landing
            if (!wasPreviouslyGrounded) // Only triggers when transitioning from air to ground
            {
                jumpCount = 0;
                doubleJump = false;
            }
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        transform.localScale = playerScale; // Keep player's scale constant
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


        HandleMovement();
        ApplyFastFall();
    }


    private void HandleMovement()
    {
        Vector2 moveInput = move.ReadValue<Vector2>();
        float controlMultiplier = grounded ? 1f : airControlMultiplier; // Use air control if not grounded
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
        }
        else if (platformVelocity.y >= 0)
        {
            rb.velocity += platformVelocity;
        }

        


            Jump();


        //if (!grounded && jumpCount == 0)
        //{
        //    Jump();
        //    jumpCount = 3;
        //}


        jumpHoldStartTime = Time.time;
        isHoldingJump = true;
    }

    public void Jump()
    {
        if (grounded)
        {
            Debug.Log("Boing");
            doubleJump = false; // Reset double jump
            jumpCount = 0; // Reset jump count on ground
        }

        if ((coyoteTimeCounter > 0f || jumpBufferCounter > 0f) || (doubleJump && jumpCount < maxJumps))
        {
            Debug.Log("Jump Activated");

            // If falling fast, give an extra boost to the jump
            float jumpForce = rb.velocity.y < -2f && !grounded ? jumpheight * 3f : jumpheight * 1.5f;
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            coyoteTimeCounter = 0f;
            jumpBufferCounter = 0f;

            // If the player fell off a ledge, only allow a single jump.
            if (!grounded && jumpCount == 0)
            {
                doubleJump = false; // No double jump if they walked off
            }
            else
            {
                doubleJump = !doubleJump;
            }

            jumpCount++;
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




    private void OnJumpCanceled(InputAction.CallbackContext context)
    {
        jumpInput = false;
        isHoldingJump = false;
        // End the jump hold.
        isJumping = false;
        jumpBufferCounter -= Time.deltaTime;
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


    void IsGrounded()
    {
        float groundCheckDistance = (GetComponent<CapsuleCollider>().height / 2) + 0.1f;
        grounded = Physics.Raycast(transform.position, -transform.up, groundCheckDistance, groundMask);
    }


}
