using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackedObject : MonoBehaviour
{
    public Material phantomMat;

    private ObjectParts parts;

    void Start()
    {
        parts = GetComponent<ObjectParts>();

        // Switch the default material based on global setting
        if (ComparisonManager.Instance.usePhysical)
        {
            SetMaterial(phantomMat);
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
