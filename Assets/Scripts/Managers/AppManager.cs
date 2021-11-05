using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppManager : MonoBehaviour
{
    protected static AppManager _Instance;

    public static AppManager Instance
    {
        get
        {
            if (_Instance == null)
            {
                _Instance = new AppManager();
            }
            return _Instance;
        }
    }

    protected AppManager() { }

    // References to all required objects
    private TrackedObject       trackedObjectLogic;
    private Transform           trackedTransform;

    private ComparisonObject    comparisonObjectLogic;
    private ObjectParts         differencesObjectLogic;

    private GameObject          timelineObject;
    private VersionObject       virtualTwin;

    // References to all managers
    private TimelineManager     timelineManager;
    private ComparisonManager   comparisonManager;
    private TransitionManager   transitionManager;
    private StartupManager      startupManager;

    private void Awake()
    {
        _Instance = this;

        // Find managers
        timelineManager = GameObject.FindObjectOfType<TimelineManager>();
        comparisonManager = GameObject.FindObjectOfType<ComparisonManager>();
        transitionManager = GameObject.FindObjectOfType<TransitionManager>();
        startupManager = GameObject.FindObjectOfType<StartupManager>();

        // Find all required objects
        trackedObjectLogic = GameObject.FindObjectOfType<TrackedObject>();
        trackedTransform = trackedObjectLogic.transform.parent;

        comparisonObjectLogic = GameObject.FindObjectOfType<ComparisonObject>();
        differencesObjectLogic = trackedObjectLogic.transform.parent.Find("DifferencesObject").GetComponent<ObjectParts>();

        comparisonObjectLogic = GameObject.FindObjectOfType<ComparisonObject>();
        timelineObject = GameObject.Find("Timeline");
        FindAndSetVirtualTwin();
    }

    private void Start()
    {
        if (ReadyVerification())
        {
            Debug.Log("The AppManager is ready.");
            startupManager.StartCoroutine("StartUp");
        } else
        {
            Debug.LogError("The AppManager was not able to find all required components.");
        }
    }

    #region Getter Functions
    public TimelineManager GetTimelineManager()
    {
        return timelineManager;
    }

    public ComparisonManager GetComparisonManager()
    {
        return comparisonManager;
    }

    public TransitionManager GetTransitionManager()
    {
        return transitionManager;
    }

    public StartupManager GetStartupManager()
    {
        return startupManager;
    }

    public TrackedObject GetTrackedObjectLogic()
    {
        return trackedObjectLogic;
    }

    public Transform GetTrackedTransform()
    {
        return trackedTransform;
    }

    public ComparisonObject GetComparisonObjectLogic()
    {
        return comparisonObjectLogic;
    }

    public VersionObject GetVirtualTwin()
    {
        return virtualTwin;
    }

    public GameObject GetTimelineObject()
    {
        return timelineObject;
    }

    public ObjectParts GetDifferencesObjectLogic()
    {
        return differencesObjectLogic;
    }
    #endregion

    /// <summary>
    /// Iterate through objects in the timeline to find the virtual twin and set it as reference.
    /// </summary>
    public VersionObject FindAndSetVirtualTwin(bool setEverywhere = false)
    {
        // Find virtual twin
        VersionObject[] timelineObjs = timelineObject.transform.GetComponentsInChildren<VersionObject>();
        foreach (var vo in timelineObjs)
        {
            if (vo.virtualTwin)
            {
                virtualTwin = vo;

                if (setEverywhere)
                {
                    timelineManager.SetVirtualTwinReference(vo);
                    timelineManager.UpdateTimeline();
                }

                return vo;
            }
        }

        Debug.LogError("No object in the timeline is set as virtual twin.");
        return null;
    }

    private bool ReadyVerification()
    {
        if (GetTrackedObjectLogic() == null)
        {
            Debug.LogError("TrackedObjectLogic not found");
            return false;
        }

        if (GetTrackedTransform() == null)
        {
            Debug.LogError("TrackedTransform not found");
            return false;
        }

        if (GetComparisonObjectLogic() == null)
        {
            Debug.LogError("ComparisonObjectLogic not found");
            return false;
        }

        if (GetDifferencesObjectLogic() == null)
        {
            Debug.LogError("DifferencesObjectLogic not found");
            return false;
        }

        if (GetTimelineObject() == null)
        {
            Debug.LogError("TimelineObject not found");
            return false;
        }

        if (GetVirtualTwin() == null)
        {
            Debug.LogError("VirtualTwin not found");
            return false;
        }

        if (GetTimelineManager() == null)
        {
            Debug.LogError("TimelineManager not found");
            return false;
        }

        if (GetComparisonManager() == null)
        {
            Debug.LogError("ComparisonManager not found");
            return false;
        }

        if (GetStartupManager() == null)
        {
            Debug.LogError("StartupManager not found");
            return false;
        }

        if (GetTransitionManager() == null)
        {
            Debug.LogError("TransitionManager not found");
            return false;
        }

        return true;
    }
}
