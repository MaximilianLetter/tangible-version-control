using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(BoxCollider))]
public class Branch : MonoBehaviour
{
    private TimelineManager timelineManager;
    private ComparisonManager comparisonManager;
    private VersionObject[] vObjects;
    private LineRenderer branchLine;
    private BoxCollider coll;

    // Meta data of branch
    public int index;
    public string branchName;
    public string branchDate;
    private int numberOfVersions;

    void Start()
    {
        timelineManager = AppManager.Instance.GetTimelineManager();
        comparisonManager = AppManager.Instance.GetComparisonManager();
        vObjects = GetComponentsInChildren<VersionObject>();
        branchLine = GetComponent<LineRenderer>();
        coll = GetComponent<BoxCollider>();

        numberOfVersions = vObjects.Length;

        // Position objects in a line, with (0,0,0) as center point; get most left and most right positions for the line renderer
        float step = timelineManager.betweenVersionsDistance;
        float wideStep = timelineManager.branchColliderWidth;

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
        float collWidth = numberOfVersions * step + wideStep;
        coll.center = Vector3.zero;
        coll.size = new Vector3(collWidth, wideStep * 1.5f, wideStep); // make sure collider ist high enough on Y axis
        coll.isTrigger = true;
    }

    public void SetHighlightActive(bool status)
    {
        //branchLine.enabled = status;
        ToggleMaterials(status);
    }

    /// <summary>
    /// Set the collider of a branch to the given boolean.
    /// </summary>
    /// <param name="status">true equals active collider</param>
    public void SetColliderActive(bool status)
    {
        coll.enabled = status;
    }

    /// <summary>
    /// Toggle between transparent material during positioning and the default materials after the placement finished.
    /// </summary>
    /// <param name="status">True equals the normal display material, false equals the edges material.</param>
    private void ToggleMaterials(bool status)
    {
        foreach (var obj in vObjects)
        {
            if (obj.virtualTwin) continue; // Never change the appearance of the virtual twin

            if (obj == comparisonManager.GetComparedAgainstVersionObject()) continue; // Dont change the appearance of the compared against version

            if (!status)
            {
                obj.SetMaterial(timelineManager.GetEdgesMaterial());
            }
            else
            {
                obj.ResetMaterial();
            }
        }
    }

    public VersionObject[] GetVersionObjects()
    {
        return vObjects;
    }
}
