using UnityEngine;

public class FinishLineTrigger : MonoBehaviour
{
    public GameObject targetPlayer; // Assign the corresponding player in the Inspector
    public FinishLineManager manager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == targetPlayer)
        {
            if (targetPlayer == manager.GetComponent<FinishLineManager>().playerA)
                manager.playerAAtFinish = true;
            else
                manager.playerBAtFinish = true;

            manager.CheckGameEnd();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == targetPlayer)
        {
            if (targetPlayer == manager.GetComponent<FinishLineManager>().playerA)
                manager.playerAAtFinish = false;
            else
                manager.playerBAtFinish = false;
        }
    }
}