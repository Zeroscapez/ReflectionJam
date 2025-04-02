using UnityEngine;

public class SwitchControl : MonoBehaviour
{
    // Parent object that contains all controllable platforms as children.
    public GameObject platformParent;
    private MovingPlatform[] platformScripts;

    private bool isActive = false;

    void Start()
    {
        if (platformParent != null)
        {
            // Get all MovingPlatform components in the children of platformParent.
            platformScripts = platformParent.GetComponentsInChildren<MovingPlatform>();

            // Initialize all platforms as inactive.
            foreach (MovingPlatform mp in platformScripts)
            {
                if (mp != null)
                {
                    mp.activated = false;
                }
            }
        }
        else
        {
            Debug.LogWarning("Platform parent is not assigned.");
        }
    }

    // Activate all platforms under the parent.
    public void Activate()
    {
        if (!isActive && platformScripts != null)
        {
            isActive = true;
            foreach (MovingPlatform mp in platformScripts)
            {
                if (mp != null)
                {
                    mp.activated = true;
                }
            }
            Debug.Log("Switch Activated: Platforms set to active.");
        }
    }

    // Deactivate all platforms under the parent.
    public void Deactivate()
    {
        if (isActive && platformScripts != null)
        {
            isActive = false;
            foreach (MovingPlatform mp in platformScripts)
            {
                if (mp != null)
                {
                    mp.activated = false;
                }
            }
            Debug.Log("Switch Deactivated: Platforms set to inactive.");
        }
    }
}
