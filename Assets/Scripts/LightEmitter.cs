using UnityEngine;
using System.Collections.Generic;

public class LightEmitter : MonoBehaviour
{
    public int maxReflections = 5; // Maximum number of reflections
    public float maxDistance = 20f; // Max distance the light travels
    public LineRenderer lineRenderer; // Reference to LineRenderer

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
        CastLight(transform.position, transform.right, maxReflections);
    }

    void CastLight(Vector3 position, Vector3 direction, int reflectionsRemaining)
    {
        List<Vector3> lightPoints = new List<Vector3>();
        lightPoints.Add(position);

        for (int i = 0; i < reflectionsRemaining; i++)
        {
            RaycastHit hit;
            if (Physics.Raycast(position, direction, out hit, maxDistance))
            {
                lightPoints.Add(hit.point);

                if (hit.collider.CompareTag("Reflective"))
                {
                    ReflectiveSurface surface = hit.collider.GetComponent<ReflectiveSurface>();
                    if (surface != null)
                    {
                        direction = surface.GetReflectionDirection(); // Get predefined reflection direction
                        position = hit.point;
                        continue;
                    }
                }

                break; // Stop if not reflective
            }
            else
            {
                lightPoints.Add(position + direction * maxDistance);
                break;
            }
        }

        lineRenderer.positionCount = lightPoints.Count;
        lineRenderer.SetPositions(lightPoints.ToArray());
    }
}
