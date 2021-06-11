using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class StartupManager : MonoBehaviour
{
    public GameObject startupPanel;

    public GameObject[] sceneObjects;

    private TrackingManager trackingManager;
    private PlacementManager placementManager;
    private ActionPanel actionPanel;
    private ComparisonObject comparisonObject;
    private TrackedObject trackedObject;

    IEnumerator Start()
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

        // Wait for all parts to be initialized
        VersionObject[] vos = FindObjectsOfType<VersionObject>();

        allReady = false;
        while (true)
        {
            allReady = true;
            foreach (var vo in vos)
            {
                if (!vo.IsReady())
                {
                    allReady = false;
                }
            }

            if (allReady) break;

            yield return null;
        }

        // Get necessary references
        placementManager = FindObjectOfType<PlacementManager>();
        trackingManager = FindObjectOfType<TrackingManager>();
        actionPanel = FindObjectOfType<ActionPanel>();
        comparisonObject = FindObjectOfType<ComparisonObject>();
        trackedObject = FindObjectOfType<TrackedObject>();

        // Wait for other objects getting ready
        while (true)
        {
            if (placementManager.IsReady() &&
                (!trackingManager || trackingManager.IsReady()) && // To use in non-tracking scenes
                actionPanel.IsReady() &&
                ComparisonManager.Instance.IsReady() &&
                comparisonObject.IsReady() &&
                trackedObject.IsReady()
                )
            {
                break;
            }
            yield return null;
        }
        Debug.Log("Managers loaded, ready for scene setup.");


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

        startupPanel.SetActive(true);

        Debug.Log("Startup finished.");
    }
}
