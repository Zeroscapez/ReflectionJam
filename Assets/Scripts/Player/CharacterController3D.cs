using Cinemachine.Utility;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

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
    public Vector3 rayOrigin;

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
    private Animator animator;

    private Vector3 respawnPosition;

    private void Awake()
    {
        
        controls = new PlayerControls();
        rb = GetComponent<Rigidbody>();
        
        playerScale = transform.localScale;
        controls.Player.Interact.performed += ctx => Interact();
        animator = GetComponentInChildren<Animator>();
        grounded = false;
        
    }

    private void Start()
    {
        if (SceneInitializer.Instance == null || !SceneInitializer.Instance.isInitialized)
        {
            Debug.LogError("SceneInitializer is not ready!");
            return;
        }

        manager = SceneInitializer.Instance.playerManager;
        playerCamera = manager.pCam;
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

        animator.SetBool("on_floor", grounded);
    }


    public void SetCheckpoint(Vector3 newCheckpoint)
    {
        respawnPosition = newCheckpoint;
    }

    public void ResetToStartPosition()
    {
        transform.position = respawnPosition;
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
        if(manager == null)
        {
            manager = SceneInitializer.Instance.playerManager;
        }

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

        if (playerCamera == null){

            playerCamera = FindObjectOfType<PlayerManager>().GetComponentInChildren<Camera>();

        }
        else
        {
            forceDirection += moveInput.x * GetCameraRight(playerCamera) * movementForce * controlMultiplier;
            forceDirection += moveInput.y * GetCameraForward(playerCamera) * movementForce * controlMultiplier;
        }
           

        rb.AddForce(forceDirection, ForceMode.Impulse);
        forceDirection = Vector3.zero;

        // Clamp horizontal velocity as before.
        Vector3 horizontalVelocity = rb.velocity;
        horizontalVelocity.y = 0;
        if (horizontalVelocity.sqrMagnitude > maxSpeed * maxSpeed)
        {
            rb.velocity = horizontalVelocity.normalized * maxSpeed + Vector3.up * rb.velocity.y;
        }
        if (grounded)
        {
            animator.SetFloat("Run", moveInput.magnitude);
        }
        else
        {
            animator.SetFloat("Run", -0.1f);
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
            doubleJump = false; // Reset double jump when touching ground
            jumpCount = 0; // Reset jump count on ground
        }

        // Allow jumping if:
        // - Within coyote time
        // - Jump buffering active
        // - Still has double jump available
        if ((coyoteTimeCounter > 0f || jumpBufferCounter > 0f) || (doubleJump && (jumpCount < maxJumps)))
        {
            Debug.Log("Jump Activated");

            // Extra boost if falling fast
            float jumpForce = jumpheight * 1.5f;
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z); // Reset vertical velocity
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            // Handle animations
            if (jumpCount == 0)
            {
                animator.SetTrigger("Jump"); // First jump animation
                jumpCount++;
            }
            else
            {
                animator.ResetTrigger("Jump"); // Ensure proper animation transition

                if (IsLifting()) // Special animation if lifting
                {
                    animator.Play("TVHeadLiftJump.001", 0, 0f);
                    
                }
                else
                {
                    animator.Play("TVHeadJump", 0, 0f); // Regular double jump animation
                }

                jumpCount++;
            }

            // Reset coyote time and jump buffer after jumping
            coyoteTimeCounter = 0f;
            jumpBufferCounter = 0f;

            // If the player fell off a ledge but hasn't jumped yet, allow one extra jump
            if (!grounded && jumpCount == 0 || jumpCount == 1)
            {
                doubleJump = true; // Enable double jump only if the player hasn't used it
               
              

            }

        }
        else
        {
            Debug.Log("Jump not activated: conditions not met");
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

        // Define where we check for objects (around the player's chest)
        Vector3 checkPosition = transform.position + transform.forward * 1.5f + Vector3.up * 0.5f;
        float detectionRadius = 1f; // Adjust for better range

        // Check for nearby colliders
        Collider[] colliders = Physics.OverlapSphere(checkPosition, detectionRadius);

        foreach (Collider collider in colliders)
        {
            IInteractable interactable = collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                Debug.Log("Interacting with: " + collider.name);
                interactable.OnInteract();
                return; // Stop after first valid interaction
            }
        }

        Debug.Log("No interactable object found.");
    }




    void IsGrounded()
    {
        float groundCheckDistance = (GetComponent<CapsuleCollider>().height / 2) + 0.1f;
        grounded = Physics.Raycast(transform.position, -transform.up, groundCheckDistance, groundMask);
    }

    private bool IsLifting()
    {
        PickupObject pickup = GetComponentInChildren<PickupObject>();
        return pickup != null && pickup.lifting;
    }

}
