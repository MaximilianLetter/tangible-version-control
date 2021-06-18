using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectPhysicalObjectToTimeline : MonoBehaviour
{
    public float lowerDist;
    public float upperDist;
    private float distRange;
    private float farUpperDist;

    // External references
    private Transform physObj;
    private Transform virtTwin;

    // Internal references
    private LineRenderer lineRend;
    private Material lineMat;
    private Color lineCol;

    void Start()
    {
        lineRend = GetComponent<LineRenderer>();
        lineMat = lineRend.material;
        lineCol = lineMat.color;
        lineRend.positionCount = 2;

        physObj = FindObjectOfType<TrackedObject>().transform;
        
        // Find virtual twin
        var timelineObjs = FindObjectsOfType<VersionObject>();
        foreach (var vo in timelineObjs)
        {
            if (vo.virtualTwin)
            {
                virtTwin = vo.transform;
                break;
            }
        }

        distRange = upperDist - lowerDist;
        farUpperDist = upperDist + 0.1f;
    }

    void Update()
    {
        float dist = Vector3.Distance(physObj.position, virtTwin.position);

        if (dist > farUpperDist)
        {
            return;
        }

        // Make sure the line is fully invisible when distance threshold is reached
        if (dist > upperDist)
        {
            Reset();
            return;
        }

        float alpha = Mathf.Min(1.0f - ((dist - lowerDist) / distRange), 1.0f);

        lineCol.a = alpha;
        lineMat.color = lineCol;

        lineRend.SetPositions(new[]
        {
             physObj.position,
             virtTwin.position
        });
    }

    /// <summary>
    /// Resets the line positions to zero, zero which results in an invisible line.
    /// </summary>
    public void Reset()
    {
        lineRend.SetPositions(new Vector3[2] { Vector3.zero, Vector3.zero });
    }
}
