using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using Vuforia;

public class PlacementManager : MonoBehaviour
{
    public Vector3 comparisonPanelPositionOffset;

    [SerializeField]
    private GameObject floor;
    [SerializeField]
    private GameObject versionHistoryContainer;
    [SerializeField]
    private GameObject comparisonPanel;
    [SerializeField]
    private GameObject startUpPanel;
    [SerializeField]
    private Material placementMaterial;

    private TapToPlace tapToPlace;
    private VersionObject[] versionObjs;

    private bool ready;
    private bool inPlacement;

    private void Start()
    {
        versionObjs = versionHistoryContainer.GetComponentsInChildren<VersionObject>();

        tapToPlace = versionHistoryContainer.GetComponent<TapToPlace>();

        ready = true;
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
        // Activate necessary objects and scripts
        comparisonPanel.SetActive(false);
        versionHistoryContainer.SetActive(true);
        tapToPlace.enabled = true;
        tapToPlace.StartPlacement();
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
        // Stop the ability to move the version history
        tapToPlace.enabled = false;
        ToggleMaterials(false);

#if UNITY_EDITOR
        floor.SetActive(false);
#endif

        // Place the comparison panel according to version history positioning
        comparisonPanel.transform.rotation = versionHistoryContainer.transform.rotation;
        comparisonPanel.transform.position = versionHistoryContainer.transform.position + (versionHistoryContainer.transform.rotation * comparisonPanelPositionOffset);
        comparisonPanel.SetActive(true);

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
