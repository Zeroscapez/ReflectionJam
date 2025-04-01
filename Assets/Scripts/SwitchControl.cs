using ProBuilder2.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchControl : MonoBehaviour
{
    public GameObject[] platforms;
    


    public void Awake()
    {
        foreach (GameObject platform in platforms)
        {
            MovingPlatform charles = platform.GetComponent<MovingPlatform>();
            charles.activated = false;
        }
    }

    public void Activate()
    {
    
    }
    

    public void Deactivate()
    {

    }
   
}
