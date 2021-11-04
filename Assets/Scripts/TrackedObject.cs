using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities;

public class TrackedObject : MonoBehaviour
{
    private ComparisonManager comparisonManager;
    private ObjectParts parts;
    private bool ready;

    IEnumerator Start()
    {
        comparisonManager = AppManager.Instance.GetComparisonManager();
        parts = GetComponent<ObjectParts>();

        // Wait for the timeline to be ready setup
        yield return null;

        CloneVirtualTwin();

        // Wait until all component references are gathered
        while (true)
        {
            if (parts.IsReady()) break;

            yield return null;
        }

        ready = true;
    }

    private void CloneVirtualTwin()
    {
        Transform virtTwinObjsTransf = AppManager.Instance.GetVirtualTwin().transform.GetChild(0);
        var objParts = new GameObject[virtTwinObjsTransf.childCount];
        for (int i = 0; i < virtTwinObjsTransf.childCount; i++)
        {
            // Clone each part of the object, remove the MeshOutline Script
            GameObject original = virtTwinObjsTransf.GetChild(i).gameObject;
            GameObject part = Instantiate(original, transform);

            // doesnt change anything
            //part.transform.SetParent(transform);
            //part.transform.localPosition = original.transform.localPosition;

            var outL = part.GetComponent<MeshOutline>();
            if (outL != null)
            {
                if (comparisonManager.usePhysical)
                {
                    Material[] mats = new Material[1] { comparisonManager.phantomMat };
                    part.GetComponent<MeshRenderer>().materials = mats;
                }
                else
                {
                    Material[] mats = new Material[1] { part.GetComponent<PreserveMaterial>().GetBaseMat() };
                    part.GetComponent<MeshRenderer>().materials = mats;
                }
            }

            // Make sure the real name of the part is kept for part-wise comparisons
            part.name = original.name;

            // Store necessary information about parts
            objParts[i] = part;
        }

        // adjust collider to new object
        ColliderToFit.FitToChildren(gameObject);

        if (comparisonManager.usePhysical)
        {
            // Set and override base material as phantom
            SetMaterial(comparisonManager.phantomMat);
            parts.CollectRenderersAndMaterials(objParts);
        }
        else
        {
            parts.CollectRenderersAndMaterials(objParts);
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

    public bool IsReady()
    {
        return ready;
    }
}
