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
    public void CollectRenderersAndMaterials()
    {
        outlines = GetComponentsInChildren<MeshOutline>();
        childRenderers = GetComponentsInChildren<MeshRenderer>();

        childMats = new Material[childRenderers.Length];
        for (int i = 0; i < childRenderers.Length; i++)
        {
            childMats[i] = childRenderers[i].material;
        }
    }

    /// <summary>
    /// Replaces the material of each part of the object with the given material.
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
    /// Reset the material of each part of the object to the default.
    /// </summary>
    public void ResetMaterial()
    {
        for (int i = 0; i < childRenderers.Length; i++)
        {
            childRenderers[i].material = childMats[i];
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
