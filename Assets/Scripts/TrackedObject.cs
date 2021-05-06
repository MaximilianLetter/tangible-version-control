using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackedObject : MonoBehaviour
{
    public Material phantomMat;

    private MeshRenderer[] childRenderers;
    private Material[] childMats;

    void Start()
    {
        // Save a reference to the base materials
        childRenderers = new MeshRenderer[transform.childCount];
        childMats = new Material[transform.childCount];

        for (int i = 0; i < transform.childCount; i++)
        {
            childRenderers[i] = transform.GetChild(i).GetComponent<MeshRenderer>();
            childMats[i] = childRenderers[i].material;
        }

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
        foreach (var child in childRenderers)
        {
            child.material = mat;
        }
    }

    /// <summary>
    /// Resets all materials to the base materials.
    /// </summary>
    public void ResetMaterial()
    {
        for (int i = 0; i < childRenderers.Length; i++)
        {
            childRenderers[i].material = childMats[i];
        }
    }
}
