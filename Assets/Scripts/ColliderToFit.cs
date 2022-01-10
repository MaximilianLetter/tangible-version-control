using UnityEngine;
using UnityEditor;
using System.Collections;

/// <summary>
/// Builds a perfect fitting box collider based on the children sizes.
/// From https://answers.unity.com/questions/22019/auto-sizing-primitive-collider-based-on-child-mesh.html
/// </summary>
public static class ColliderToFit
{
    public static void FitToChildren(GameObject go)
    {
        BoxCollider collider = (BoxCollider)go.GetComponent<Collider>();
        if (collider != null && !(collider is BoxCollider)) 
            return;

        bool hasBounds = false;
        Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);

        for (int i = 0; i < go.transform.childCount; ++i)
        {
            Renderer childRenderer = go.transform.GetChild(i).GetComponent<Renderer>();
            if (childRenderer != null)
            {
                if (hasBounds)
                {
                    bounds.Encapsulate(childRenderer.bounds);
                }
                else
                {
                    bounds = childRenderer.bounds;
                    hasBounds = true;
                }
            }
        }

        collider.center = bounds.center - go.transform.position;
        collider.size = bounds.size;

        // If container is minimized, update its collider bounds
        if (go.transform.localScale.x < 1)
        {
            float factor = (1 / go.transform.localScale.x);
            collider.center *= factor;
            collider.size *= factor;
        }

        // If container is rotated, flip some axis
        if (go.transform.localRotation.x != 0)
        {
            Vector3 newCenter = new Vector3(collider.center.x, collider.center.z, -collider.center.y);
            Vector3 newSize = new Vector3(collider.size.x, collider.size.z, collider.size.y);

            collider.center = newCenter;
            collider.size = newSize;
        }
    }

    public static void FitToMesh(GameObject go)
    {
        BoxCollider collider = (BoxCollider)go.GetComponent<Collider>();
        if (collider != null && !(collider is BoxCollider))
            return;

        Renderer rend = go.GetComponent<Renderer>();
        Bounds bounds = rend.bounds;

        collider.center = bounds.center - go.transform.position;
        collider.size = bounds.size;
    }
}