using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Branch : MonoBehaviour
{
    private TimelineManager timelineManager;
    private ComparisonManager comparisonManager;
    private VersionObject[] vObjects;
    private BoxCollider[] vObjColliders;
    private LineRenderer branchLine;
    //private BoxCollider branchColl;

    // Meta data of branch
    public int index;
    public string branchName;
    public string lastCommit;
    private int numberOfVersions;

    private bool ready;

    public void Initialize()
    {
        timelineManager = AppManager.Instance.GetTimelineManager();
        comparisonManager = AppManager.Instance.GetComparisonManager();
        vObjects = GetComponentsInChildren<VersionObject>();
        branchLine = GetComponent<LineRenderer>();
        //branchColl = GetComponent<BoxCollider>();

        numberOfVersions = vObjects.Length;

        vObjColliders = new BoxCollider[numberOfVersions];

        // Position objects in a line, with (0,0,0) as center point; get most left and most right positions for the line renderer
        float step = timelineManager.betweenVersionsDistance;
        float wideStep = timelineManager.branchColliderWidth;

        for (int i = 0; i < vObjects.Length; i++)
        {
            //vObjects[i].id = branchName + "_" + i;

            //var pos = new Vector3(-(step * (numberOfVersions - 1) / 2) + (i * step), 0, 0);
            //vObjects[i].transform.localPosition = pos;

            // Setup collider based on stepsize
            var coll = vObjects[i].GetComponent<BoxCollider>();
            coll.size = new Vector3(step, wideStep * 1.5f, wideStep);
            coll.isTrigger = true;

            vObjColliders[i] = coll;
        }

        // Set random color for the line
        Color randomColor = Random.ColorHSV();
        if (AppManager.Instance.experiment)
        {
            randomColor = new Color(0.859f, 0.047f, 0.039f);
        }
        branchLine.startColor = randomColor;
        branchLine.endColor = randomColor;
        branchLine.startWidth = timelineManager.branchLineWidth;
        branchLine.endWidth = timelineManager.branchLineWidth;
        branchLine.useWorldSpace = false;

        // Set the collider for the branch
        //float collWidth = numberOfVersions * step;
        //branchColl.center = Vector3.zero;
        //branchColl.size = new Vector3(collWidth, wideStep * 1.5f, wideStep); // make sure collider ist high enough on Y axis
        //branchColl.isTrigger = true;

        ready = true;
    }

    public void SetBranchLinePositionsAndOrder(int order)
    {
        branchLine.positionCount = transform.childCount;

        List<Vector3> positions = new List<Vector3>();

        foreach (Transform child in transform)
        {
            positions.Add(child.localPosition);
        }

        branchLine.SetPositions(positions.ToArray());

        branchLine.sortingOrder = order;
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
        foreach (var coll in vObjColliders)
        {
            coll.enabled = status;
        }
        //branchColl.enabled = status;
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

    public bool IsReady()
    {
        return ready;
    }
}
