using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        CharacterController3D player = other.GetComponent<CharacterController3D>();

        if (player != null)
        {
            player.ResetToStartPosition();
        }
    }
}
