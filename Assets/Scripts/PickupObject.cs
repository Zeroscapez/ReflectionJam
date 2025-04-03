using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PickupObject : MonoBehaviour, IInteractable
{
    private Transform objectHolder;
    private Rigidbody rb;
    private PlayerControls controls;
    private InputAction rotate;
    public bool lifting;

    [SerializeField] private float rotationSpeed = 100f; // Speed of rotation
    [SerializeField] private Collider objectCollider; // Assign in Inspector
    private Collider currentPlayerCollider; // Player holding the object
    private Collider lastPlayerCollider; // Player who last held the object
    [SerializeField] private Animator animator; // Animator for the player
    public Vector3 cubeOrigin;

    private void Start()
    {
        cubeOrigin = transform.position;
        rb = GetComponent<Rigidbody>();
        controls = new PlayerControls();
        rotate = controls.Player.Rotate; // Get rotation input
        controls.Enable();

        if (objectCollider == null)
        {
            objectCollider = GetComponent<Collider>();
        }
    }

    public void OnInteract()
    {
        Transform newHolder = GetActiveCharacterObjectHolder();

        if (newHolder == null)
        {
            Debug.LogError("No active character or object holder found!");
            return;
        }

        // Get the animator from the player interacting with the object
        animator = newHolder.GetComponentInParent<CharacterController3D>().GetComponentInChildren<Animator>();

        // If another player picks it up, pass it instead of dropping
        if (lifting && newHolder != objectHolder)
        {
            PassToNewPlayer(newHolder);
        }
        else if (lifting)
        {
            Drop(); // Drop if the same player presses interact
        }
        else
        {
            PickUp(newHolder);
        }
    }

    private Transform GetActiveCharacterObjectHolder()
    {
        PlayerManager manager = FindObjectOfType<PlayerManager>();
        if (manager != null && manager.activeCharacter != null)
        {
            CharacterController3D characterController = manager.activeCharacter.GetComponent<CharacterController3D>();
            if (characterController != null)
            {
                return characterController.transform.Find("ObjectHolder");
            }
        }
        return null;
    }

    private void PickUp(Transform newHolder)
    {
        Debug.Log("Picked Up");

        // Restore collision for the last player before setting a new one
        if (lastPlayerCollider != null && objectCollider != null)
        {
            Physics.IgnoreCollision(lastPlayerCollider, objectCollider, false);
        }

        rb.isKinematic = true;
        transform.SetParent(newHolder);
        transform.localPosition = Vector3.zero;
        lifting = true;
        objectHolder = newHolder;

        // Get the new player's collider
        currentPlayerCollider = newHolder.GetComponentInParent<Collider>();

        // Ignore collision with the current player
        if (currentPlayerCollider != null && objectCollider != null)
        {
            Physics.IgnoreCollision(currentPlayerCollider, objectCollider, true);
        }

        // Update last player
        lastPlayerCollider = currentPlayerCollider;

        // Set the animation state
        if (animator != null)
        {
            animator.SetBool("Is_Lifting", true);
        }
    }

    private void PassToNewPlayer(Transform newHolder)
    {
        Debug.Log("Passing Object to Another Player");

        // Restore collision for the current player before passing
        if (currentPlayerCollider != null && objectCollider != null)
        {
            Physics.IgnoreCollision(currentPlayerCollider, objectCollider, false);
        }

        // Stop the current player's lifting animation
        if (currentPlayerCollider != null)
        {
            Animator currentPlayerAnimator = currentPlayerCollider.GetComponentInChildren<Animator>();
            if (currentPlayerAnimator != null)
            {
                currentPlayerAnimator.SetBool("Is_Lifting", false);
            }
        }

        // Switch to the new holder
        transform.SetParent(newHolder);
        transform.localPosition = Vector3.zero;
        objectHolder = newHolder;

        // Update player colliders
        lastPlayerCollider = currentPlayerCollider;  // Previous holder is now lastPlayerCollider
        currentPlayerCollider = newHolder.GetComponentInParent<Collider>();

        // Ignore collision with the new player
        if (currentPlayerCollider != null && objectCollider != null)
        {
            Physics.IgnoreCollision(currentPlayerCollider, objectCollider, true);
        }

        // Update animator for the new player
        animator = newHolder.GetComponentInParent<CharacterController3D>().GetComponentInChildren<Animator>();
        if (animator != null)
        {
            animator.SetBool("Is_Lifting", true);
        }
    }

    private void Drop()
    {
        Debug.Log("Dropped");
        transform.SetParent(null);
        rb.isKinematic = false;
        lifting = false;

        // Re-enable collision with the player who dropped it
        if (currentPlayerCollider != null && objectCollider != null)
        {
            Physics.IgnoreCollision(currentPlayerCollider, objectCollider, false);
        }

        // Stop the current player's lifting animation
        if (currentPlayerCollider != null)
        {
            Animator currentAnimator = currentPlayerCollider.GetComponentInChildren<Animator>();
            if (currentAnimator != null)
            {
                currentAnimator.SetBool("Is_Lifting", false);
            }
        }

        // Reset player references
        lastPlayerCollider = currentPlayerCollider;
        currentPlayerCollider = null;
        objectHolder = null;

        // Reset the animator reference
        animator = null;
    }



    private void Update()
    {
        if (lifting) // Rotate only if held
        {
            Rotate();
        }
    }

    private void Rotate()
    {
        float rotateInput = rotate.ReadValue<float>(); // Get rotation input (-1 for Q, 1 for E)

        if (Mathf.Abs(rotateInput) > 0.1f) // Rotate only when input is detected
        {
            transform.Rotate(Vector3.up, rotateInput * rotationSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Death"))
        {
            transform.position = cubeOrigin;
        }
    }
}
