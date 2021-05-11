using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackedObject : MonoBehaviour
{
    private ObjectParts parts;

    IEnumerator Start()
    {
        parts = GetComponent<ObjectParts>();

        // Wait until all component references are gathered
        while (true)
        {
            if (parts.IsReady()) break;

            yield return null;
        }

        // Switch the default material based on global setting
        if (ComparisonManager.Instance.usePhysical)
        {
            // Set and override base material as phantom
            SetMaterial(ComparisonManager.Instance.phantomMat);
            parts.CollectRenderersAndMaterials();
        }
    }

    /// <summary>
    /// Replaces all materials by the given material.
    /// </summary>
    /// <param name="mat">Material to display.</param>
    public void SetMaterial(Material mat)
    {
        parts.SetMaterial(mat);
    }

    /// <summary>
    /// Resets all materials to the base materials.
    /// </summary>
    public void ResetMaterial()
    {
        parts.ResetMaterial();
    }
}
