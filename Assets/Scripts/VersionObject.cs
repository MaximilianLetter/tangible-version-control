using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities;

public class VersionObject : MonoBehaviour
{
    public bool virtualTwin;
    public string title;
    public string description;
    public string createdAt;
    public string createdBy;

    private Transform objContainer;
    private MeshRenderer[] childRenderers;
    private Material[] childMats;
    private MeshOutline[] outlines;
    //private Material[] baseMats;
    //private int matCount;
    //private MeshRenderer meshRenderer;

    private void Start()
    {
        objContainer = transform.GetChild(0);
        childRenderers = new MeshRenderer[objContainer.childCount];
        childMats = new Material[objContainer.childCount];

        for (int i = 0; i < objContainer.childCount; i++)
        {
            childRenderers[i] = objContainer.GetChild(i).GetComponent<MeshRenderer>();
            childMats[i] = childRenderers[i].material;
        }

        outlines = GetComponentsInChildren<MeshOutline>();
        
        if (!virtualTwin) ToggleOutlines(false);
    }

    /// <summary>
    /// Replaces every material of the object with a placement material.
    /// </summary>
    /// <param name="mat">Material to display.</param>
    public void SetMaterial(Material mat)
    {
        //    var matArray = new Material[matCount];
        //    for (int i = 0; i < matCount; i++)
        //    {
        //        matArray[i] = mat;
        //    }

        //    meshRenderer.materials = matArray;
        foreach (var child in childRenderers)
        {
            child.material = mat;
        }
    }

    /// <summary>
    /// Reset all materials to the default materials.
    /// </summary>
    public void ResetToBaseMaterial()
    {
        //meshRenderer.materials = baseMats;
        for (int i = 0; i < childRenderers.Length; i++)
        {
            childRenderers[i].material = childMats[i];
        }
    }

    public void ToggleOutlines(bool state)
    {
        foreach (var line in outlines)
        {
            line.enabled = state;
        }
    }
}
