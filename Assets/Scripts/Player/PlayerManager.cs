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
   
    public Camera pCam;

    private void Awake()
    {
        controls = new PlayerControls();
        
       pCam = GetComponentInChildren<Camera>();

        allCharacters = GameObject.FindObjectsOfType<CharacterController3D>()
            .Where(cc => cc != null) // Ensure the component exists
            .Select(cc => cc.gameObject)
            .ToArray();

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
