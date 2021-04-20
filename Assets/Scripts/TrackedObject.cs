using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackedObject : MonoBehaviour
{
    private MeshRenderer meshRenderer;
    private Material baseMat;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        baseMat = meshRenderer.material;
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
