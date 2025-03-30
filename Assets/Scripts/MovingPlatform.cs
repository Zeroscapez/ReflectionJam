using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Vertical movement height (amplitude)")]
    public float moveHeight = 2f;
    [Tooltip("Movement speed of the platform")]
    public float moveSpeed = 1f;
    [Tooltip("Determines if the platform starts by moving up")]
    public bool startsMovingUp = true;

    private Vector3 startPosition;
    private float directionMultiplier;
    private List<Rigidbody> playersOnPlatform = new List<Rigidbody>();

    void Start()
    {
        startPosition = transform.position;
        directionMultiplier = startsMovingUp ? 1f : -1f;
    }

    void FixedUpdate()
    {
        // Calculate the new Y position using a sine wave with direction control
        float newY = startPosition.y + moveHeight * Mathf.Sin(Time.time * moveSpeed) * directionMultiplier;
        Vector3 newPosition = new Vector3(startPosition.x, newY, startPosition.z);

        // Determine how far the platform has moved since the last frame
        Vector3 deltaMovement = newPosition - transform.position;

        // Move the platform to the new position
        transform.position = newPosition;

        // Move each player by the same delta so they remain "attached" to the platform
        foreach (Rigidbody playerRb in playersOnPlatform)
        {
            if (playerRb != null)
            {
                playerRb.MovePosition(playerRb.position + deltaMovement);
            }
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
