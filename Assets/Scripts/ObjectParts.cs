using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities;

[RequireComponent(typeof(ObjectParts))]
public class ObjectParts : MonoBehaviour
{
    private MeshRenderer[] childRenderers;
    private Material[] childMats;
    private MeshOutline[] outlines;

    private bool ready;

    void Start()
    {
        CollectRenderersAndMaterials();
        ready = true;
    }

    public bool IsReady()
    {
        return ready;
    }

    /// <summary>
    /// Set up references to each part's renderer and get the default material.
    /// </summary>
    public void CollectRenderersAndMaterials(GameObject[] parts = null)
    {
        // This is the case for the comparison object only for collecting references while the old parts are being destroyed
        if (parts != null)
        {
            childRenderers = new MeshRenderer[parts.Length];
            for (var i = 0; i < parts.Length; i++)
            {
                childRenderers[i] = parts[i].GetComponent<MeshRenderer>();
            }
        }
        else
        {
            childRenderers = GetComponentsInChildren<MeshRenderer>();
            outlines = GetComponentsInChildren<MeshOutline>();
        }

        childMats = new Material[childRenderers.Length];
        for (int i = 0; i < childRenderers.Length; i++)
        {
            childMats[i] = childRenderers[i].material;
        }
    }

    //public void ResetRenderersAndMaterials()
    //{
    //    outlines = null;
    //    childRenderers = null;
    //    childMats = null;

    //    Debug.Log("__*_reset happened");
    //    Debug.Log(childRenderers);
    //}

    /// <summary>
    /// Replaces the material of each part of the object with the given material.
    /// </summary>
    /// <param name="mat">Material to display.</param>
    public void SetMaterial(Material mat)
    {
        foreach (var child in childRenderers)
        {
            if (child) child.material = mat;
        }
    }

    /// <summary>
    /// Reset the material of each part of the object to the default.
    /// </summary>
    /// <param name="getPreservedMaterial">Use the preserved material, this is currently only possible for the tracked object.</param>
    public void ResetMaterial(bool getPreservedMaterial = false)
    {
        Debug.Log("boolean getPresrvedmaterial " + getPreservedMaterial);
        if (getPreservedMaterial)
        {
            Debug.Log("PRESERVED MAT");
            PreserveMaterial[] preserved = GetComponentsInChildren<PreserveMaterial>();

            for (int i = 0; i < childRenderers.Length; i++)
            {
                var mat = preserved[i].GetBaseMat();
                childRenderers[i].material = mat;
            }
            return;
        }

        // In a normal case, reset to the 
        for (int i = 0; i < childRenderers.Length; i++)
        {
            childRenderers[i].material = childMats[i];
        }
    }

    public void SetOutlineMaterial(Material mat)
    {
        foreach (var line in outlines)
        {
            line.OutlineMaterial = mat;
        }
    }

    /// <summary>
    /// Toggle outlines on and off.
    /// </summary>
    /// <param name="state">True is on, false is off.</param>
    public void ToggleOutlines(bool state)
    {
        foreach (var line in outlines)
        {
            line.enabled = state;
        }
    }
}
