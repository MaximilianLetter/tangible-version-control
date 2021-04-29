using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;

public class PlacementManager : MonoBehaviour
{
    public Vector3 comparisonPanelPositionOffset;

    [SerializeField]
    private GameObject floor;
    [SerializeField]
    private GameObject versionHistoryObj;
    [SerializeField]
    private GameObject comparisonPanel;
    [SerializeField]
    private GameObject startUpPanel;
    [SerializeField]
    private Material displayMaterial;
    [SerializeField]
    private Material placementMaterial;

    private TapToPlace tapToPlace;
    private GameObject[] versionObjs;

    private bool ready;
    private bool inPlacement;

    private void Start()
    {
        int objCount = versionHistoryObj.transform.childCount;
        versionObjs = new GameObject[objCount];

        // Fill list of sub objects
        for (int i = 0; i < objCount; i++)
        {
            versionObjs[i] = versionHistoryObj.transform.GetChild(i).gameObject;
        }

        tapToPlace = versionHistoryObj.GetComponent<TapToPlace>();

        // Setup internal values
        ToggleMaterials(false);
        inPlacement = false;

        ready = true;
    }

    public bool IsReady()
    {
        return ready;
    }

    /// <summary>
    /// Toggle between transparent material during positioning and opaque material after the placement finished.
    /// </summary>
    /// <param name="status">True equals the placement material, false equals the normal display material.</param>
    private void ToggleMaterials(bool status)
    {
        Material mat = status ? placementMaterial : displayMaterial;

        foreach (var obj in versionObjs)
        {
            obj.GetComponent<MeshRenderer>().material = mat;
        }
    }

    /// <summary>
    /// Starts the placement process.
    /// </summary>
    public void PlacementStarts()
    {
        // Activate necessary objects and scripts
        comparisonPanel.SetActive(false);
        versionHistoryObj.SetActive(true);
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
        comparisonPanel.transform.rotation = versionHistoryObj.transform.rotation;
        comparisonPanel.transform.position = versionHistoryObj.transform.position + (versionHistoryObj.transform.rotation * comparisonPanelPositionOffset);
        comparisonPanel.SetActive(true);

        inPlacement = false;
    }

    public bool GetInPlacement()
    {
        return inPlacement;
    }
}
