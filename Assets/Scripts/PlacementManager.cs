using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;

public class PlacementManager : MonoBehaviour
{
    public Vector3 comparisonPanelPositionOffset;

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
    private int objCount;

    private bool inPlacement;

    private void Start()
    {
        objCount = versionHistoryObj.transform.childCount;
        versionObjs = new GameObject[objCount];

        // Fill list of sub objects
        for (int i = 0; i < objCount; i++)
        {
            versionObjs[i] = versionHistoryObj.transform.GetChild(i).gameObject;
        }

        tapToPlace = versionHistoryObj.GetComponent<TapToPlace>();
        SetUp();
    }

    //public void ToggleStatus(bool status)
    //{
    //    if (status)
    //    {
    //        versionHistoryObj.SetActive(status);
    //        tapToPlace.enabled = true;
    //        tapToPlace.StartPlacement();

    //        inPlacement = true;
    //    }
    //    else
    //    {
    //        inPlacement = false;
    //    }
    //}

    private void ToggleMaterials(bool status)
    {
        Material mat = status ? placementMaterial : displayMaterial;

        foreach (var obj in versionObjs)
        {
            obj.GetComponent<MeshRenderer>().material = mat;
        }
    }

    public void PlacementStarts()
    {
        comparisonPanel.SetActive(false);
        versionHistoryObj.SetActive(true);
        tapToPlace.enabled = true;
        tapToPlace.StartPlacement();

        inPlacement = true;

        ToggleMaterials(true);
    }

    public void PlacementFinished()
    {
        ToggleMaterials(false);
        inPlacement = false;

        comparisonPanel.transform.rotation = versionHistoryObj.transform.rotation;
        comparisonPanel.transform.position = versionHistoryObj.transform.position + (versionHistoryObj.transform.rotation * comparisonPanelPositionOffset);
        comparisonPanel.SetActive(true);
    }

    public bool GetInPlacement()
    {
        return inPlacement;
    }

    public void SetUp()
    {
        versionHistoryObj.SetActive(false);
        comparisonPanel.SetActive(false);
        startUpPanel.SetActive(true);

        ToggleMaterials(false);

        inPlacement = false;
    }
}
