using UnityEngine;

public class PlatformVelocity : MonoBehaviour
{
    private Vector3 lastPosition;
    public Vector3 CurrentVelocity { get; private set; }

    void Start()
    {
        lastPosition = transform.position;
    }

    void FixedUpdate()
    {
        Vector3 newPosition = transform.position;
        CurrentVelocity = (newPosition - lastPosition) / Time.fixedDeltaTime;
        lastPosition = newPosition;
    }
}
