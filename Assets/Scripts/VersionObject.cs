using System.Collections;
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
    [SerializeField]
    private GameObject dummyModelPrefab;
    private GameObject dummyModel;

    private MeshRenderer meshRenderer;
    private Material[] baseMat;

    public void Initialize()
    {
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        baseMat = meshRenderer.materials;
    }

    public void ToggleDummyModel(bool status)
    {
        if (status)
        {
            modelContainer.gameObject.SetActive(false);
            dummyModel = Instantiate(dummyModelPrefab, transform);
        }
        else
        {
            Destroy(dummyModel);
            modelContainer.gameObject.SetActive(true);
        }
    }

    public void OverrideBaseMaterial(Material newMat)
    {
        baseMat = MultiMats.BuildMaterials(newMat, meshRenderer.materials.Length);
        meshRenderer.material = newMat;
    }

    /// <summary>
    /// Replaces every material of the object with a placement material.
    /// </summary>
    /// <param name="mat">Material to display.</param>
    public void SetMaterial(Material mat)
    {
        meshRenderer.materials = MultiMats.BuildMaterials(mat, meshRenderer.materials.Length);
    }

    /// <summary>
    /// Reset all materials to the default materials.
    /// </summary>
    public void ResetMaterial()
    {
        meshRenderer.materials = baseMat;
    }

    public Material[] GetBaseMaterial()
    {
        return baseMat;
    }

    public Transform GetModelContainer()
    {
        return modelContainer;
    }
}
