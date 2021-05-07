using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusTrackedObject : MonoBehaviour
{
    private Transform trackedObj;

    void Start()
    {
        trackedObj = GameObject.Find("TrackedContainer").transform;
    }

    private void Update()
    {
        transform.LookAt(trackedObj);
    }
}
