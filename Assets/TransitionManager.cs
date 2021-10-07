using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionManager : MonoBehaviour
{
    public GameObject startBtn;
    public GameObject transitionUIContainer;

    private bool inTransition;
    private ComparisonManager comparisonManager;
    private ConnectPhysicalObjectToTimeline connectionLine;
    private PlacementManager placementManager;

    private void Start()
    {
        comparisonManager = FindObjectOfType<ComparisonManager>();
        connectionLine = FindObjectOfType<ConnectPhysicalObjectToTimeline>();
        placementManager = FindObjectOfType<PlacementManager>();

        ResetTransitionUI();
    }

    /// <summary>
    /// 
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
    /// 
    /// </summary>
    public void CompleteTransition()
    {
        if (!comparisonManager.IsInComparison() || !inTransition) return;

        var currentVirtualTwin = comparisonManager.GetVirtualTwin();
        if (currentVirtualTwin == null) return;

        var newVirtualTwin = comparisonManager.GetVersionHistoryObject();
        if (newVirtualTwin == null) return;


        // Actual transition
        currentVirtualTwin.virtualTwin = false;
        newVirtualTwin.GetComponentInParent<VersionObject>().virtualTwin = true;

        // update physicalobject parts, update partmanagement

        // Update elements to reflect the changes
        // NOTE: order matters
        connectionLine.FindAndSetVirtualTwin();
        comparisonManager.StopComparison();
        comparisonManager.FindAndSetVirtualTwin();
        placementManager.UpdateTimeline();

        ResetTransitionUI();
        inTransition = false;
    }

    /// <summary>
    /// 
    /// </summary>
    public void CancelTransition()
    {
        if (!inTransition) return;

        ResetTransitionUI();
        inTransition = false;
    }

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
