using UnityEngine;

public class ReflectiveSurface : MonoBehaviour
{
    public Vector3 reflectionDirection = Vector3.right; // Set this in the Inspector

    public Vector3 GetReflectionDirection()
    {
        return transform.TransformDirection(reflectionDirection);
    }
}
