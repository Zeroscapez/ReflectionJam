using UnityEngine;

public class FinishLineManager : MonoBehaviour
{
    public bool playerAAtFinish = false;
    public bool playerBAtFinish = false;
    public GameObject playerA;
    public GameObject playerB;

    public void Update()
    {
        if (playerA == null || playerB == null)
        {
            playerA = GetComponentInChildren<PlayerManager>().characterA;
            playerB = GetComponentInChildren<PlayerManager>().characterB;

        }
    }
    public void CheckGameEnd()
    {
        if (playerAAtFinish && playerBAtFinish)
        {
            Debug.Log("Both players reached their finish lines! Quitting game...");
            QuitGame();
        }
    }

    private void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // For testing in Unity Editor
#endif
    }
}