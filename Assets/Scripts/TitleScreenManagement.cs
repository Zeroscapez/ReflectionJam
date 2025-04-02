using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class TitleScreenManagement : MonoBehaviour
{

    public GameObject startScreen;
    public GameObject stageSelectScreen;
    public GameObject controlsScreen;
    public GameObject stagePreviewOne;
    public GameObject stagePreviewTwo;
    public GameObject controlsImgOne;
    public GameObject controlsImgTwo;
    public Button stageRightCycle;
    public Button stageLeftCycle;

    private int stagePreview;
    private string currentLevel;


    // Start is called before the first frame update
    void Start()
    {
        stageSelectScreen.SetActive(false);
        controlsScreen.SetActive(false);
        stagePreview = 0;
    }

    // Update is called once per frame
    void Update()
    {
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
                currentLevel = "MoveTest";
                break;
        }
    }
    public void StartGameOpen()
    {
        startScreen.SetActive(false);
        stageSelectScreen.SetActive(true);
        stagePreviewOne.SetActive(true);
        stagePreviewTwo.SetActive(false);
    }

    public void StartGameClose()
    {
        stageSelectScreen.SetActive(false);
        startScreen.SetActive(true);
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
      controlsImgTwo.SetActive(false);
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
}


