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
    }

}