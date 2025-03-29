using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPad : MonoBehaviour
{
    public float jumpForce = 10f; // The force applied when the object collides with the jump pad

    void OnCollisionEnter(Collision collision)
    {
        // Check if the object has a Rigidbody (so it's a movable object)
        Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();



        if (rb != null)
        {
            // Apply an upward force to the object's Rigidbody
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
}
