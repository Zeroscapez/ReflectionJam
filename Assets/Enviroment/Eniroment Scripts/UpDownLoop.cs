using UnityEngine;
using System.Collections.Generic;

public class UpDownLoop : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;     // Speed of the movement
    public float moveHeight = 3f;    // How far up the platform moves (from its starting position)
    public bool startGoingUp = true; // Toggle: true = starts going up, false = starts going down
    public bool moveEnabled = true;  // Enable or disable movement
    public bool rotateEnabled = false; // Optionally enable rotation (not implemented here)

    private Vector3 startPosition;
    private Rigidbody rb;
    private float offset; // Used to offset PingPong based on startGoingUp

    // Velocity tracking for the platform itself
    public Vector3 CurrentVelocity { get; private set; }
    private Vector3 lastPosition;

    // Anchor management: track anchors for each player on the platform
    private Dictionary<Transform, Transform> playerAnchors = new Dictionary<Transform, Transform>();
    private Dictionary<Transform, Vector3> anchorPrevPositions = new Dictionary<Transform, Vector3>();

    void Start()
    {
        startPosition = transform.position;
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
        }
        offset = startGoingUp ? 0f : moveHeight;
        lastPosition = transform.position;
    }

    void FixedUpdate()
    {
        if (moveEnabled)
        {
            // Calculate new Y using PingPong, so platform moves between startPosition.y and startPosition.y + moveHeight
            float newY = Mathf.PingPong(Time.time * moveSpeed, moveHeight) + startPosition.y - offset;
            Vector3 newPos = new Vector3(startPosition.x, newY, startPosition.z);
            rb.MovePosition(newPos);

            // Update platform's velocity tracking
            CurrentVelocity = (newPos - lastPosition) / Time.fixedDeltaTime;
            lastPosition = newPos;
        }

        // If there are any anchors attached (players on the platform), update their positions.
        foreach (var kvp in playerAnchors)
        {
            Transform playerTransform = kvp.Key;
            Transform anchor = kvp.Value;
            // Calculate how far the anchor has moved since last frame
            Vector3 delta = anchor.position - anchorPrevPositions[playerTransform];
            // Apply that delta to the player's position
            playerTransform.position += delta;
            // Update the anchor's position to match the player's current position
            anchor.position = playerTransform.position;
            // Save the new anchor position for next frame
            anchorPrevPositions[playerTransform] = anchor.position;
        }
    }

    // When a player lands on the platform, create an anchor for that player and parent it to the platform.
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Transform playerTransform = collision.transform;
            // If the player doesn't already have an anchor, create one
            if (!playerAnchors.ContainsKey(playerTransform))
            {
                GameObject anchorObj = new GameObject("PlatformAnchor");
                // Parent the anchor to the platform so it moves with it
                anchorObj.transform.SetParent(transform, true);
                // Set the anchor's position to match the player's current world position
                anchorObj.transform.position = playerTransform.position;
                playerAnchors[playerTransform] = anchorObj.transform;
                anchorPrevPositions[playerTransform] = anchorObj.transform.position;
            }
        }
    }

    // When the player leaves the platform (or jumps off), remove the anchor.
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Transform playerTransform = collision.transform;
            if (playerAnchors.ContainsKey(playerTransform))
            {
                Destroy(playerAnchors[playerTransform].gameObject);
                playerAnchors.Remove(playerTransform);
                anchorPrevPositions.Remove(playerTransform);
            }
        }
    }
}
