using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComparisonManager : MonoBehaviour
{
    private bool inComparison;
    private GameObject comparingObj;
    private GameObject originalVersionObj;

    private void Start()
    {
        inComparison = false;
        comparingObj = null;
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
        comparingObj.transform.localPosition = new Vector3(-0.2f, 0, 0);
    }

    public void ResetComparison()
    {
        Destroy(comparingObj);

        comparingObj = null;
        originalVersionObj = null;
        inComparison = false;
    }
}
