using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineManager : MonoBehaviour
{
    private ConnectionLine connectionLineLogic;
    private LineRenderer comparisonLine;

    void Start()
    {
        connectionLineLogic = FindObjectOfType<ConnectionLine>();
        // TODO comparisonLine
    }

    public void DisableConnectionLine()
    {
        // Reset the connection line to be invisible
        connectionLineLogic.Reset();
        connectionLineLogic.enabled = false;
    }
    
    public void EnableConnectionLine()
    {
        connectionLineLogic.enabled = true;
    }

    public void SetVirtualTwinReference(VersionObject vo)
    {
        connectionLineLogic.SetVirtualTwinTransform(vo);
    }
}
