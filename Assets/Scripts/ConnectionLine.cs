using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionLine : MonoBehaviour
{
    public float lowerDist;
    public float upperDist;
    private float distRange;
    private float farUpperDist;

    // External references
    private Transform physObj;
    private Transform virtualTwin;

    // Internal references
    private LineRenderer lineRend;
    private Material lineMat;
    private Color lineCol;

    private bool isActive;

    void Start()
    {
        lineRend = GetComponent<LineRenderer>();
        lineMat = lineRend.material;
        lineCol = lineMat.color;
        lineRend.positionCount = 4;

        physObj = FindObjectOfType<TrackedObject>().transform;

        SetVirtualTwinTransform(AppManager.Instance.GetVirtualTwin());

        distRange = upperDist - lowerDist;
        farUpperDist = upperDist + 0.1f;
    }

    void Update()
    {
        if (!isActive) return;

        float dist = Vector3.Distance(physObj.position, virtualTwin.position);

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
             Vector3.Lerp(physObj.position, virtualTwin.position, 0.15f),
             Vector3.Lerp(physObj.position, virtualTwin.position, 0.85f),
             virtualTwin.position
        });
    }

    /// <summary>
    /// Resets the line positions to zero, zero which results in an invisible line.
    /// </summary>
    public void Reset()
    {
        lineRend.SetPositions(new Vector3[4] { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero });
    }

    public void SetActive(bool state)
    {
        isActive = state;
    }

    /// <summary>
    /// Iterate through objects in the timeline to find the virtual twin and set it as reference.
    /// </summary>
    public void SetVirtualTwinTransform(VersionObject vo)
    {
        virtualTwin = vo.transform;
    }
}
