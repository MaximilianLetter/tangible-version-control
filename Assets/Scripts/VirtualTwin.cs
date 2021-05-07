using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities;

public class VirtualTwin : MonoBehaviour
{
    void Start()
    {
        var outline = GetComponentsInChildren<MeshOutline>();

        foreach (var line in outline)
        {
            line.enabled = true;
        }
    }
}
