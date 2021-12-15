﻿using System.Collections;
using UnityEngine;

public class TimelineManager : MonoBehaviour
{
    // Settings for the timeline and branches
    public float betweenVersionsDistance;
    public float betweenBranchesDistance;
    public float branchColliderWidth;
    public float branchLineWidth;

    public GameObject infoPanelTilt;
    public GameObject infoPanelVersionObject;
    public bool mirrorAllAxis;

    public int maxDescLength = 20;

    private ComparisonManager comparisonManager;

    private ConnectionLine connectionLineLogic;
    private LineRenderer comparisonLine;
    private BoxCollider coll;

    // GameObjects to align during placement
    private GameObject timelineContainer;
    private Transform movableBranchContainer;
    private Transform trackedObjectTransform;
    private Transform trackedTargetTransform;

    // Placement buttons
    [SerializeField]
    private GameObject uiPanel;
    private GameObject placeBtn;
    private GameObject otherBtns;

    // Material
    [SerializeField]
    private Material edgesMaterial;

    // Individual versions in the timeline
    private Branch[] branches;
    private VersionObject[] versionObjs;
    private VersionObject virtualTwin;

    private TransitionToPosition infoPanelVersionObjectTransition;
    private bool closeInteraction;
    private bool ready;
    private bool inPlacement;

    void Start()
    {
        comparisonManager = AppManager.Instance.GetComparisonManager();
        timelineContainer = AppManager.Instance.GetTimelineContainer();
        movableBranchContainer = timelineContainer.transform.Find("BranchContainer");
        branches = timelineContainer.GetComponentsInChildren<Branch>();
        trackedObjectTransform = AppManager.Instance.GetTrackedObjectLogic().transform;
        trackedTargetTransform = AppManager.Instance.GetTrackedTransform();
        versionObjs = timelineContainer.GetComponentsInChildren<VersionObject>();
        virtualTwin = AppManager.Instance.GetVirtualTwin();

        infoPanelVersionObjectTransition = infoPanelVersionObject.GetComponent<TransitionToPosition>();
        coll = movableBranchContainer.GetComponent<BoxCollider>();

        // UI
        placeBtn = uiPanel.transform.GetChild(0).gameObject;
        otherBtns = uiPanel.transform.GetChild(1).gameObject;

        // Line logic
        connectionLineLogic = FindObjectOfType<ConnectionLine>();
        connectionLineLogic.SetActive(false);

        comparisonLine = GameObject.Find("ComparisonLine").GetComponent<LineRenderer>();
        comparisonLine.alignment = LineAlignment.TransformZ;
        comparisonLine.useWorldSpace = false;
        comparisonLine.enabled = false;

        inPlacement = false;

        ready = true;
    }

    public void BuildTimeline()
    {
        // Timeline building out of branches
        // NOTE: this is not yet a realistic branch building but it is evenly spaced
        int mostVersionsInBranch = 0;
        Vector3 center = Vector3.zero;
        var numberOfBranches = branches.Length;
        for (int i = 0; i < numberOfBranches; i++)
        {
            var pos = new Vector3(0, 0, i * betweenBranchesDistance);
            branches[i].transform.localPosition = pos;

            // Get longest branch
            int length = branches[i].GetVersionObjects().Length;
            if (length > mostVersionsInBranch)
            {
                mostVersionsInBranch = length;
            }
            center += branches[i].transform.localPosition;
        }

        center /= branches.Length;
        float collWidth = mostVersionsInBranch * betweenVersionsDistance;
        coll.center = center;
        coll.size = new Vector3(collWidth, branchColliderWidth * 1.5f, branchColliderWidth * branches.Length); // make sure collider ist high enough on Y axis
        coll.isTrigger = true;
    }

    private void Update()
    {
        // Only align timeline and physical artifact during placement
        if (inPlacement)
        {
            timelineContainer.transform.SetPositionAndRotation(trackedObjectTransform.position, trackedTargetTransform.rotation);
        } else
        {
            if (closeInteraction)
            {
                // Rotate versions according to tracked object rotation
                Quaternion objRot = trackedTargetTransform.localRotation;

                if (mirrorAllAxis)
                {
                    foreach (var vo in versionObjs)
                    {
                        vo.transform.rotation = objRot;
                    }
                }
                else
                {
                    Quaternion rot = Quaternion.Euler(0, objRot.eulerAngles.y, 0);
                    foreach (var vo in versionObjs)
                    {
                        vo.transform.rotation = rot;
                    }
                }

                connectionLineLogic.ConnectVirtualAndPhysical();
            }
        }
    }

    #region Lines

    /// <summary>
    /// Sets the start and endpoints for the comparison line.
    /// </summary>
    /// <param name="obj1">Startpoint of line.</param>
    /// <param name="obj2">Endpoint of line.</param>
    public void EnableComparisonLine(Transform obj1, Transform obj2)
    {
        float height1 = obj1.GetChild(0).GetComponent<Collider>().bounds.size.y;
        float height2 = obj2.GetChild(0).GetComponent<Collider>().bounds.size.y;

        // Add the parent's local position as the branches have offsets to each other aswell.
        Vector3 posStart = obj1.localPosition + obj1.parent.localPosition + new Vector3(0, height1 / 2, 0);
        Vector3 posEnd = obj2.localPosition + obj2.parent.localPosition + new Vector3(0, height2 / 2, 0);

        comparisonLine.enabled = true;
        comparisonLine.positionCount = 4;
        comparisonLine.SetPositions(new[] {
            posStart,
            posStart + (obj1.up * height2) + (obj1.up * height1 / 2),
            posEnd +  (obj2.up * height1) + (obj2.up * height2 / 2),
            posEnd
        });
    }

    public void DisableComparisonLine()
    {
        comparisonLine.enabled = false;
    }

    #endregion

    #region Placement

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
        uiPanel.transform.localPosition = Vector3.zero;
        otherBtns.SetActive(false);
        placeBtn.SetActive(true);

        // Move the timeline so that the virtual twin is at the 0,0,0 position and matches with the physical artifact
        movableBranchContainer.localPosition = -virtualTwin.transform.localPosition;

        // Reset rotation of objects
        foreach (var vo in versionObjs)
        {
            vo.transform.localRotation = Quaternion.identity;
        }

        connectionLineLogic.Reset();
        connectionLineLogic.SetActive(false);

        // Display the timeline as in placement
        foreach(var branch in branches)
        {
            branch.SetColliderActive(false);
            branch.SetHighlightActive(false);
        }
        SetColliderActive(false);
        ToggleMaterials(false); // Ensure that the virtual twin is also reset

        SetVersionInfoPanel(null);

        inPlacement = true;
    }

    /// <summary>
    /// Finishes the placement process.
    /// </summary>
    public void FinishPlacement()
    {
        // Manage the placement buttons
        placeBtn.SetActive(false);
        otherBtns.SetActive(true);
        uiPanel.GetComponent<TransitionToPosition>().StartTransition(new Vector3(0, 0.1f, 0), true);

        connectionLineLogic.SetActive(true);

        // Display the timeline but the virtual twin as solid models
        foreach (var branch in branches)
        {
            branch.SetColliderActive(true);
            branch.SetHighlightActive(true);
        }
        SetColliderActive(true);

        // For the moment fix. Required on HL1.
        ToggleMaterials(true);
        foreach (var vo in versionObjs)
        {
            vo.GetComponent<ObjectParts>().ToggleOutlines(false);
        }

        //virtualTwin.GetComponent<ObjectParts>().SetMaterial(edgesMaterial);

        SetVersionInfoPanel(virtualTwin);

        inPlacement = false;
    }

    private void SetColliderActive(bool status)
    {
        coll.enabled = status;
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
    /// <param name="status">True equals the normal display material, false equals the normal placement edges material.</param>
    public void ToggleMaterials(bool status)
    {
        foreach (var obj in versionObjs)
        {
            if (!status)
            {
                obj.SetMaterial(edgesMaterial);
            }
            else
            {
                if (obj.virtualTwin) continue;

                obj.ResetMaterial();
            }
        }
    }

    #endregion

    #region Movement

    public void MoveCenter(bool left)
    {
        movableBranchContainer.localPosition += new Vector3(betweenVersionsDistance * (left ? -1f : 1f), 0, 0);
    }

    #endregion

    public void SetCloseInteraction(bool status, VersionObject vo = null)
    {
        if (status)
        {
            SetVersionInfoPanel(vo);

            if (!closeInteraction)
            {
                ToggleMaterials(true);
                closeInteraction = status;
                connectionLineLogic.StartCoroutine("FadeLine", true);
            }
        }
        else
        {
            if (closeInteraction)
            {
                ToggleMaterials(false);
                SetVersionInfoPanel(null);
                connectionLineLogic.StartCoroutine("FadeLine", false);
            }
        }

        closeInteraction = status;
    }

    private void SetVersionInfoPanel(VersionObject vo)
    {
        if (vo == null)
        {
            infoPanelVersionObject.SetActive(false);
            return;
        }

        var plate = infoPanelVersionObject.GetComponent<VersionPlate>();
        string desc = vo.description;
        desc = desc.Length > maxDescLength ? (desc.Substring(0, maxDescLength) + "...") : desc;
        plate.SetText(vo.id + "\n" + desc + "\n" + vo.createdBy + " " + vo.createdAt);

        //infoPanelVersionObject.transform.position = vo.transform.position;
        infoPanelVersionObject.SetActive(true);

        infoPanelVersionObjectTransition.StartTransition(vo.transform.position);
    }

    /// <summary>
    /// Update the timeline after changes has been done to it, for example another object has become the virtual twin.
    /// </summary>
    public void UpdateTimeline()
    {
        virtualTwin = AppManager.Instance.GetVirtualTwin();

        ToggleMaterials(true);
        virtualTwin.GetComponent<ObjectParts>().SetMaterial(edgesMaterial);
    }

    /// <summary>
    /// Sets the reference of virtual twin to the connection line.
    /// </summary>
    /// <param name="vo">The version object representing the virtual twin.</param>
    public void SetVirtualTwinReference(VersionObject vo)
    {
        connectionLineLogic.SetVirtualTwinTransform(vo);
    }

    public Material GetEdgesMaterial()
    {
        return edgesMaterial;
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
