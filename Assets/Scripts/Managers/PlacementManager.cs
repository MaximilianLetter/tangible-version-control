using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using Vuforia;

public class PlacementManager : MonoBehaviour
{
    // GameObjects to align during placement
    [SerializeField]
    private GameObject versionHistoryContainer;
    [SerializeField]
    private Transform trackedContent;

    // Panels
    [SerializeField]
    private GameObject menuPanel;
    [SerializeField]
    private GameObject startUpPanel;
    [SerializeField]
    private GameObject placementPanel;

    // Material
    [SerializeField]
    private Material placementMaterial;

    // Individual versions in the timeline
    private VersionObject[] versionObjs;
    private GroundRay[] groundRays;

    private bool ready;
    private bool inPlacement;

    private void Start()
    {
        versionObjs = versionHistoryContainer.GetComponentsInChildren<VersionObject>();
        groundRays = versionHistoryContainer.GetComponentsInChildren<GroundRay>();

        inPlacement = false;

        ready = true;
    }

    private void Update()
    {
        // Only align timeline and physical artifact during placement
        if (inPlacement)
        {
            versionHistoryContainer.transform.SetPositionAndRotation(trackedContent.position, trackedContent.rotation);
        }
    }

    public void ToggleVisibilityDuringPlacement(bool status)
    {
        if (!inPlacement) return;

        versionHistoryContainer.SetActive(status);
    }

    /// <summary>
    /// Toggle between transparent material during positioning and the default materials after the placement finished.
    /// </summary>
    /// <param name="status">True equals the placement material, false equals the normal display material.</param>
    private void ToggleMaterialsAndRays(bool status)
    {
        for (int i = 0; i < versionObjs.Length; i++)
        {
            if (status)
            {
                versionObjs[i].SetMaterial(placementMaterial);
                groundRays[i].HideLine();
            }
            else
            {
                versionObjs[i].ResetMaterial();
                groundRays[i].DrawLine(versionObjs[i].transform.position);
            }
        }
    }

    /// <summary>
    /// Starts the placement process.
    /// </summary>
    public void PlacementStarts()
    {
        // For replacing the timeline while a comparison is running
        if (ComparisonManager.Instance.IsInComparison())
        {
            ComparisonManager.Instance.StopComparison();
        }

        // Activate necessary objects and scripts
        placementPanel.SetActive(true);
        menuPanel.SetActive(false);

        // Hide timeline if the physical artifact is not visible
        // TODO this is not the optimal way of getting the current tracking state
        var trackingState = trackedContent.GetComponent<DefaultTrackableEventHandler>().StatusFilter;
        if (trackingState == DefaultTrackableEventHandler.TrackingStatusFilter.Tracked)
        {
            versionHistoryContainer.SetActive(true); // it is enabled if the marker is found
        }

        ToggleMaterialsAndRays(true);

        inPlacement = true;
    }

    /// <summary>
    /// Finishes the placement process.
    /// </summary>
    public void PlacementFinished()
    {
        menuPanel.SetActive(true);
        placementPanel.SetActive(false);

        ToggleMaterialsAndRays(false);

        inPlacement = false;
    }

    public bool GetInPlacement()
    {
        return inPlacement;
    }
    public bool IsReady()
    {
        return ready;
    }
}
