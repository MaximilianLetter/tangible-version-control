using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComparisonManager : MonoBehaviour
{
    private bool inComparison;
    private GameObject comparingObj;

    private void Start()
    {
        inComparison = false;
        comparingObj = null;
    }

    public void StartComparison(GameObject trackedObj, GameObject versionObj)
    {
        if (inComparison) return;

        Debug.Log("Comparison started");

        inComparison = true;

        comparingObj = Instantiate(versionObj, trackedObj.transform.parent);
        comparingObj.transform.localPosition = new Vector3(-0.2f, 0, 0);
    }
}
