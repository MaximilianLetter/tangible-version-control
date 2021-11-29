using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(BoxCollider))]
public class Branch : MonoBehaviour
{
    private TimelineManager timelineManager;
    private VersionObject[] vObjects;
    private LineRenderer branchLine;
    private BoxCollider coll;

    // Meta data of branch
    public string branchName;
    public string branchDate;
    private int numberOfVersions;

    void Start()
    {
        timelineManager = AppManager.Instance.GetTimelineManager();
        vObjects = GetComponentsInChildren<VersionObject>();
        branchLine = GetComponent<LineRenderer>();
        coll = GetComponent<BoxCollider>();

        numberOfVersions = vObjects.Length;

        // Position objects in a line, with (0,0,0) as center point; get most left and most right positions for the line renderer
        float step = timelineManager.betweenVersionsDistance;

        for (int i = 0; i < vObjects.Length; i++)
        {
            var pos = new Vector3(-(step * (numberOfVersions - 1) / 2) + (i * step), 0, 0);
            vObjects[i].transform.localPosition = pos;
        }

        // Use these posititons to draw a line on the ground
        branchLine.useWorldSpace = false;
        branchLine.SetPositions(new Vector3[2] {
            vObjects[0].transform.localPosition,
            vObjects[numberOfVersions-1].transform.localPosition
        });

        // Set random color for the line
        Color randomColor = Random.ColorHSV();
        branchLine.startColor = randomColor;
        branchLine.endColor = randomColor;
        branchLine.startWidth = timelineManager.branchLineWidth;
        branchLine.endWidth = timelineManager.branchLineWidth;

        // Set the collider for the branch
        float doubleStep = step * 2;
        float collWidth = numberOfVersions * step + doubleStep;
        coll.center = Vector3.zero;
        coll.size = new Vector3(collWidth, doubleStep, doubleStep);
        coll.isTrigger = true;
    }

    public void ActivateHighlight()
    {
        branchLine.enabled = true;
    }

    public void DeactivateHighlight()
    {
        branchLine.enabled = false;
    }
}
