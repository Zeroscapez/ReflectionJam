using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class TitleScreenManagement : MonoBehaviour
{

    [Header("Screens")]
    public GameObject startScreen;
    public GameObject stageSelectScreen;
    public GameObject controlsScreen;

    [Header("Stage Selection")]
    public GameObject stagePreviewOne;
    public GameObject stagePreviewTwo;
    public Button stageRightCycle;
    public Button stageLeftCycle;
    public Button selectStageButton; // The "Select" button
    private int stagePreview;
    [SerializeField] private string currentLevel;

    [Header("Controls Screen")]
    public GameObject controlsImgOne;
    //public GameObject controlsImgTwo;

    [Header("Navigation Buttons")]
    public Button startButton; // Default selected button on the title screen
    public Button backButtonStageSelect; // "Back" button in stage select
    public Button backButtonControls; // "Back" button in controls menu

    private PlayerControls playerInput;
    private InputAction navigateAction;
    private InputAction submitAction;
    private InputAction cancelAction;


    private void Awake()
    {
        playerInput = new PlayerControls();

        // Get UI Input Actions
        navigateAction = playerInput.UI.Navigate;
        submitAction = playerInput.UI.Submit;
        cancelAction = playerInput.UI.Cancel;

        // Enable Actions
        navigateAction.Enable();
        submitAction.Enable();
        cancelAction.Enable();

        // Add listeners
        submitAction.performed += ctx => SelectCurrentButton();
        cancelAction.performed += ctx => BackToPreviousScreen();
    }
    // Start is called before the first frame update
    void Start()
    {

        stageSelectScreen.SetActive(false);
        controlsScreen.SetActive(false);
        stagePreview = 0;

        startButton.Select();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 navigationInput = navigateAction.ReadValue<Vector2>();

        if (navigationInput.x > 0.5f)
        {
            StageCycleRight();
        }
        else if (navigationInput.x < -0.5f)
        {
            StageCycleLeft();
        }

        switch (stagePreview)
        {
            case 0:
                stagePreviewOne.SetActive(true);
                stagePreviewTwo.SetActive(false);
                stageLeftCycle.interactable = false;
                stageRightCycle.interactable = true;
                currentLevel = "Level 01";

                break;
            case 1:
                stagePreviewOne.SetActive(false);
                stagePreviewTwo.SetActive(true);
                stageLeftCycle.interactable = true;
                stageRightCycle.interactable = false;
                currentLevel = "Level 02";
                break;
        }
    }
    public void StartGameOpen()
    {
        startScreen.SetActive(false);
        stageSelectScreen.SetActive(true);
        stagePreviewOne.SetActive(true);
        stagePreviewTwo.SetActive(false);

        // Reset selection before assigning a new one
        StartCoroutine(ResetButtonSelection(selectStageButton));
    }


    private IEnumerator ResetButtonSelection(Button newButton)
    {
        EventSystem.current.SetSelectedGameObject(null); // Clear selection
        yield return null; // Wait a frame
        EventSystem.current.SetSelectedGameObject(newButton.gameObject); // Select the new button
    }


    public void StartGameClose()
    {
        stageSelectScreen.SetActive(false);
        startScreen.SetActive(true);
        startButton.Select();
    }

    public void StageCycleLeft()
    {
        if (stagePreview != 0)
        {
            stagePreview -= 1;
        }
    }

    public void StageCycleRight()
    {
        if (stagePreview != 1)
        {
            stagePreview += 1;
        }
    }

    public void SelectLevel()
    {

        SceneManager.LoadScene(currentLevel);
    }

    public void ControlsOpen()
    {
        startScreen.SetActive(false);
        controlsScreen.SetActive(true);
        controlsImgOne.SetActive(true);
        //controlsImgTwo.SetActive(false);
        
    }

    public void ControlsClose()
    {
        controlsScreen.SetActive(false);
        startScreen.SetActive(true);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    private void SelectCurrentButton()
    {
        GameObject selectedObject = EventSystem.current?.currentSelectedGameObject;
        if (selectedObject != null && selectedObject.TryGetComponent<Button>(out Button button))
        {
            button.onClick.Invoke();
        }
        else
        {
            Debug.LogWarning("No button is currently selected or the selected object is not a button.");
        }
    }
    private void BackToPreviousScreen()
    {
        if(stageSelectScreen != null)
        {

            if (stageSelectScreen.activeSelf)
            {
                StartGameClose();
            }
            else if (controlsScreen.activeSelf)
            {
                ControlsClose();
            }

        }
        // Return to start screen if in submenus
        
    }


    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        submitAction.performed -= ctx => SelectCurrentButton();
        cancelAction.performed -= ctx => BackToPreviousScreen();
    }

    IEnumerator LoadLevel(string sceneName)
{
    AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
    while (!operation.isDone)
    {
        yield return null; // Wait until scene loads
    }

    // Now safely get PlayerManager references
    PlayerManager playerManager = FindObjectOfType<PlayerManager>();
    if (playerManager == null)
    {
        Debug.LogError("PlayerManager not found after scene load!");
    }
}


}


