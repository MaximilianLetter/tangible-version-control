using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities;

public class TrackedObject : MonoBehaviour
{
    private ComparisonManager comparisonManager;
    //private ObjectParts parts;

    private MeshRenderer meshRenderer;
    private Material[] baseMat;

    private AudioSource audioSrc;

    public void Initialize()
    {
        audioSrc = GetComponent<AudioSource>();
        comparisonManager = AppManager.Instance.GetComparisonManager();

        VersionObject virtTwin = AppManager.Instance.GetVirtualTwin();
        Transform virtTwinModel = virtTwin.transform.GetChild(0);
        var clonedModel = Instantiate(virtTwinModel, transform).gameObject;

        clonedModel.tag = "Untagged";
        clonedModel.AddComponent<CollisionInteraction>();

        meshRenderer = clonedModel.GetComponentInChildren<MeshRenderer>();

        if (comparisonManager.usePhysical)
        {
            baseMat = MultiMats.BuildMaterials(comparisonManager.phantomMat, meshRenderer.materials.Length);
        }
        else
        {
            baseMat = virtTwin.GetBaseMaterial();
        }

        if (comparisonManager.usePhysical)
        {
            // Set and override base material as phantom
            SetMaterial(comparisonManager.phantomMat);
        }
        else
        {
            SetMaterial(AppManager.Instance.GetVirtualTwin().GetBaseMaterial());
        }

        gameObject.SetActive(true);
    }

    /// <summary>
    /// Replaces the used material by the given material.
    /// </summary>
    /// <param name="mat">Material to display.</param>
    public void SetMaterial(Material[] mat)
    {
        meshRenderer.materials = mat;
    }

    public void SetMaterial(Material mat)
    {
        meshRenderer.materials = MultiMats.BuildMaterials(mat, meshRenderer.materials.Length);
    }

    /// <summary>
    /// Resets the used material to the base materials.
    /// </summary>
    public void ResetMaterial()
    {
        meshRenderer.materials = baseMat;
    }

    public void PlaySound()
    {
        audioSrc.Play();
    }
}
