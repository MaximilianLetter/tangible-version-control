using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreserveMaterial : MonoBehaviour
{
    private Material baseMat;

    void Awake()
    {
        baseMat = GetComponent<MeshRenderer>().material;
    }

    public Material GetBaseMat()
    {
        return baseMat;
    }
}
