using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public GameObject firstSelectedButton;
    public InputActionReference cancelAction;
    public CharacterController3D playerController; // Reference to player movement
    public PlayerManager playerManager; // Reference to PlayerManager

    private bool isPaused = false;

    void OnEnable()
    {
       
        cancelAction.action.performed += TogglePause;
    }

    void OnDisable()
    {
        cancelAction.action.performed -= TogglePause;
    }

    private void Start()
    {
        playerManager = GetComponentInChildren<PlayerManager>();
        pauseMenuUI.SetActive(false);
    }

    void TogglePause(InputAction.CallbackContext context)
    {

       if(playerManager != null)
        {
            playerController = playerManager.activeCharacter.GetComponent<CharacterController3D>();
            if (isPaused)
                Resume();
            else
                Pause();
        }

    }
        

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;

        // Re-enable player controls
        playerController.controls.Player.Enable();
        playerManager.controls.Player.Enable();
        playerController.controls.UI.Disable();
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;

        // Disable Player controls and enable UI controls
        playerController.controls.Player.Disable();
        playerManager.controls.Player.Disable();
        playerController.controls.UI.Enable();

        // Set first UI button selected for gamepad navigation
        EventSystem.current.SetSelectedGameObject(firstSelectedButton);
    }

    public void Restart()
    {
        Application.Quit();
    }


    public void QuitToMainMenu()
    {
        Application.Quit();
    }

}
