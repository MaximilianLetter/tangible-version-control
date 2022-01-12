using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities;

public class TrackedObject : MonoBehaviour
{
    private ComparisonManager comparisonManager;
    //private ObjectParts parts;

    private MeshRenderer meshRenderer;
    private Material baseMat;

    public void Initialize()
    {
        comparisonManager = AppManager.Instance.GetComparisonManager();

        VersionObject virtTwin = AppManager.Instance.GetVirtualTwin();
        Transform virtTwinModel = virtTwin.transform.GetChild(0);
        var clonedModel = Instantiate(virtTwinModel, transform).gameObject;

        clonedModel.tag = "Untagged";
        clonedModel.AddComponent<CollisionInteraction>();

        meshRenderer = clonedModel.GetComponentInChildren<MeshRenderer>();

        if (comparisonManager.usePhysical)
        {
            baseMat = comparisonManager.phantomMat;
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
    public void SetMaterial(Material mat)
    {
        meshRenderer.material = mat;
    }

    /// <summary>
    /// Resets the used material to the base materials.
    /// </summary>
    public void ResetMaterial()
    {
        meshRenderer.material = baseMat;
    }
}
