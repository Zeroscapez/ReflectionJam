using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Camera Target")]
    public Transform target;  // Set this via your PlayerManager when the active character changes

    [Header("Camera Settings")]
    public float distance = 5f;         // Distance from the target
    public float heightOffset = 2f;     // Height above the target
    public float smoothSpeed = 10f;     // Smoothing speed for camera movement
    public float rotationSpeed = 150f;  // Speed of camera rotation via input
    public float verticalClampMin = 10f; // Minimum pitch angle (in degrees)
    public float verticalClampMax = 80f; // Maximum pitch angle (in degrees)

    public float sensX = 8f;
    public float sensY = 0.5f;
    float mouseX, mouseY;
    float xRotation = 0f;


    // Internal rotation state
    private float yaw;
    private float pitch;
    private Vector2 lookInput;

    void Start()
    {
        // Initialize yaw and pitch based on current camera rotation
        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;
    }

    void Update()
    {
        //if (target == null)
        //    return;

        transform.Rotate(Vector3.up, mouseX * Time.deltaTime);
    }

    private void UpdateCamera()
    {
       
        xRotation -= mouseY;
       
        transform.LookAt(target.position + Vector3.up * heightOffset);
    }

    // This method should be called by your Input System when a "Look" action is performed.
    public void HandleLookInput(Vector2 input)
    {
      mouseX = input.x * sensX;
      mouseY = input.y * sensY;   
    }

    // Method to update the camera's target (e.g., when switching characters)
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }


    //yaw += lookInput.x* rotationSpeed * Time.deltaTime;
    //    pitch -= lookInput.y* rotationSpeed * Time.deltaTime;
    //    pitch = Mathf.Clamp(pitch, verticalClampMin, verticalClampMax);

    //    Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
    //Vector3 desiredPosition = target.position + Vector3.up * heightOffset - (rotation * Vector3.forward * distance);

    //// Keep the Lerp for smooth movement
    //transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed* Time.deltaTime);
    //    transform.LookAt(target.position + Vector3.up* heightOffset);
}


  