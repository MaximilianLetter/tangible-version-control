using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ComparisonMode { SideBySide, Overlay, Differences }

public class ComparisonManager : MonoBehaviour
{
    // Comparison properties
    public Material transparentMat;
    public Material wireframesMat;
    public Material phantomMat;

    // Required objects
    private bool inComparison;
    private GameObject comparingObj;
    private ComparisonObject comparingObjLogic;
    private GameObject originalVersionObj;
    private GameObject trackedObj;
    private TrackedObject trackedObjLogic;

    public ComparisonMode mode;

    private void Start()
    {
        trackedObj = GameObject.Find("TrackedContainer");
        trackedObjLogic = trackedObj.transform.GetChild(0).GetComponent<TrackedObject>();

        inComparison = false;
        comparingObj = null;

        mode = ComparisonMode.SideBySide;
    }

    public void StartComparison(GameObject physicalObj, GameObject versionObj)
    {
        if (inComparison)
        {
            if (versionObj == originalVersionObj) return;

            ResetComparison();
        }

        Debug.Log("Comparison started");

        originalVersionObj = versionObj;
        inComparison = true;

        comparingObj = Instantiate(versionObj, physicalObj.transform.parent);
        comparingObjLogic = comparingObj.AddComponent<ComparisonObject>();

        // Disable collider so it does not collide with the comparing obj
        var coll = comparingObj.GetComponent<Collider>();
        if (coll != null)
        {
            coll.enabled = false;
        }

        DisplayComparison();
    }

    private void DisplayComparison()
    {        
        // Reset properties if necessary
        if (comparingObjLogic == null || trackedObjLogic == null)
        {
            Debug.Log("Scripts of comparingObj or trackedObj missing");
            return;
        }

        comparingObjLogic.Reset();
        trackedObjLogic.ResetMaterial();

        // Activate effects based on activated mode
        if (mode == ComparisonMode.SideBySide)
        {
            comparingObj.transform.parent = null;
            comparingObjLogic.hoverNext = true;
        }
        else if (mode == ComparisonMode.Overlay)
        {
            comparingObj.transform.parent = trackedObj.transform;
            comparingObj.transform.localPosition = Vector3.zero;

            trackedObjLogic.SetMaterial(phantomMat);
            comparingObjLogic.SetOverlayMaterial(transparentMat);
        }
        else if (mode == ComparisonMode.Differences)
        {
            comparingObj.transform.parent = trackedObj.transform;
            comparingObj.transform.localPosition = Vector3.zero;

            // For testing purposes
            trackedObjLogic.SetMaterial(phantomMat);
            comparingObjLogic.SetOverlayMaterial(wireframesMat);
        }
    }

    public void ResetComparison()
    {
        Destroy(comparingObj);

        comparingObj = null;
        originalVersionObj = null;
        inComparison = false;
        trackedObjLogic.ResetMaterial();
    }

    public void SwitchComparisonMode()
    {
        // Cycle through modes
        mode = (ComparisonMode)(((int)mode + 1) % 3);

        Debug.Log("Comparison mode switched to: " + mode);

        DisplayComparison();
    }
}
