using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        CharacterController3D player = other.GetComponent<CharacterController3D>();

        if (player != null)
        {
            // Update player's respawn position to this checkpoint
            player.SetCheckpoint(transform.position);
        }
    }
}
