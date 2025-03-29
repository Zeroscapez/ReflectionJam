using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpDownLoop : MonoBehaviour
{
    public float moveSpeed = 2f;  // Speed of the movement
    public float moveHeight = 3f; // Maximum height the object will move

    private Vector3 startPosition;
    private float targetY;

    void Start()
    {
        // Store the initial position of the object
        startPosition = transform.position;
        targetY = startPosition.y + moveHeight;  // Set the target height
    }

    void Update()
    {
        // Move the object up and down smoothly
        float step = moveSpeed * Time.deltaTime;  // Movement speed based on frame time
        transform.position = new Vector3(startPosition.x, Mathf.PingPong(Time.time * moveSpeed, moveHeight) + startPosition.y, startPosition.z);
    }
}
