using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PickupObject : MonoBehaviour, IInteractable
{
    private Transform objectHolder;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void OnInteract()
    {
        // Get the active character's object holder
        objectHolder = GetActiveCharacterObjectHolder();

        if (objectHolder == null)
        {
            Debug.LogError("No active character or object holder found!");
            return;
        }

        // Pick up or drop depending on current state
        if (transform.parent == objectHolder)
        {
            Drop();
        }
        else
        {
            PickUp();
        }
    }

    private Transform GetActiveCharacterObjectHolder()
    {
        PlayerManager manager = FindObjectOfType<PlayerManager>();
        if (manager != null && manager.activeCharacter != null)
        {
            CharacterController3D characterController = manager.activeCharacter.GetComponent<CharacterController3D>();
            if (characterController != null)
            {
                Transform holder = characterController.transform.Find("ObjectHolder");
                return holder;
            }
        }
        return null;
    }

    private void PickUp()
    {
        Debug.Log("Picked Up");
        rb.isKinematic = true;
        transform.SetParent(objectHolder);
        transform.localPosition = Vector3.zero;
    }

    private void Drop()
    {
        Debug.Log("Dropped");
        transform.SetParent(null);
        rb.isKinematic = false;
    }
}
