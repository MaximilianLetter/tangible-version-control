﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class StartupManager : MonoBehaviour
{
    public GameObject startupPanel;

    public GameObject[] sceneObjects;

    public IEnumerator StartUp()
    {
        // Wait one frame for other objects to instantiate
        yield return null;

        // Wait for all parts to be initialized
        ObjectParts[] parts = FindObjectsOfType<ObjectParts>();

        bool allReady = false;
        while (true)
        {
            allReady = true;
            foreach (var part in parts)
            {
                if (!part.IsReady())
                {
                    allReady = false;
                }
            }

            if (allReady) break;

            yield return null;
        }
        Debug.Log("All object parts initialized.");

        // Wait for all branches to be setup
        Branch[] branches = FindObjectsOfType<Branch>();

        allReady = true;
        while (true)
        {
            foreach (var b in branches)
            {
                if (!b.IsReady())
                {
                    allReady = false;
                }
            }

            if (allReady) break;
            yield return null;
        }
        Debug.Log("All branches setup.");

        // NOTE: this is not clean, this should rather be a callback of Vuforia setup, should then be placed in the TrackingManager
#if UNITY_EDITOR
        if (Camera.main.GetComponent<VuforiaBehaviour>() != null) // To use in non-tracking scenes
        {
            // Make sure Vuforia is fully instantiated to disable Positionial Device Tracking
            while (true)
            {
                if (VuforiaRuntime.Instance.InitializationState == VuforiaRuntime.InitState.INITIALIZED)
                {
                    break;
                }
                yield return null;
            }

            var pdt = TrackerManager.Instance.GetTracker<PositionalDeviceTracker>();
            pdt.Stop();
        }
#endif

        // Setup the scene
        foreach (var obj in sceneObjects)
        {
            obj.SetActive(false);
        }

        AppManager.Instance.GetTimelineManager().BuildTimeline();
        startupPanel.SetActive(true);

        Debug.Log("Startup finished.");
    }
}
