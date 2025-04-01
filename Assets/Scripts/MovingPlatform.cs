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

    [Header("Rotation Settings")]
    public bool enableRotation = false;
    public float rotationSpeed = 30f;
    public Vector3 rotationAxis = Vector3.up;

    private Vector3 startPosition;
    private float directionMultiplier;
    private List<Rigidbody> playersOnPlatform = new List<Rigidbody>();

    void Start()
    {
        startPosition = transform.position;
        directionMultiplier = startForward ? 1f : -1f;
    }

    void FixedUpdate()
    {
        Vector3 newPosition = transform.position;

        switch (movementType)
        {
            case MovementType.Horizontal:
                newPosition.x = startPosition.x + Mathf.PingPong(Time.time * moveSpeed, moveDistance) * directionMultiplier;
                break;
            case MovementType.Vertical:
                newPosition.y = startPosition.y + Mathf.PingPong(Time.time * moveSpeed, moveDistance) * directionMultiplier;
                break;
        }

        Vector3 deltaMovement = newPosition - transform.position;
        transform.position = newPosition;

        foreach (Rigidbody playerRb in playersOnPlatform)
        {
            if (playerRb != null)
            {
                playerRb.MovePosition(playerRb.position + deltaMovement);
            }
        }

        if (enableRotation)
        {
            transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);
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
