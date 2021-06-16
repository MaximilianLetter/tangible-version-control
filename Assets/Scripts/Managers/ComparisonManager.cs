using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities;
public enum ComparisonMode { SideBySide, Overlay, Differences }

public class ComparisonManager : MonoBehaviour
{
    // Singleton setup
    private static ComparisonManager _instance;

    public static ComparisonManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new ComparisonManager();
                Debug.Log("Comparison Manager created");
            }
            return _instance;
        }
    }

    // Use tracking and physical objects
    public bool usePhysical;
    public float staticFloatingDistance;

    [Header("Object Materials")]
    public Material phantomMat;
    public Material invisibleMat;
    public Material edgesMat;
    public Material greenMat;
    public Material redMat;
    public Material yellowMat;
    public Material[] overlayMats;

    //[Space(10)]
    [Header("Outline Materials")]
    public Material neutralHighlight;
    public Material greenHighlight;
    public Material redHighlight;
    public Material yellowHighlight;

    public Color32 textHighlight;
    public Color32 textDefault;

    // Required object references
    private ActionPanel actionPanel;
    private TrackedObject trackedObj;
    private Transform trackedTransform;
    private ComparisonObject comparisonObj;
    private Transform comparisonObjContainer;
    private VersionObject virtualTwin;
    private LineRenderer comparisonLine;

    // State variables
    private bool ready;
    private GameObject versionHistoryObj;
    private float floatingDistance;
    private bool inComparison;

    [Space(14)]
    public ComparisonMode mode;

    private void Awake()
    {
        _instance = this;

        // Get relevant gameobject logic
        actionPanel = GameObject.FindObjectOfType<ActionPanel>();
        trackedObj = GameObject.FindObjectOfType<TrackedObject>();
        comparisonLine = GameObject.Find("ComparisonLine").GetComponent<LineRenderer>();
        
        // Find the virtual twin between the version objects
        var vObjs = GameObject.FindObjectsOfType<VersionObject>();
        foreach (var obj in vObjs)
        {
            if (obj.virtualTwin)
            {
                virtualTwin = obj;
                break;
            }
        }

        comparisonObj = GameObject.FindObjectOfType<ComparisonObject>();
        comparisonObjContainer = comparisonObj.transform.parent;

        // Get relevant transform information
        trackedTransform = trackedObj.transform.parent;
        
        // NOTE: giving the markerPlane a phantom materials results in unwanted behavior occluding the comparison object
        //var markerPlane = trackedTransform.Find("MarkerPlane");
        //if (markerPlane != null)
        //{
        //    markerPlane.GetComponent<Renderer>().material = phantomMat;
        //}

        // Initialize states
        comparisonLine.enabled = false;
        inComparison = false;
        mode = ComparisonMode.SideBySide;

#if UNITY_EDITOR
        // A bigger floating distance is required on HoloLens
        staticFloatingDistance = 0.045f;
#endif

        ready = true;
    }

    /// <summary>
    /// Starts a comparison between the tracked physical object and the collided object of the version history.
    /// </summary>
    /// <param name="physicalObj"></param>
    /// <param name="versionObj"></param>
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
            if (virtualObj == versionHistoryObj)
            {
                Debug.Log("This comparison is already active; ABORT COMPARISON.");
                return;
            }

            // Reset if a new comparison is about to start
            StopComparison();
        }

        Debug.Log("A new comparison is initiated. START COMPARISON");

        // Save reference to object for avoiding reinitializing the same comparison
        versionHistoryObj = virtualObj;

        // NOTE: Order matters, first clone the object, then activate the comparison
        comparisonObj.Activate(virtualObj);
        HighlightInHistory();
        inComparison = true;

        floatingDistance = CalculateFloatingDistance(physicalObj, virtualObj);

        // Fill information panel with content and show
        actionPanel.gameObject.SetActive(true);
        //actionPanel.SetContents(virtualTwin, versionObj, floatingDistance);

        DisplayComparison();
    }

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
    /// Displays a comparison operation based on the currently active comparison mode.
    /// </summary>
    private void DisplayComparison()
    {
        // Reset properties of tracked object and comparison object
        comparisonObj.Reset();
        trackedObj.ResetMaterial();

        // Activate effects based on activated mode
        if (mode == ComparisonMode.SideBySide)
        {
            comparisonObj.transform.parent = comparisonObjContainer;
            comparisonObj.SetSideBySide(floatingDistance);
            comparisonObj.SetOverlayMaterial(true);
        }
        else if (mode == ComparisonMode.Overlay)
        {
            comparisonObj.SetPivotPointBottom();
            comparisonObj.transform.parent = trackedTransform;
            comparisonObj.transform.localPosition = Vector3.zero;

            // NOTE: the phantom Mat occludes the overlayed mat, short term solution > invisible material
            //trackedObj.SetMaterial(phantomMat);
            trackedObj.SetMaterial(invisibleMat);
            comparisonObj.SetOverlayMaterial();
        }
        else if (mode == ComparisonMode.Differences)
        {
            comparisonObj.SetPivotPointBottom();
            comparisonObj.transform.parent = trackedTransform;
            comparisonObj.transform.localPosition = Vector3.zero;

            trackedObj.SetMaterial(invisibleMat);
            comparisonObj.HighlightDifferences();
        }

        actionPanel.SetOptions();
    }

    /// <summary>
    /// Resets the status of tracked and comparison object, also resets internal values.
    /// </summary>
    public void StopComparison()
    {
        // Disable highlight on version object
        if (versionHistoryObj != null)
        {
            versionHistoryObj.GetComponentInParent<ObjectParts>().ToggleOutlines(false);
            versionHistoryObj.GetComponentInParent<VersionObject>().ChangeTextColor(textDefault);
            comparisonLine.enabled = false;
            versionHistoryObj = null;
        }

        actionPanel.gameObject.SetActive(false);

        comparisonObj.Deactivate();
        trackedObj.ResetMaterial();

        inComparison = false;
    }

    /// <summary>
    /// Activate outlines on the version object and draw a line between virtual twin and version object.
    /// </summary>
    private void HighlightInHistory()
    {
        // Highlight the versionObj as being compared against
        versionHistoryObj.GetComponentInParent<ObjectParts>().ToggleOutlines(true);
        versionHistoryObj.GetComponentInParent<ObjectParts>().SetMaterial(edgesMat);
        versionHistoryObj.GetComponentInParent<VersionObject>().ChangeTextColor(textHighlight);

        float height1 = virtualTwin.GetComponentInChildren<Collider>().bounds.size.y;
        float height2 = versionHistoryObj.GetComponentInChildren<Collider>().bounds.size.y;

        Vector3 posStart = virtualTwin.transform.position + new Vector3(0, height1 / 2, 0);
        Vector3 posEnd = versionHistoryObj.transform.position + new Vector3(0, height2 / 2, 0);

        comparisonLine.enabled = true;
        comparisonLine.positionCount = 4;
        comparisonLine.SetPositions(new[] {
            posStart,
            posStart + (virtualTwin.transform.up * height2) + (virtualTwin.transform.up * height1 / 2),
            posEnd +  (versionHistoryObj.transform.up * height1) + (versionHistoryObj.transform.up * height2 / 2),
            posEnd
        });
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

    public bool IsInComparison()
    {
        return inComparison;
    }

    public bool IsReady()
    {
        return ready;
    }
}
