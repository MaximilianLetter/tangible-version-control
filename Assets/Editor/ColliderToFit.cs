using UnityEngine;
using UnityEditor;
using System.Collections;

/// <summary>
/// Builds a perfect fitting box collider based on the children sizes.
/// From https://answers.unity.com/questions/22019/auto-sizing-primitive-collider-based-on-child-mesh.html
/// </summary>
public class ColliderToFit : MonoBehaviour
{

    [MenuItem("Helpers/Collider/Fit to Children")]
    static void FitToChildren()
    {
        foreach (GameObject rootGameObject in Selection.gameObjects)
        {
            BoxCollider collider = (BoxCollider)rootGameObject.GetComponent<Collider>();
            if (collider != null && !(collider is BoxCollider))
                continue;

            bool hasBounds = false;
            Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);

            for (int i = 0; i < rootGameObject.transform.childCount; ++i)
            {
                Renderer childRenderer = rootGameObject.transform.GetChild(i).GetComponent<Renderer>();
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

            collider.center = bounds.center - rootGameObject.transform.position;
            collider.size = bounds.size;
        }
    }

}