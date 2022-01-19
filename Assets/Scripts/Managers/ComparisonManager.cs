using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ComparisonMode { SideBySide, Overlay, Differences }

public class ComparisonManager : MonoBehaviour
{
    // Use tracking and physical objects
    public bool usePhysical;
    public float staticFloatingDistance;
    public float pulseCadence;
    public float pulseHold;
    [Range(0f, 1f)]
    public float pulseAlphaLimit = 0.5f;

    [Header("Object Materials")]
    public Material phantomMat;
    public Material invisibleMat;
    public Material edgesMat;

    [Space(10)]
    public Material diffMatBase;
    public Material diffMatAdded;
    public Material diffMatSubtracted;

    [Space(10)]
    public ComparisonMode mode = ComparisonMode.SideBySide;

    // Required object references
    private ActionPanel actionPanel;
    private VersionObject virtualTwin;
    private TimelineManager timelineManager;

    // Transforms ordered by hierarchy
    private Transform contentContainer;
    private Transform trackedObjContainer;
    private TrackedObject trackedObj;
    private Transform comparisonObjContainer;
    private ComparisonObject comparisonObj;

    // State variables
    private VersionObject comparedAgainstVersionObject;
    private GameObject mainComparisonModel;
    private float floatingDistance;
    private bool inComparison;

    /// <summary>
    /// Initialize this manager, similar to a start function. This is required to get the virtual twin from the AppManager.
    /// </summary>
    public void Initialize()
    {
        // Get relevant gameobject logic
        virtualTwin = AppManager.Instance.GetVirtualTwin();
        actionPanel = AppManager.Instance.GetActionPanel();
        timelineManager = AppManager.Instance.GetTimelineManager();
        comparisonObj = AppManager.Instance.GetComparisonObjectLogic();
        comparisonObjContainer = comparisonObj.transform;

        // Get relevant transform information
        contentContainer = AppManager.Instance.GetContentContainer();
        trackedObj = AppManager.Instance.GetTrackedObjectLogic();
        trackedObjContainer = trackedObj.transform.parent;

        inComparison = false;

#if UNITY_EDITOR
        // A bigger floating distance is required on HoloLens
        staticFloatingDistance = 0.045f;
#endif
        StartCoroutine(PulseMaterials());
    }

    /// <summary>
    /// Starts a comparison between the tracked physical object and the collided object of the version history.
    /// </summary>
    /// <param name="physicalObj"></param>
    /// <param name="virtualObj"></param>
    public void StartComparison(GameObject physicalObj, GameObject virtualObj)
    {
        VersionObject versionObj = virtualObj.GetComponentInParent<VersionObject>();

        if (versionObj.virtualTwin)
        {
            Debug.Log("Comparing against virtual twin; ABORT COMPARISON.");
            return;
        }

        // Check for existing comparison, suppress reinitializing the same comparison
        if (inComparison)
        {
            if (virtualObj == comparedAgainstVersionObject.gameObject)
            {
                Debug.Log("This comparison is already active; ABORT COMPARISON.");
                return;
            }

            // Reset if a new comparison is about to start
            StopComparison();
        }

        Debug.Log("A new comparison is initiated. START COMPARISON");

        // Save reference to object for avoiding reinitializing the same comparison
        comparedAgainstVersionObject = versionObj;

        // Save the main model that is used by the comparison object
        mainComparisonModel = comparisonObj.Initialize(versionObj);

        // Highlight in timeline
        timelineManager.EnableComparisonLine(virtualTwin.transform, comparedAgainstVersionObject.transform);

        inComparison = true;
        floatingDistance = CalculateFloatingDistance(physicalObj, virtualObj);

        // Show panel
        actionPanel.gameObject.SetActive(true);

        DisplayComparison();
    }

    /// <summary>
    /// Displays a comparison operation based on the currently active comparison mode.
    /// </summary>
    private void DisplayComparison()
    {
        // Reset properties of tracked object and comparison object
        CleanUpComparison();

        // Activate effects based on activated mode
        if (mode == ComparisonMode.SideBySide)
        {
            trackedObj.ResetMaterial();
            comparisonObjContainer.SetParent(contentContainer);
            comparisonObj.SetSideBySide(true, floatingDistance);
        }
        else if (mode == ComparisonMode.Overlay)
        {
            comparisonObj.SetPivotPointBottom();

            // Parent the comparison object container under the tracked transform to match the physical object
            comparisonObjContainer.SetParent(trackedObjContainer);
            comparisonObjContainer.localPosition = Vector3.zero;

            trackedObj.SetMaterial(invisibleMat);
        }
        else if (mode == ComparisonMode.Differences)
        {
            comparisonObj.SetPivotPointBottom();
            trackedObj.SetMaterial(invisibleMat);

            // Parent the comparison object container under the tracked transform to match the physical object
            comparisonObjContainer.SetParent(trackedObjContainer);
            comparisonObjContainer.localPosition = Vector3.zero;
            comparisonObjContainer.localRotation = Quaternion.identity;

            // Create two more GameObjects to display the differences
            // currently only the compared against is visible, two base objects need to be instantiated
            var baseModelContainer = trackedObj.transform.GetChild(0).gameObject;

            var diffBase = Instantiate(baseModelContainer, comparisonObjContainer);
            var diffSub = Instantiate(baseModelContainer, comparisonObjContainer);
            var diffAdded = mainComparisonModel;

            // Clean up copied components, except the already existing comparison object
            for (int i = 1; i < comparisonObjContainer.childCount; i++)
            {
                var coll = comparisonObjContainer.GetChild(i).GetComponent<BoxCollider>();
                if (coll) Destroy(coll);

                var collScript = comparisonObjContainer.GetChild(i).GetComponent<CollisionInteraction>();
                if (collScript) Destroy(collScript);
            }

            diffBase.transform.localPosition = baseModelContainer.transform.localPosition;
            diffSub.transform.localPosition = baseModelContainer.transform.localPosition;
            diffAdded.transform.localPosition = baseModelContainer.transform.localPosition;

            diffBase.GetComponentInChildren<Renderer>().material = diffMatBase;
            diffSub.GetComponentInChildren<Renderer>().material = diffMatSubtracted;
            diffAdded.GetComponentInChildren<Renderer>().material = diffMatAdded;
        }

        actionPanel.SetOptions();
    }

    /// <summary>
    /// Resets the status of tracked and comparison object, also resets internal values.
    /// </summary>
    /// <param name="softReset">Models are not destroyed and values are not fully reset.</param>
    public void StopComparison()
    {
        // Disable highlight on version object
        if (comparedAgainstVersionObject != null)
        {
            timelineManager.DisableComparisonLine();
            comparedAgainstVersionObject = null;
        }

        actionPanel.gameObject.SetActive(false);

        trackedObj.ResetMaterial();

        // Destroy all objects
        foreach (Transform child in comparisonObjContainer)
        {
            Destroy(child.gameObject);
        }
        comparisonObj.gameObject.SetActive(false);

        inComparison = false;
    }

    /// <summary>
    /// Cleans up the comparison object so that it can be repopulated.
    /// </summary>
    void CleanUpComparison()
    {
        int modelsInUse = comparisonObjContainer.childCount;

        if (modelsInUse > 1)
        {
            for (int i = 0; i < comparisonObjContainer.childCount; i++)
            {
                if (comparisonObjContainer.GetChild(i).gameObject == mainComparisonModel)
                {
                    comparisonObj.ReInitialize(mainComparisonModel);
                    continue;
                }

                Destroy(comparisonObjContainer.GetChild(i).gameObject);
            }
        }

        comparisonObj.ResetMaterial();
        comparisonObj.SetSideBySide(false, floatingDistance);
    }

    /// <summary>
    /// Cycles through the comparison modes, if a comparison is running, the effect is displayed immediately.
    /// </summary>
    public void SwitchComparisonMode()
    {
        // Cycle through modes
        mode = (ComparisonMode)(((int)mode + 1) % 3);

        Debug.Log("Comparison mode switched to: " + mode);

        if (inComparison)
        {
            DisplayComparison();
        }
    }
    
    /// <summary>
    /// Sets a specific ComparisonMode.
    /// </summary>
    /// <param name="inMode">The mode to activate</param>
    public void SetComparisonMode(int inMode)
    {
        mode = (ComparisonMode)inMode;

        Debug.Log("Comparison mode switched to: " + mode);

        if (inComparison)
        {
            DisplayComparison();
        }
    }

    /// <summary>
    /// Iterate through objects in the timeline to find the virtual twin and set it as reference.
    /// </summary>
    public void FindAndSetVirtualTwin()
    {
        // Find virtual twin
        var timelineObjs = FindObjectsOfType<VersionObject>();
        foreach (var vo in timelineObjs)
        {
            if (vo.virtualTwin)
            {
                virtualTwin = vo;
                break;
            }
        }
    }

    /// <summary>
    /// Calculate the floating distance between two objects, based off their colliders.
    /// </summary>
    /// <param name="obj1">First object, probably the tracked one.</param>
    /// <param name="obj2">Second object, probably from the timeline.</param>
    /// <returns>Calculated distance between objects.</returns>
    private float CalculateFloatingDistance(GameObject obj1, GameObject obj2)
    {
        float dist;
        // Calculate floating distance based on object sizes
        // NOTE: this could further be improved by calculating the maximum diaginonal distance
        var coll1 = obj1.GetComponent<Collider>().bounds.size;
        var coll2 = obj2.GetComponent<Collider>().bounds.size;

        float coll1max = Mathf.Max(Mathf.Max(coll1.x, coll1.y), coll1.z);
        float coll2max = Mathf.Max(Mathf.Max(coll2.x, coll2.z), coll2.z);

        dist = (coll1max / 2) + (coll2max / 2) + staticFloatingDistance;

        return dist;
    }

    /// <summary>
    /// Pulses the difference materials between visible and transparent.
    /// </summary>
    IEnumerator PulseMaterials()
    {
        // Initialize values
        float passedTime = 0.0f;
        bool invertDirection = false;

        while (true)
        {
            if (!inComparison) yield return null;

            passedTime += Time.deltaTime;
            if (passedTime >= pulseCadence)
            {
                passedTime = 0f;
                invertDirection = !invertDirection;

                // Hold state
                yield return new WaitForSeconds(pulseHold);
            }

            float alpha = invertDirection ? Mathf.SmoothStep(0f, pulseAlphaLimit, passedTime / pulseCadence) : Mathf.SmoothStep(pulseAlphaLimit, 0f, passedTime / pulseCadence);

            var subColor = diffMatSubtracted.color;
            diffMatSubtracted.color = new Color(subColor.r, subColor.g, subColor.b, alpha);

            var addColor = diffMatAdded.color;
            diffMatAdded.color = new Color(addColor.r, addColor.g, addColor.b, alpha);

            yield return null;
        }
    }

    public VersionObject GetVirtualTwin()
    {
        return virtualTwin;
    }
    
    public VersionObject GetComparedAgainstVersionObject()
    {
        return comparedAgainstVersionObject;
    }

    public bool IsInComparison()
    {
        return inComparison;
    }
}
