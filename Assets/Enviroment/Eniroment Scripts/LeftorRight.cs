using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformMover : MonoBehaviour
{
    public float moveSpeed = 5f;  // Speed at which the platform moves
    public float moveDistance = 10f;  // The distance the platform moves left and right
    private Vector3 startPosition;

    private void Start()
    {
        // Store the initial position of the platform
        startPosition = transform.position;
    }

    private void Update()
    {
        // Calculate the new position for the platform based on sine wave oscillation
        float newPosX = startPosition.x + Mathf.Sin(Time.time * moveSpeed) * moveDistance;

        // Apply the calculated position to the platform while maintaining the same Y and Z values
        transform.position = new Vector3(newPosX, transform.position.y, transform.position.z);
    }
}
