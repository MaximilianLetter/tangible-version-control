using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComparisonManager : MonoBehaviour
{
    enum ComparisonMode { SideBySide, Transparent, Differences };

    private bool inComparison;
    private GameObject comparingObj;
    private GameObject originalVersionObj;

    private ComparisonMode mode;

    private void Start()
    {
        inComparison = false;
        comparingObj = null;

        mode = ComparisonMode.SideBySide;
    }

    public void StartComparison(GameObject trackedObj, GameObject versionObj)
    {
        if (inComparison)
        {
            if (versionObj == originalVersionObj) return;

            ResetComparison();
        }

        Debug.Log("Comparison started");

        originalVersionObj = versionObj;
        inComparison = true;

        comparingObj = Instantiate(versionObj, trackedObj.transform.parent);

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
            comparingObj.transform.localPosition = new Vector3(-0.2f, 0, 0);
        }
        else if (mode == ComparisonMode.Transparent)
        {
            comparingObj.transform.localPosition = Vector3.zero;
        }
        else if (mode == ComparisonMode.Differences)
        {
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
        mode = (ComparisonMode)(((int)mode + 1) % 3);

        Debug.Log("Comparison mode switched to: " + mode);

        DisplayComparison();
    }
}
