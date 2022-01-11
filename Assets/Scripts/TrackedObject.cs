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
            //parts.CollectRenderersAndMaterials(objParts);
        }
        else
        {
            SetMaterial(AppManager.Instance.GetVirtualTwin().GetBaseMaterial());
            //parts.CollectRenderersAndMaterials(objParts);
        }

        gameObject.SetActive(true);
    }

    //private void CloneVirtualTwin()
    //{
    //    Transform virtTwinModel = AppManager.Instance.GetVirtualTwin().transform.GetChild(0);
    //    //var objParts = new GameObject[virtTwinObjsTransf.childCount];
    //    //for (int i = 0; i < virtTwinObjsTransf.childCount; i++)
    //    //{
    //    //    // Clone each part of the object, remove the MeshOutline Script
    //    //    GameObject original = virtTwinObjsTransf.GetChild(i).gameObject;
    //    //    GameObject part = Instantiate(original, transform);

    //    //    // doesnt change anything
    //    //    //part.transform.SetParent(transform);
    //    //    //part.transform.localPosition = original.transform.localPosition;

    //    //    var outL = part.GetComponent<MeshOutline>();
    //    //    if (outL != null)
    //    //    {
    //    //        if (comparisonManager.usePhysical)
    //    //        {
    //    //            Material[] mats = new Material[1] { comparisonManager.phantomMat };
    //    //            part.GetComponent<MeshRenderer>().materials = mats;
    //    //        }
    //    //        else
    //    //        {
    //    //            Material[] mats = new Material[1] { part.GetComponent<PreserveMaterial>().GetBaseMat() };
    //    //            part.GetComponent<MeshRenderer>().materials = mats;
    //    //        }
    //    //    }


    //    // Make sure the real name of the part is kept for part-wise comparisons
    //    //part.name = original.name;

    //    // Store necessary information about parts
    //    //objParts[i] = part;
    //        Instantiate(virtTwinModel, transform);
    //    }

    //    // adjust collider to new object
    //    //ColliderToFit.FitToChildren(gameObject);

    //    if (ComparisonManager.usePhysical)
    //    {
    //        // Set and override base material as phantom
    //        SetMaterial(comparisonManager.phantomMat);
    //        //parts.CollectRenderersAndMaterials(objParts);
    //    }
    //    else
    //    {
    //        //parts.CollectRenderersAndMaterials(objParts);
    //    }
    //}

    /// <summary>
    /// Replaces all materials by the given material.
    /// </summary>
    /// <param name="mat">Material to display.</param>
    public void SetMaterial(Material mat)
    {
        meshRenderer.material = mat;
    }

    /// <summary>
    /// Resets all materials to the base materials.
    /// </summary>
    public void ResetMaterial()
    {
        meshRenderer.material = baseMat;
    }
}
