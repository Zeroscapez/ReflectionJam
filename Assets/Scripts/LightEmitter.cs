using System.Collections.Generic;
using UnityEngine;

public class LightEmitter : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public int maxBounces = 10; // Maximum number of light bounces
    public Transform lightOrigin;

    // Store the switch that was hit in the previous frame
    private SwitchControl currentSwitch = null;

    void Start()
    {
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        lineRenderer.positionCount = 0;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
    }

    void Update()
    {
        CastLight(lightOrigin.position, -transform.up, maxBounces);
    }

    void CastLight(Vector3 position, Vector3 direction, int remainingBounces)
    {
        List<Vector3> lightPoints = new List<Vector3>();
        lightPoints.Add(position);

        // Local variable to hold the switch hit in this cast
        SwitchControl hitSwitch = null;

        while (remainingBounces > 0)
        {
            RaycastHit hit;
            if (Physics.Raycast(position, direction, out hit, Mathf.Infinity))
            {
                lightPoints.Add(hit.point);

                if (hit.collider.CompareTag("Reflective"))
                {
                    ReflectiveSurface surface = hit.collider.GetComponent<ReflectiveSurface>();
                    if (surface != null)
                    {
                        // Update direction and position, then decrement bounce counter
                        direction = surface.GetReflectionDirection();
                        position = hit.point;
                        remainingBounces--;
                        continue;
                    }
                }
                else if (hit.collider.CompareTag("Switch"))
                {
                    SwitchControl switchControl = hit.collider.GetComponent<SwitchControl>();
                    if (switchControl != null)
                    {
                        hitSwitch = switchControl;
                        // Activate the switch if it's hit
                        switchControl.Activate();
                    }
                }
                break; // End the loop if we hit a non-reflective surface
            }
            else
            {
                lightPoints.Add(position + direction * 1000f);
                break;
            }
        }

        // Only call Deactivate if the switch hit this frame is different from the previously hit switch.
        if (currentSwitch != hitSwitch)
        {
            if (currentSwitch != null)
            {
                currentSwitch.Deactivate();
            }
            currentSwitch = hitSwitch;
        }

        lineRenderer.positionCount = lightPoints.Count;
        lineRenderer.SetPositions(lightPoints.ToArray());
    }
}
