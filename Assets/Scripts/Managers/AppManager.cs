using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    public bool experiment;

    // References to all required objects
    private TrackedObject       trackedObjectLogic;
    private Transform           trackedTransform;

    private ComparisonObject    comparisonObjectLogic;
    //private ObjectParts         differencesObjectLogic;

    private GameObject          timelineContainer;
    private Transform           branchContainer;
    private Transform           contentContainer;
    private VersionObject       virtualTwin;
    private ConnectionLine      connectionLine;
    private LineRenderer        comparisonLine;
    private ActionPanel         actionPanel;
    private TaskPanel           taskPanel;

    // References to all managers
    private TimelineManager     timelineManager;
    private ComparisonManager   comparisonManager;
    private TransitionManager   transitionManager;
    private StartupManager      startupManager;
    private GitHubAPIManager    apiManager;
    private ExperimentManager   experimentManager;

    private void Awake()
    {
        _Instance = this;

        // Find managers
        timelineManager = GameObject.FindObjectOfType<TimelineManager>();
        comparisonManager = GameObject.FindObjectOfType<ComparisonManager>();
        transitionManager = GameObject.FindObjectOfType<TransitionManager>();
        startupManager = GameObject.FindObjectOfType<StartupManager>();
        apiManager = GameObject.FindObjectOfType<GitHubAPIManager>();
        experimentManager = FindObjectOfType<ExperimentManager>();

        // Find all required objects
        trackedObjectLogic = GameObject.FindObjectOfType<TrackedObject>();
        trackedTransform = trackedObjectLogic.transform.parent.parent; // could be alternatively found as MultiTargetBehaviour or similar

        comparisonObjectLogic = GameObject.FindObjectOfType<ComparisonObject>();
        //differencesObjectLogic = trackedObjectLogic.transform.parent.Find("DifferencesObject").GetComponent<ObjectParts>();

        contentContainer = GameObject.Find("MixedRealitySceneContent").transform;
        timelineContainer = GameObject.Find("Timeline");
        branchContainer = timelineContainer.transform.Find("BranchContainer");
        connectionLine = FindObjectOfType<ConnectionLine>();
        comparisonLine = GameObject.Find("ComparisonLine").GetComponent<LineRenderer>();
        actionPanel = FindObjectOfType<ActionPanel>();
        taskPanel = FindObjectOfType<TaskPanel>();
    }

    private void Start()
    {
        if (ReadyVerification())
        {
            Debug.Log("The AppManager is ready.");
            if (experiment)
            {
                // experiment Manager will call StartupManager by itself
                experimentManager.SetupExperiment(true);
            }
            else
            {
                startupManager.StartCoroutine("StartUp", true);
            }
        } else
        {
            Debug.Log("The AppManager was not able to find all required components.");
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

    public ExperimentManager GetExperimentManager()
    {
        return experimentManager;
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

    public GameObject GetTimelineContainer()
    {
        return timelineContainer;
    }

    public Transform GetBranchContainer()
    {
        return branchContainer;
    }

    //public ObjectParts GetDifferencesObjectLogic()
    //{
    //    return differencesObjectLogic;
    //}

    public GitHubAPIManager GetApiManager()
    {
        return apiManager;
    }

    public ConnectionLine GetConnectionLine()
    {
        return connectionLine;
    }
    
    public LineRenderer GetComparisonLine()
    {
        return comparisonLine;
    }

    public ActionPanel GetActionPanel()
    {
        return actionPanel;
    }
    public TaskPanel GetTaskPanel()
    {
        return taskPanel;
    }

    public Transform GetContentContainer()
    {
        return contentContainer;
    }
    #endregion

    /// <summary>
    /// Iterate through objects in the timeline to find the virtual twin and set it as reference.
    /// </summary>
    public VersionObject FindAndSetVirtualTwin(bool setEverywhere = false)
    {
        // Find virtual twin
        VersionObject[] timelineObjs = timelineContainer.transform.GetComponentsInChildren<VersionObject>();
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

        Debug.Log("No object in the timeline is set as virtual twin.");
        return null;
    }

    private bool ReadyVerification()
    {
        if (GetTrackedObjectLogic() == null)
        {
            Debug.Log("TrackedObjectLogic not found");
            return false;
        }

        if (GetTrackedTransform() == null)
        {
            Debug.Log("TrackedTransform not found");
            return false;
        }

        if (GetComparisonObjectLogic() == null)
        {
            Debug.Log("ComparisonObjectLogic not found");
            return false;
        }

        //if (GetDifferencesObjectLogic() == null)
        //{
        //    Debug.Log("DifferencesObjectLogic not found");
        //    return false;
        //}

        if (GetTimelineContainer() == null)
        {
            Debug.Log("TimelineContainer not found");
            return false;
        }

        if (GetBranchContainer() == null)
        {
            Debug.Log("BranchContainer not found");
            return false;
        }

        //if (GetVirtualTwin() == null)
        //{
        //    Debug.Log("VirtualTwin not found");
        //    return false;
        //}

        if (GetTimelineManager() == null)
        {
            Debug.Log("TimelineManager not found");
            return false;
        }

        if (GetComparisonManager() == null)
        {
            Debug.Log("ComparisonManager not found");
            return false;
        }

        if (GetStartupManager() == null)
        {
            Debug.Log("StartupManager not found");
            return false;
        }

        // Not mandatory
        //if (GetTransitionManager() == null)
        //{
        //    Debug.Log("TransitionManager not found");
        //    //return false;
        //}

        // Not mandatory
        //if (GetApiManager() == null)
        //{
        //    Debug.Log("ApiManager not found");
        //    //return false;
        //}

        // Not mandatory
        if (GetExperimentManager() == null)
        {
            Debug.Log("ExperimentManager not found");
            //return false;
        }

        if (GetConnectionLine() == null)
        {
            Debug.Log("Connection line not found");
            return false;
        }

        if (GetComparisonLine() == null)
        {
            Debug.Log("Comparison line not found");
            return false;
        }

        if (GetActionPanel() == null)
        {
            Debug.Log("Action panel not found");
            return false;
        }

        // Not mandatory
        if (GetTaskPanel() == null)
        {
            Debug.Log("Task panel not found");
            //return false;
        }

        if (GetContentContainer() == null)
        {
            Debug.Log("Content container not found");
            return false;
        }

        return true;
    }

    public void LoadSceneSelection()
    {
        SceneManager.LoadScene("_SelectionScene");
    }
    
    public void LoadExperimentScene()
    {
        SceneManager.LoadScene("_ExperimentScene");
    }
}
