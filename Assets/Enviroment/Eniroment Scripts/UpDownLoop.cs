using UnityEngine;

public class UpDownLoop : MonoBehaviour
{
    public float moveSpeed = 2f;  // Speed of the movement
    public float moveHeight = 3f; // Maximum height the object will move
    public bool startGoingUp = true; // Toggle: true = starts going up, false = starts going down

    private Vector3 startPosition;
    private Rigidbody rb;
    private float offset; // Controls PingPong direction

    // Velocity tracking
    public Vector3 CurrentVelocity { get; private set; }
    private Vector3 lastPosition;

    void Start()
    {
        startPosition = transform.position;
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true; // Only moves with code, not physics forces
        }

        // If the platform should start going down, offset the time so it begins at the peak
        offset = startGoingUp ? 0f : moveHeight;

        // Initialize lastPosition for velocity calculation
        lastPosition = transform.position;
    }

    void FixedUpdate() // Use FixedUpdate for physics
    {
        // Calculate the new Y position using PingPong
        float newY = Mathf.PingPong(Time.time * moveSpeed, moveHeight) + startPosition.y - offset;
        Vector3 newPos = new Vector3(startPosition.x, newY, startPosition.z);

        // Move the platform using Rigidbody.MovePosition
        rb.MovePosition(newPos);

        // Update velocity tracking based on the change in position
        CurrentVelocity = (newPos - lastPosition) / Time.fixedDeltaTime;
        lastPosition = newPos;
    }

    // When a player collides with the platform, parent them so they move together.
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(transform);
        }
    }

    // When the player leaves the platform, unparent them.
    private void OnCollisionExit(Collision collision)
    {
       
    }
}
