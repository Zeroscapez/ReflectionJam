using UnityEngine;
using System.Collections.Generic;

public class LightEmitter : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public int maxBounces = 10; // Maximum number of light bounces
    public Transform lightOrigin;
    void Start()
    {
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }
        lineRenderer.positionCount = 90;
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

        while (remainingBounces > 0) // Use bounce counter instead of infinite loop
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
                        // Update direction/position and decrement bounce counter
                        direction = surface.GetReflectionDirection();
                        position = hit.point;
                        remainingBounces--;
                        continue;
                    }
                }

                break; // Exit if non-reflective or no surface component
            }
            else
            {
                lightPoints.Add(position + direction * 1000f);
                break;
            }
        }

        lineRenderer.positionCount = lightPoints.Count;
        lineRenderer.SetPositions(lightPoints.ToArray());
    }
}