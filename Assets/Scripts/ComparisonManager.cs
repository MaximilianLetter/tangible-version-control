using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ComparisonMode { SideBySide, Transparent, Differences }

public class ComparisonManager : MonoBehaviour
{

    private bool inComparison;
    private GameObject comparingObj;
    private ComparisonObject comparingObjLogic;
    private GameObject originalVersionObj;
    private GameObject trackedObj;

    public ComparisonMode mode;

    private void Start()
    {
        trackedObj = GameObject.Find("TrackedContainer");

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
        if (mode == ComparisonMode.SideBySide)
        {
            comparingObj.transform.parent = null;
            comparingObjLogic.hoverNext = true;
        }
        else if (mode == ComparisonMode.Transparent)
        {
            comparingObj.transform.parent = trackedObj.transform;
            comparingObj.transform.localPosition = Vector3.zero;
        }
        else if (mode == ComparisonMode.Differences)
        {
            comparingObj.transform.parent = trackedObj.transform;
            comparingObj.transform.localPosition = Vector3.zero;
        }
    }

    public void ResetComparison()
    {
        Destroy(comparingObj);

        comparingObj = null;
        originalVersionObj = null;
        inComparison = false;
    }

    public void SwitchComparisonMode()
    {
        // Reset properties if necessary
        if (comparingObjLogic != null)
        {
            comparingObjLogic.hoverNext = false;
        }

        // Cycle through modes
        mode = (ComparisonMode)(((int)mode + 1) % 3);

        Debug.Log("Comparison mode switched to: " + mode);

        DisplayComparison();
    }
}
