using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using Vuforia;

public class PlacementManager : MonoBehaviour
{
    public Vector3 comparisonPanelPositionOffset;

    // Objects
    [SerializeField]
    private GameObject floor;
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

    private VersionObject[] versionObjs;

    private bool ready;
    private bool inPlacement;

    private void Start()
    {
        versionObjs = versionHistoryContainer.GetComponentsInChildren<VersionObject>();

        ready = true;
    }

    private void Update()
    {
        if (inPlacement)
        {
            versionHistoryContainer.transform.SetPositionAndRotation(trackedContent.position, trackedContent.rotation);
        }
    }

    public void SetScene()
    {
        ToggleMaterials(false);
        inPlacement = false;
    }

    public bool IsReady()
    {
        return ready;
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
        versionHistoryContainer.SetActive(true);

        ToggleMaterials(true);

#if UNITY_EDITOR
        floor.SetActive(true);
#endif

        inPlacement = true;
    }

    /// <summary>
    /// Finishes the placement process.
    /// </summary>
    public void PlacementFinished()
    {
        ToggleMaterials(false);

#if UNITY_EDITOR
        floor.SetActive(false);
#endif

        // Place the comparison panel according to version history positioning
        //comparisonPanel.transform.rotation = versionHistoryContainer.transform.rotation;
        //comparisonPanel.transform.position = versionHistoryContainer.transform.position + (versionHistoryContainer.transform.rotation * comparisonPanelPositionOffset);
        menuPanel.SetActive(true);
        placementPanel.SetActive(false);

        inPlacement = false;

        // Start vuforia tracking
        var vuforiaTracking = Camera.main.GetComponent<VuforiaBehaviour>();
        if (vuforiaTracking != null) vuforiaTracking.enabled = true;
    }

    public bool GetInPlacement()
    {
        return inPlacement;
    }
}
