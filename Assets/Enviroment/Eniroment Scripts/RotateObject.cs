using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObject : MonoBehaviour
{
    public float rotationSpeed = 50f; // Speed of rotation (degrees per second)

    void Update()
    {
        // Rotate the object around its Y-axis (you can change this to X or Z for different axes)
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }
}
