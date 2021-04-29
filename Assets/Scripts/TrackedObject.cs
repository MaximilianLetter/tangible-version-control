using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackedObject : MonoBehaviour
{
    public Material phantomMat;
    public Material virtualMat;

    private MeshRenderer meshRenderer;
    private Material baseMat;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        // Switch the default material based on global setting
        if (ComparisonManager.Instance.usePhysical)
        {
            baseMat = phantomMat;
        } else
        {
            baseMat = virtualMat;
        }

        meshRenderer.material = baseMat;
    }

    public void SetMaterial(Material mat)
    {
        meshRenderer.material = mat;
    }

    public void ResetMaterial()
    {
        meshRenderer.material = baseMat;
    }
}
