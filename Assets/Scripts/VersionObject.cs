﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class VersionObject : MonoBehaviour
{
    public bool virtualTwin;
    public string description;
    public string createdAt;
    public string createdBy;
    [HideInInspector]
    public string id;
    [HideInInspector]
    public int sequence;

    [SerializeField]
    private Transform modelContainer;

    private MeshRenderer meshRenderer;
    private Material baseMat;

    public void Initialize()
    {
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        baseMat = meshRenderer.material;
    }

    public void OverrideBaseMaterial(Material newMat)
    {
        baseMat = newMat;
        meshRenderer.material = newMat;
    }

    /// <summary>
    /// Replaces every material of the object with a placement material.
    /// </summary>
    /// <param name="mat">Material to display.</param>
    public void SetMaterial(Material mat)
    {
        meshRenderer.material = mat;
    }

    /// <summary>
    /// Reset all materials to the default materials.
    /// </summary>
    public void ResetMaterial()
    {
        meshRenderer.material = baseMat;
    }

    public Material GetBaseMaterial()
    {
        return baseMat;
    }

    public Transform GetModelContainer()
    {
        return modelContainer;
    }
}
