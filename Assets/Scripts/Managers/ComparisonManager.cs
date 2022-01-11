﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities;
public enum ComparisonMode { SideBySide, Overlay, Differences }

public class ComparisonManager : MonoBehaviour
{
    // Use tracking and physical objects
    public bool usePhysical;
    public float staticFloatingDistance;
    public float pulseCadence;
    public float pulseHold;

    [Header("Object Materials")]
    public Material phantomMat;
    public Material invisibleMat;
    public Material edgesMat;
    public Material greenMat;
    public Material redMat;
    public Material yellowMat;
    public Material[] overlayMats;

    //NEW
    public Material diffMatBase;
    public Material diffMatAdded;
    public Material diffMatSubtracted;

    //[Space(10)]
    [Header("Outline Materials")]
    public float outlineWidth;
    public Material neutralHighlight;
    public Material greenHighlight;
    public Material redHighlight;
    public Material transitionHighlight;

    public Color32 textHighlight;
    public Color32 textDefault;
    public Color32 red;
    public Color32 green;

    // Required object references
    private ActionPanel actionPanel;
    private TrackedObject trackedObj;
    private Transform trackedTransform;
    private ComparisonObject comparisonObj;
    private Transform comparisonObjContainer;
    private VersionObject virtualTwin;
    private TimelineManager timelineManager;

    // State variables
    private VersionObject comparedAgainstVersionObject;
    private float floatingDistance;
    private bool inComparison;

    [Space(14)]
    public ComparisonMode mode;

    public void Initialize()
    {
        // Get relevant gameobject logic
        virtualTwin = AppManager.Instance.GetVirtualTwin();
        actionPanel = AppManager.Instance.GetActionPanel();
        timelineManager = AppManager.Instance.GetTimelineManager();
        comparisonObj = AppManager.Instance.GetComparisonObjectLogic();
        comparisonObjContainer = comparisonObj.transform.parent;

        // Get relevant transform information
        trackedObj = AppManager.Instance.GetTrackedObjectLogic();
        trackedTransform = AppManager.Instance.GetTrackedTransform();
        
        // NOTE: giving the markerPlane a phantom materials results in unwanted behavior occluding the comparison object
        //var markerPlane = trackedTransform.Find("MarkerPlane");
        //if (markerPlane != null)
        //{
        //    markerPlane.GetComponent<Renderer>().material = phantomMat;
        //}

        // Initialize states
        inComparison = false;
        mode = ComparisonMode.SideBySide;

#if UNITY_EDITOR
        // A bigger floating distance is required on HoloLens
        staticFloatingDistance = 0.045f;
#endif
    }

    /// <summary>
    /// Starts a comparison between the tracked physical object and the collided object of the version history.
    /// </summary>
    /// <param name="physicalObj"></param>
    /// <param name="virtualObj"></param>
    public void StartComparison(GameObject physicalObj, GameObject virtualObj)
    {
        VersionObject versionObj = virtualObj.GetComponentInParent<VersionObject>();

        if (versionObj.virtualTwin)
        {
            Debug.Log("Comparing against virtual twin; ABORT COMPARISON.");
            return;
        }

        // Check for existing comparison, suppress reinitializing the same comparison
        if (inComparison)
        {
            if (virtualObj == comparedAgainstVersionObject.gameObject)
            {
                Debug.Log("This comparison is already active; ABORT COMPARISON.");
                return;
            }

            // Reset if a new comparison is about to start
            StopComparison();
        }

        Debug.Log("A new comparison is initiated. START COMPARISON");

        // Save reference to object for avoiding reinitializing the same comparison
        comparedAgainstVersionObject = versionObj;

        // NOTE: Order matters, first clone the object, then activate the comparison
        comparisonObj.Initialize(versionObj);

        // Highlight in timeline
        //comparedAgainstVersionObject.GetComponentInParent<ObjectParts>().ToggleOutlines(true);
        timelineManager.EnableComparisonLine(virtualTwin.transform, comparedAgainstVersionObject.transform);

        inComparison = true;

        floatingDistance = CalculateFloatingDistance(physicalObj, virtualObj);

        // Fill information panel with content and show
        actionPanel.gameObject.SetActive(true);

        DisplayComparison();
    }

    private float CalculateFloatingDistance(GameObject obj1, GameObject obj2)
    {
        float dist;
        // Calculate floating distance based on object sizes
        // NOTE: this could further be improved by calculating the maximum diaginonal distance
        var coll1 = obj1.GetComponent<Collider>().bounds.size;
        var coll2 = obj2.GetComponent<Collider>().bounds.size;

        float coll1max = Mathf.Max(Mathf.Max(coll1.x, coll1.y), coll1.z);
        float coll2max = Mathf.Max(Mathf.Max(coll2.x, coll2.z), coll2.z);

        dist = (coll1max / 2) + (coll2max / 2) + staticFloatingDistance;

        return dist;
    }

    /// <summary>
    /// Displays a comparison operation based on the currently active comparison mode.
    /// </summary>
    private void DisplayComparison()
    {
        // Reset properties of tracked object and comparison object
        comparisonObj.Reset();
        trackedObj.ResetMaterial();

        // Activate effects based on activated mode
        if (mode == ComparisonMode.SideBySide)
        {
            DestoryDifferenceObjects();
            comparisonObjContainer.SetParent(null);

            comparisonObj.SetSideBySide(floatingDistance);
            comparisonObj.SetOverlayMaterial(true);
        }
        else if (mode == ComparisonMode.Overlay)
        {
            DestoryDifferenceObjects();
            comparisonObj.SetPivotPointBottom();
            comparisonObj.transform.localPosition = Vector3.zero;

            // Parent the comparison object container under the tracked transform to match the physical object
            comparisonObjContainer.SetParent(trackedTransform);
            comparisonObjContainer.localPosition = Vector3.zero;

            // NOTE: the phantom Mat occludes the overlayed mat, short term solution > invisible material
            //trackedObj.SetMaterial(phantomMat);
            trackedObj.SetMaterial(invisibleMat);
            comparisonObj.SetOverlayMaterial();
        }
        else if (mode == ComparisonMode.Differences)
        {
            comparisonObj.SetPivotPointBottom();
            trackedObj.SetMaterial(invisibleMat);

            // Parent the comparison object container under the tracked transform to match the physical object
            comparisonObjContainer.SetParent(trackedTransform);
            comparisonObjContainer.localPosition = Vector3.zero;
            comparisonObj.transform.parent = comparisonObjContainer;

            // Create two more GameObjects to display the differences
            // currently only the compared against is visible, we now need two base objects

            var baseModelContainer = trackedObj.transform.GetChild(0).gameObject;

            var diffBase = Instantiate(baseModelContainer, comparisonObjContainer);
            var diffSub = Instantiate(baseModelContainer, comparisonObjContainer);
            var diffAdded = comparisonObj;

            diffBase.transform.localPosition = Vector3.zero;
            diffSub.transform.localPosition = Vector3.zero;
            diffAdded.transform.localPosition = Vector3.zero;

            diffBase.GetComponentInChildren<Renderer>().material = diffMatBase;
            diffSub.GetComponentInChildren<Renderer>().material = diffMatSubtracted;
            diffAdded.GetComponentInChildren<Renderer>().material = diffMatAdded;
        }

        actionPanel.SetOptions();
    }

    public void DestoryDifferenceObjects()
    {
        foreach (Transform go in comparisonObjContainer)
        {
            if (go.gameObject != comparisonObj.gameObject)
            {
                Destroy(go.gameObject);
            }
        }
    }

    /// <summary>
    /// Resets the status of tracked and comparison object, also resets internal values.
    /// </summary>
    public void StopComparison()
    {
        // Disable highlight on version object
        if (comparedAgainstVersionObject != null)
        {
            //comparedAgainstVersionObject.GetComponentInParent<ObjectParts>().ToggleOutlines(false);
            //versionHistoryObj.GetComponentInParent<VersionObject>().ChangeTextColor(textDefault);
            timelineManager.DisableComparisonLine();
            comparedAgainstVersionObject = null;
        }

        actionPanel.gameObject.SetActive(false);

        comparisonObj.Deactivate();
        trackedObj.ResetMaterial();

        inComparison = false;
    }

    /// <summary>
    /// Cycles through the comparison modes, if a comparison is running, the effect is displayed immediately.
    /// </summary>
    public void SwitchComparisonMode()
    {
        // Cycle through modes
        mode = (ComparisonMode)(((int)mode + 1) % 3);

        Debug.Log("Comparison mode switched to: " + mode);

        if (inComparison)
        {
            DisplayComparison();
        }
    }
    
    /// <summary>
    /// Sets a specific ComparisonMode.
    /// </summary>
    /// <param name="inMode">The mode to activate</param>
    public void SetComparisonMode(int inMode)
    {
        mode = (ComparisonMode)inMode;

        Debug.Log("Comparison mode switched to: " + mode);

        if (inComparison)
        {
            DisplayComparison();
        }
    }

    /// <summary>
    /// Iterate through objects in the timeline to find the virtual twin and set it as reference.
    /// </summary>
    public void FindAndSetVirtualTwin()
    {
        // Find virtual twin
        var timelineObjs = FindObjectsOfType<VersionObject>();
        foreach (var vo in timelineObjs)
        {
            if (vo.virtualTwin)
            {
                virtualTwin = vo;
                break;
            }
        }
    }

    public VersionObject GetVirtualTwin()
    {
        return virtualTwin;
    }
    
    public VersionObject GetComparedAgainstVersionObject()
    {
        return comparedAgainstVersionObject;
    }

    public bool IsInComparison()
    {
        return inComparison;
    }
}
