using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities;

public class TransitionManager : MonoBehaviour
{
    public GameObject startBtn;
    public GameObject transitionUIContainer;

    private bool inTransition;
    private ComparisonManager comparisonManager;
    private TimelineManager timelineManager;

    private TrackedObject physObj;
    private ObjectParts physObjParts;

    private void Start()
    {
        comparisonManager = AppManager.Instance.GetComparisonManager();
        timelineManager = AppManager.Instance.GetTimelineManager();

        physObj = AppManager.Instance.GetTrackedObjectLogic();
        physObjParts = physObj.GetComponent<ObjectParts>();

        ResetTransitionUI();
    }

    /// <summary>
    /// Start a transition by changing to Differences mode and updating the UI.
    /// </summary>
    public void StartTransition()
    {
        if (!comparisonManager.IsInComparison() || inTransition) return;

        // Make sure the Differences mode is activated
        comparisonManager.SetComparisonMode((int)ComparisonMode.Differences);

        startBtn.SetActive(false);
        transitionUIContainer.SetActive(true);

        inTransition = true;
    }

    /// <summary>
    /// Execute the transition from one version to another.
    /// </summary>
    public void CompleteTransition()
    {
        if (!comparisonManager.IsInComparison() || !inTransition) return;

        var currentVirtualTwin = comparisonManager.GetVirtualTwin();
        if (currentVirtualTwin == null) return;

        var newVirtualTwin = comparisonManager.GetVersionHistoryObject();
        if (newVirtualTwin == null) return;

        // Destroy current, no longer needed parts
        foreach (Transform oldPart in physObj.transform)
        {
            Debug.Log(oldPart.name + " destroyed from: " + oldPart.parent.name);
            //oldPart.gameObject.SetActive(false);
            Destroy(oldPart.gameObject);
            // NOTE: this somehow destroys parts of the version objets
            // TODO
        }

        // Clone all parts from the new virtual twin to the physical object
        var parts = new GameObject[newVirtualTwin.transform.childCount];
        for (int i = 0; i < newVirtualTwin.transform.childCount; i++)
        {
            // Clone each part of the object, remove the MeshOutline Script
            GameObject original = newVirtualTwin.transform.GetChild(i).gameObject;
            GameObject part = Instantiate(original, physObj.transform);
            var outL = part.GetComponent<MeshOutline>();
            if (outL != null)
            {
                Material[] mats = new Material[1] { part.GetComponent<PreserveMaterial>().GetBaseMat() };
                part.GetComponent<MeshRenderer>().materials = mats;
            }

            // Make sure the real name of the part is kept for part-wise comparisons
            part.name = original.name;

            // Store necessary information about parts
            parts[i] = part;
        }

        // Update new parts and new collider
        // Style physical object representation to accordance
        if (comparisonManager.usePhysical)
        {
            // Set and override base material as phantom
            physObj.SetMaterial(comparisonManager.phantomMat);
            physObjParts.CollectRenderersAndMaterials(parts);
        } else
        {
            physObjParts.CollectRenderersAndMaterials(parts);
        }

        // Actual transition
        currentVirtualTwin.virtualTwin = false;
        newVirtualTwin.GetComponentInParent<VersionObject>().virtualTwin = true;

        // Update elements to reflect the changes
        // NOTE: order matters
        AppManager.Instance.FindAndSetVirtualTwin(true);
        comparisonManager.StopComparison();
        comparisonManager.FindAndSetVirtualTwin(); // todo remove
        timelineManager.UpdateTimeline(); //TODO remove

        ResetTransitionUI();
        inTransition = false;
    }

    /// <summary>
    /// Cancels a started transition by resetting UI and variables.
    /// </summary>
    public void CancelTransition()
    {
        if (!inTransition) return;

        ResetTransitionUI();
        inTransition = false;
    }

    /// <summary>
    /// Resets the UI to default.
    /// </summary>
    private void ResetTransitionUI()
    {
        // Reset transition-UI
        startBtn.SetActive(true);
        transitionUIContainer.SetActive(false);
    }

    public bool IsInTransition()
    {
        return inTransition;
    }
}
