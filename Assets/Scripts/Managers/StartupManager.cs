﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class StartupManager : MonoBehaviour
{
    public GameObject[] sceneObjects;
    public GameObject markerHint;
    public GameObject taskPanel;

    public IEnumerator StartUp(bool firstTime = false)
    {
        // Wait one frame for other objects to instantiate
        yield return null;

        // Setup the scene
        foreach (var obj in sceneObjects)
        {
            obj.SetActive(false);
        }

        // First of wait for the ApiManager to finish
        var apiManager = AppManager.Instance.GetApiManager();
        var experimentManager = AppManager.Instance.GetExperimentManager();
        while (true)
        {
            if (apiManager != null)
            {
                if (apiManager.IsReady()) break;
            }
            if (experimentManager != null)
            {
                if (experimentManager.IsReady()) break;
            }
            
            yield return null;
        }
        Debug.Log("API MANAGER READY _ START OTHER STUFF");

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
        AppManager.Instance.GetTimelineManager().Initialize();
        AppManager.Instance.FindAndSetVirtualTwin(true);
        AppManager.Instance.GetTrackedObjectLogic().Initialize();

        AppManager.Instance.GetComparisonManager().Initialize();

        if (!AppManager.Instance.experiment)
        {
            AppManager.Instance.GetTrackedObjectLogic().GetComponent<TiltToMove>().Initialize();
        }

        //yield return new WaitForSeconds(0.1f);

        AppManager.Instance.GetTimelineManager().StartPlacement();

        if (AppManager.Instance.experiment)
        {
            if (!firstTime)
            {
                // Disable replacement
                taskPanel.gameObject.SetActive(true);
                AppManager.Instance.GetTimelineManager().FinishPlacement();
            }
            //AppManager.Instance.GetTimelineManager().DisableReplacement();
            AppManager.Instance.GetTimelineManager().ToggleDummyModels(true);
        }

#if UNITY_EDITOR
        if (AppManager.Instance.GetComparisonManager().usePhysical)
        {
            yield return new WaitForSeconds(0.5f);
            if (firstTime)
            {
                markerHint.SetActive(true);
            }
        }
#else
        yield return new WaitForSeconds(0.5f);
        if (firstTime) 
        {
            markerHint.SetActive(true);
        }
#endif

        //if (noPlacement)
        //{
        //    Debug.Log("EXPERIMENT MODE");
        //    taskPanel.SetActive(true);
        //    AppManager.Instance.GetTimelineContainer().SetActive(false);
        //}

        Debug.Log("Startup finished.");
    }
}
