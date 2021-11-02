using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using Vuforia;

public class TimelineManager : MonoBehaviour
{
    private ComparisonManager comparisonManager;

    private ConnectionLine connectionLineLogic;
    private LineRenderer comparisonLine;

    // GameObjects to align during placement
    private GameObject timelineContainer;
    private Transform trackedTransform;

    // Placement buttons
    [SerializeField]
    private GameObject placeBtn;
    [SerializeField]
    private GameObject replaceBtn;

    // Material
    [SerializeField]
    private Material placementMaterial;

    // Individual versions in the timeline
    private VersionObject[] versionObjs;
    private VersionObject virtualTwin;

    private bool ready;
    private bool inPlacement;

    private void Start()
    {
        comparisonManager = AppManager.Instance.GetComparisonManager();
        timelineContainer = AppManager.Instance.GetTimelineObject();
        trackedTransform = AppManager.Instance.GetTrackedTransform();
        versionObjs = timelineContainer.GetComponentsInChildren<VersionObject>();
        virtualTwin = AppManager.Instance.GetVirtualTwin();

        // Line logic
        connectionLineLogic = FindObjectOfType<ConnectionLine>();
        comparisonLine = GameObject.Find("ComparisonLine").GetComponent<LineRenderer>();
        connectionLineLogic.SetActive(false);
        comparisonLine.enabled = false;

        inPlacement = false;

        ready = true;
    }

    private void Update()
    {
        // Only align timeline and physical artifact during placement
        if (inPlacement)
        {
            timelineContainer.transform.SetPositionAndRotation(trackedTransform.position, trackedTransform.rotation);
        }
    }

    #region Lines

    public void EnableComparisonLine(Transform obj1, Transform obj2)
    {
        float height1 = obj1.GetComponentInChildren<Collider>().bounds.size.y;
        float height2 = obj2.GetComponentInChildren<Collider>().bounds.size.y;

        Vector3 posStart = obj1.transform.position + new Vector3(0, height1 / 2, 0);
        Vector3 posEnd = obj2.transform.position + new Vector3(0, height2 / 2, 0);

        comparisonLine.enabled = true;
        comparisonLine.positionCount = 4;
        comparisonLine.SetPositions(new[] {
            posStart,
            posStart + (obj1.transform.up * height2) + (obj1.transform.up * height1 / 2),
            posEnd +  (obj2.transform.up * height1) + (obj2.transform.up * height2 / 2),
            posEnd
        });
    }

    public void DisableComparisonLine()
    {
        comparisonLine.enabled = false;
    }

    #endregion

    public void SetVirtualTwinReference(VersionObject vo)
    {
        connectionLineLogic.SetVirtualTwinTransform(vo);
    }

    /// <summary>
    /// Hides the timeline if the physical object tracking is lost. Called by VuforiaTracking.
    /// </summary>
    /// <param name="status">True means showing, false means hiding the timeline.</param>
    public void ToggleVisibilityDuringPlacement(bool status)
    {
        if (!inPlacement) return;

        timelineContainer.SetActive(status);
    }

    /// <summary>
    /// Toggle between transparent material during positioning and the default materials after the placement finished.
    /// </summary>
    /// <param name="status">True equals the placement material, false equals the normal display material.</param>
    private void ToggleMaterials(bool status)
    {
        foreach (var obj in versionObjs)
        {
            if (status)
            {
                obj.SetMaterial(placementMaterial);
            }
            else
            {
                obj.ResetMaterial();
            }
        }
    }

    /// <summary>
    /// Starts the placement process.
    /// </summary>
    public void StartPlacement()
    {
        // For repositioning the timeline while a comparison is running
        if (comparisonManager.IsInComparison())
        {
            comparisonManager.StopComparison();
        }

#if UNITY_EDITOR
        // The visibily is here not triggered by the tracked object
        if (!comparisonManager.usePhysical)
        {
            timelineContainer.SetActive(true);
        }
#endif

        // Setup placement buttons
        replaceBtn.GetComponent<TransitionToPosition>().ResetToStartPosition();
        replaceBtn.SetActive(false);
        placeBtn.SetActive(true);

        connectionLineLogic.Reset();
        connectionLineLogic.SetActive(false);

        // Display the timeline as in placement
        ToggleMaterials(false); // Workaround to fix first start bug with timeline caused by outlines
        ToggleMaterials(true);

        inPlacement = true;
    }

    /// <summary>
    /// Finishes the placement process.
    /// </summary>
    public void FinishPlacement()
    {
        // Manage the placement buttons
        placeBtn.SetActive(false);
        replaceBtn.SetActive(true);
        replaceBtn.GetComponent<TransitionToPosition>().StartTransition();

        connectionLineLogic.SetActive(true);

        // Display the timeline but the virtual twin as solid models
        ToggleMaterials(false);
        virtualTwin.GetComponent<ObjectParts>().SetMaterial(placementMaterial);

        inPlacement = false;
    }

    /// <summary>
    /// Iterate through objects in the timeline to find the virtual twin and set it as reference.
    /// </summary>
    private void FindAndSetVirtualTwin()
    {
        foreach (var obj in versionObjs)
        {
            if (obj.virtualTwin)
            {
                virtualTwin = obj;
                break;
            }
        }
    }

    /// <summary>
    /// Update the timeline after changes has been done to it, for example another object has become the virtual twin.
    /// </summary>
    public void UpdateTimeline()
    {
        //TODO remove
        FindAndSetVirtualTwin();

        ToggleMaterials(false);
        virtualTwin.GetComponent<ObjectParts>().SetMaterial(placementMaterial);
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
