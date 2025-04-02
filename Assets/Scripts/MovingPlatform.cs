using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public enum MovementType { None, Horizontal, Vertical, Rotational }

    [Header("Movement Settings")]
    public MovementType movementType = MovementType.None;
    public float moveDistance = 2f;
    public float moveSpeed = 1f;
    public bool startForward = true;
    public bool activated = false; // New activation flag

    [Header("Rotation Settings")]
    public bool enableRotation = false;
    public float rotationSpeed = 30f;
    public Vector3 rotationAxis = Vector3.up;

    private Vector3 startPosition;
    private float directionMultiplier;
    private float progress = 0f; // This stores our movement progress
    private List<Rigidbody> playersOnPlatform = new List<Rigidbody>();

    void Start()
    {
        startPosition = transform.position;
        directionMultiplier = startForward ? 1f : -1f;
    }

    void FixedUpdate()
    {
        // Only update movement if activated. Otherwise, progress remains stored.
        if (!activated) return;

        // Increment our progress only when activated.
        progress += Time.fixedDeltaTime * moveSpeed;

        Vector3 newPosition = transform.position;

        // Use the stored progress variable in the PingPong calculation.
        switch (movementType)
        {
            case MovementType.Horizontal:
                newPosition.x = startPosition.x + Mathf.PingPong(progress, moveDistance) * directionMultiplier;
                break;
            case MovementType.Vertical:
                newPosition.y = startPosition.y + Mathf.PingPong(progress, moveDistance) * directionMultiplier;
                break;
        }

        // Calculate the movement delta to also move any players on the platform.
        Vector3 deltaMovement = newPosition - transform.position;
        transform.position = newPosition;

        foreach (Rigidbody playerRb in playersOnPlatform)
        {
            if (playerRb != null)
            {
                playerRb.MovePosition(playerRb.position + deltaMovement);
            }
        }

        // Optionally update rotation if enabled.
        if (enableRotation)
        {
            transform.Rotate(rotationAxis * rotationSpeed * Time.fixedDeltaTime);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody rb = collision.collider.attachedRigidbody;
            if (rb != null && !playersOnPlatform.Contains(rb))
            {
                playersOnPlatform.Add(rb);
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody rb = collision.collider.attachedRigidbody;
            if (rb != null)
            {
                playersOnPlatform.Remove(rb);
            }
        }
    }
}
