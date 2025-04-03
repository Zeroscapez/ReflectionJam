using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq;

public class SceneInitializer : MonoBehaviour
{
    public static SceneInitializer Instance; // Singleton for easy access

    public PlayerManager playerManager;
    private CharacterController3D[] characters;

    public bool isInitialized = false;

    private void Awake()
    {
        // Singleton pattern (optional, but useful for easy reference)
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        StartCoroutine(InitializeScene());
    }

    private IEnumerator InitializeScene()
    {
        yield return new WaitForEndOfFrame(); // Wait to ensure all objects are loaded

        // 1️⃣ Find or create PlayerManager
        playerManager = FindObjectOfType<PlayerManager>();
        if (playerManager == null)
        {
            Debug.LogError("PlayerManager not found in scene!");
            yield break;
        }

        // 2️⃣ Find all CharacterController3D instances
        characters = FindObjectsOfType<CharacterController3D>();

        if (characters.Length == 0)
        {
            Debug.LogError("No CharacterController3D instances found!");
            yield break;
        }

        // 3️⃣ Assign characters to PlayerManager
        playerManager.SetupCharacters(characters);

        isInitialized = true;
        Debug.Log("✅ Scene Initialization Complete!");
    }
}
