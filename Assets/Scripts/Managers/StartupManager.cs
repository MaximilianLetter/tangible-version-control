using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class StartupManager : MonoBehaviour
{
    public GameObject[] sceneObjects;
    public GameObject markerHint;

    public IEnumerator StartUp()
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
        while (true)
        {
            if (apiManager.IsReady()) break;
            
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


        AppManager.Instance.GetTimelineManager().StartPlacement();
#if UNITY_EDITOR
        if (AppManager.Instance.GetComparisonManager().usePhysical)
        {
            markerHint.SetActive(true);
        }
#else
        markerHint.SetActive(true);
#endif

        Debug.Log("Startup finished.");
    }
}
