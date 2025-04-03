using UnityEngine;

public class FinishLineManager : MonoBehaviour
{
    public bool playerAAtFinish = false;
    public bool playerBAtFinish = false;

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