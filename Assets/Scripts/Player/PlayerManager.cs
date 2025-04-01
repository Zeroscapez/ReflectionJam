using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    public GameObject characterA;
    public GameObject characterB;
    public GameObject activeCharacter;
    private CharacterController3D characterController;
    private PlayerControls controls;
    public IGUIManager guiManager;
    public bool isCharacterA;
   
    public Camera pCam;

    private void Awake()
    {
        controls = new PlayerControls();
        activeCharacter = characterA;

        // Make sure only the active character has enabled input
        SetActiveCharacter(activeCharacter);

        characterController = activeCharacter.GetComponent<CharacterController3D>();
        controls.Player.SwapCharacter.performed += ctx => SwapCharacter();
        
        pCam.GetComponent<CinemachineFreeLook>().Follow = activeCharacter.transform;
        pCam.GetComponent<CinemachineFreeLook>().LookAt = activeCharacter.transform;
    }

    private void Update()
    {
     if (activeCharacter == characterA)
        {
            isCharacterA = true;
        }
        else
        {
            isCharacterA = false;
        }
    }



    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();

    void SwapCharacter()
    {
        Debug.Log(characterController.Name + " " + characterController.ID.ToString() + " Character Swap Pressed");
        // Swap active character
        activeCharacter = (activeCharacter == characterA) ? characterB : characterA;
        SetActiveCharacter(activeCharacter);
        guiManager.SwitchCharacterImage();
    }

    void SetActiveCharacter(GameObject character)
    {
       
        // Enable PlayerInput on the active character and disable on the other.
        characterA.GetComponent<CharacterController3D>().enabled = (character == characterA);
        characterB.GetComponent<CharacterController3D>().enabled = (character == characterB);
        pCam.GetComponent<CinemachineFreeLook>().Follow = activeCharacter.transform;
        pCam.GetComponent<CinemachineFreeLook>().LookAt = activeCharacter.transform;



    }

}
