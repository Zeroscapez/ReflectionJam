using Cinemachine;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    private GameObject[] allCharacters;
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
        
       pCam = GetComponentInChildren<Camera>();
        guiManager = GetComponentInParent<GameManager>().GetComponentInChildren<IGUIManager>();

       

     
    }

    public void Start()
    {
        
    }

    private void Update()
    {

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
        characterA.GetComponentInChildren<Animator>().enabled = (character == characterA);
        characterB.GetComponentInChildren<Animator>().enabled = (character == characterB);
        characterA.GetComponent<Rigidbody>().isKinematic = (character != characterA);
        characterB.GetComponent<Rigidbody>().isKinematic = (character != characterB);
        pCam.GetComponent<CinemachineFreeLook>().Follow = activeCharacter.transform;
        pCam.GetComponent<CinemachineFreeLook>().LookAt = activeCharacter.transform;



    }

    public void SetupCharacters(CharacterController3D[] characterList)
    {
        allCharacters = characterList.Select(cc => cc.gameObject).ToArray();
        characterA = allCharacters.FirstOrDefault(go => go.GetComponent<CharacterController3D>().ID == 1);
        characterB = allCharacters.FirstOrDefault(go => go.GetComponent<CharacterController3D>().ID == 2);

        if (characterA != null && characterB != null)
        {
            activeCharacter = characterA;
            SetActiveCharacter(activeCharacter);
            characterController = activeCharacter.GetComponent<CharacterController3D>();

            controls.Player.SwapCharacter.performed += ctx => SwapCharacter();
            pCam.GetComponent<CinemachineFreeLook>().Follow = activeCharacter.transform;
            pCam.GetComponent<CinemachineFreeLook>().LookAt = activeCharacter.transform;
        }
        else
        {
            Debug.LogError("Characters with specified IDs not found.");
        }
    }


}
