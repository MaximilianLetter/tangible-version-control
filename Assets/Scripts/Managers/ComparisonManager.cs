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

    // Comparison properties
    public Material phantomMat;
    public Material invisibleMat;
    public Material[] overlayMats;
    public float staticFloatingDistance;

    // Required object references
    private InformationPanel informationPanel;
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
    public ComparisonMode mode;

    private void Awake()
    {
        _instance = this;

        // Get relevant gameobject logic
        informationPanel = GameObject.FindObjectOfType<InformationPanel>();
        trackedObj = GameObject.FindObjectOfType<TrackedObject>();
        comparisonLine = GameObject.FindObjectOfType<LineRenderer>();
        
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
            Debug.Log("Comparing against virtual twin");
            return;
        }

        // Check for existing comparison, suppress reinitializing the same comparison
        if (inComparison)
        {
            if (virtualObj == versionHistoryObj) return;

            // Reset if a new comparison is about to start
            ResetComparison();
        }

        Debug.Log("Comparison started");

        // Save reference to object for avoiding reinitializing the same comparison
        versionHistoryObj = virtualObj;

        // NOTE: Order matters, first clone the object, then activate the comparison
        comparisonObj.Activate(virtualObj);
        HighlightComparison();
        inComparison = true;

        // Calculate floating distance based on object sizes
        // NOTE: this could further be improved by calculating the maximum diaginonal distance
        var coll1 = physicalObj.GetComponent<Collider>().bounds.size;
        var coll2 = virtualObj.GetComponent<Collider>().bounds.size;

        float coll1max = Mathf.Max(Mathf.Max(coll1.x, coll1.y), coll1.z);
        float coll2max = Mathf.Max(Mathf.Max(coll2.x, coll2.z), coll2.z);

        floatingDistance = (coll1max / 2) + (coll2max / 2) + staticFloatingDistance;

        // Fill information panel with content and show
        informationPanel.gameObject.SetActive(true);
        informationPanel.SetContents(virtualTwin, versionObj, floatingDistance);

        DisplayComparison();
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
            Debug.Log("Set overlay material");
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

        informationPanel.SetOptions();
    }

    /// <summary>
    /// Resets the status of tracked and comparison object, also resets internal values.
    /// </summary>
    public void ResetComparison()
    {
        // Disable highlight on version object
        if (versionHistoryObj != null)
        {
            versionHistoryObj.GetComponentInParent<ObjectParts>().ToggleOutlines(false);
            comparisonLine.enabled = false;
            versionHistoryObj = null;
        }

        inComparison = false;

        informationPanel.gameObject.SetActive(false);

        comparisonObj.Deactivate();
        trackedObj.ResetMaterial();
    }

    /// <summary>
    /// Activate outlines on the version object and draw a line between virtual twin and version object.
    /// </summary>
    private void HighlightComparison()
    {
        // Highlight the versionObj as being compared against
        versionHistoryObj.GetComponentInParent<ObjectParts>().ToggleOutlines(true);

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

    public bool IsReady()
    {
        return ready;
    }
}
