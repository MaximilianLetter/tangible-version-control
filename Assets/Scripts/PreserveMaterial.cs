using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreserveMaterial : MonoBehaviour
{
    private Material baseMat;

    void Awake()
    {
        baseMat = GetComponent<MeshRenderer>().material;
        Debug.Log("preserved Material : " + baseMat);
    }

    public void CopyPreservedMat(Material mat)
    {
        baseMat = mat;
    }

    public Material GetBaseMat()
    {
        return baseMat;
    }
}
