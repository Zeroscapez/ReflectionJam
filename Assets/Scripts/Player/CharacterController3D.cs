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

    // Timers and state variables for jump buffering & coyote time
    private float lastGroundedTime = Mathf.Infinity; // Time since last on ground
    private float lastJumpPressTime = Mathf.Infinity; // Time since jump was pressed
    private float jumpHoldStartTime;
    private bool isJumping;
    private bool isHoldingJump;
    private bool jumpInput;

    [Header("Ground Check")]
    public float groundDistance = 0.2f;

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
    }

    private void HandleMovement()
    {
        Vector2 moveInput = move.ReadValue<Vector2>();
        forceDirection += moveInput.x * GetCameraRight(playerCamera) * movementForce;
        forceDirection += moveInput.y * GetCameraForward(playerCamera) * movementForce;

        rb.AddForce(forceDirection, ForceMode.Impulse);
        forceDirection = Vector3.zero;

        // Clamp horizontal velocity
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

    // Check for buffered jump input and coyote time.
    private void HandleBufferedJump()
    {
        // Only initiate a jump if not already jumping, the jump was pressed recently,
        // and the character was grounded within the allowed coyote time.
        if (!isJumping && lastJumpPressTime < jumpBufferTime && lastGroundedTime < coyoteTime)
        {
            float maxJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(Physics.gravity.y) * maxJumpHeight);
            // Initiate the jump with the minimum impulse.
            rb.velocity = new Vector3(rb.velocity.x, minJumpImpulse, rb.velocity.z);
            isJumping = true;
            jumpHoldStartTime = Time.time;

            // Consume the buffered jump and coyote time.
            lastJumpPressTime = jumpBufferTime;
            lastGroundedTime = coyoteTime;
        }
    }

    // While the jump button is held and within max hold time, add extra upward velocity.
    private void HandleJumpHold()
    {
        if (isJumping && isHoldingJump)
        {
            float maxJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(Physics.gravity.y) * maxJumpHeight);
            float holdDuration = Time.time - jumpHoldStartTime;
            if (holdDuration < maxJumpHoldTime)
            {
                // Calculate extra acceleration such that, if held fully, the jump velocity reaches maxJumpVelocity.
                float extraAcceleration = (maxJumpVelocity - minJumpImpulse) / maxJumpHoldTime;
                rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y + extraAcceleration * Time.fixedDeltaTime, rb.velocity.z);
            }
        }
    }

    // Process jump input only if the character is on the ground (or within the coyote time window).
    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        // Only accept jump input if grounded or just left ground within coyote time.
        if (!IsGrounded() && lastGroundedTime >= coyoteTime)
        {
            // If we've been in the air too long, ignore the jump input.
            return;
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
        // End the jump hold immediately.
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

    private bool IsGrounded()
    {
        // Cast a ray downward from the character's position.
        return Physics.Raycast(transform.position, Vector3.down, groundDistance);
    }
}
