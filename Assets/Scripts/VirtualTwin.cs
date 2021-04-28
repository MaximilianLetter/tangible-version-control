using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities;

public class VirtualTwin : MonoBehaviour
{
    private MeshOutline outline;

    void Start()
    {
        outline = GetComponent<MeshOutline>();

        outline.enabled = true;
    }
}
