using UnityEngine;

public class Treadmill : MonoBehaviour
{
    public float treadmillSpeed = 3f; // Speed of the treadmill
    public float resistanceFactor = 0.5f; // How much it slows down player movement
    public Vector3 treadmillDirection = Vector3.forward; // Set direction manually in Inspector

    private void OnTriggerStay(Collider other)
    {
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null)
        {
            CharacterController3D playerMovement = other.GetComponent<CharacterController3D>(); // Reference to your player movement script

            if (playerMovement != null)
            {
                // Normalize and scale treadmill direction
                Vector3 movementDirection = treadmillDirection.normalized * treadmillSpeed;
                Vector3 playerVelocity = rb.velocity;

                if (playerVelocity.magnitude > 0.1f) // Player is moving
                {
                    // Reduce effectiveness of movement in treadmill's direction
                    Vector3 resistance = movementDirection * resistanceFactor;
                    rb.velocity = playerVelocity - resistance;
                }
                else
                {
                    // Move the player exactly in the specified direction
                    rb.velocity = movementDirection;
                }
            }
        }
    }
}
