using UnityEngine;

public class MouseLock : MonoBehaviour
{
    void Update()
    {
        // Lock the cursor when the user clicks the game view
        if (Input.GetMouseButtonDown(0)) // Left mouse button or touch
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // Unlock the cursor when pressing Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}