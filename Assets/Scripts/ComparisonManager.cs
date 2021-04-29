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

    // Comparison properties
    public Material transparentMat;
    public Material wireframesMat;
    public Material phantomMat;
    public float staticFloatingDistance;

    // Required object references
    private InformationPanel informationPanel;
    private TrackedObject trackedObj;
    private Transform trackedTransform;
    private ComparisonObject comparisonObj;
    private VirtualTwin virtualTwin;
    private LineRenderer comparisonLine;

    // State variables
    private GameObject originalVersionObj;
    private float floatingDistance;
    private bool inComparison;
    public ComparisonMode mode;

    private void Awake()
    {
        _instance = this;

        // Get relevant gameobject logic
        informationPanel = GameObject.FindObjectOfType<InformationPanel>();
        trackedObj = GameObject.FindObjectOfType<TrackedObject>();
        comparisonObj = GameObject.FindObjectOfType<ComparisonObject>();
        virtualTwin = GameObject.FindObjectOfType<VirtualTwin>();
        comparisonLine = GameObject.FindObjectOfType<LineRenderer>();

        // Get relevant transform information
        trackedTransform = trackedObj.transform.parent;

        // Initialize states
        informationPanel.gameObject.SetActive(false);
        comparisonLine.enabled = false;
        inComparison = false;
        mode = ComparisonMode.SideBySide;
    }

    /// <summary>
    /// Starts a comparison between the tracked physical object and the collided object of the version history.
    /// </summary>
    /// <param name="physicalObj"></param>
    /// <param name="versionObj"></param>
    public void StartComparison(GameObject physicalObj, GameObject versionObj)
    {
        if (versionObj.GetComponent<VirtualTwin>() != null)
        {
            Debug.Log("Comparing against virtual twin");
            return;
        }

        // Check for existing comparison, suppress reinitializing the same comparison
        if (inComparison)
        {
            if (versionObj == originalVersionObj) return;

            // Reset if a new comparison is about to start
            ResetComparison();
        }

        Debug.Log("Comparison started");

        // Save reference to object for avoiding reinitializing the same comparison
        originalVersionObj = versionObj;

        HighlightComparison();
        inComparison = true;
        comparisonObj.Activate(versionObj);

        // Calculate floating distance based on object sizes
        // NOTE: this could further be improved by calculating the maximum diaginonal distance
        var coll1 = physicalObj.GetComponent<Collider>().bounds.size;
        var coll2 = versionObj.GetComponent<Collider>().bounds.size;

        float coll1max = Mathf.Max(Mathf.Max(coll1.x, coll1.y), coll1.z);
        float coll2max = Mathf.Max(Mathf.Max(coll2.x, coll2.z), coll2.z);

        floatingDistance = (coll1max / 2) + (coll2max / 2) + staticFloatingDistance;

        // Fill information panel with content and show
        informationPanel.gameObject.SetActive(true);
        informationPanel.SetContents(virtualTwin.GetComponent<VersionObject>(), versionObj.GetComponent<VersionObject>(), floatingDistance);

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
            comparisonObj.transform.parent = null;
            comparisonObj.SetSideBySide(floatingDistance);
        }
        else if (mode == ComparisonMode.Overlay)
        {
            comparisonObj.transform.parent = trackedTransform;
            comparisonObj.transform.localPosition = Vector3.zero;

            trackedObj.SetMaterial(phantomMat);
            comparisonObj.SetOverlayMaterial(transparentMat);
        }
        else if (mode == ComparisonMode.Differences)
        {
            comparisonObj.transform.parent = trackedTransform;
            comparisonObj.transform.localPosition = Vector3.zero;

            // NOTE: This will be replaced by another comparison operation
            trackedObj.SetMaterial(phantomMat);
            comparisonObj.SetOverlayMaterial(wireframesMat);
        }
    }

    /// <summary>
    /// Resets the status of tracked and comparison object, also resets internal values.
    /// </summary>
    public void ResetComparison()
    {
        // Disable highlight on version object
        originalVersionObj.GetComponent<MeshOutline>().enabled = false;
        comparisonLine.enabled = false;

        originalVersionObj = null;
        inComparison = false;

        informationPanel.gameObject.SetActive(false);

        comparisonObj.Deactivate();
        trackedObj.ResetMaterial();
    }

    private void HighlightComparison()
    {
        // Highlight the versionObj as being compared against
        originalVersionObj.GetComponent<MeshOutline>().enabled = true;

        float height1 = virtualTwin.GetComponent<Collider>().bounds.size.y;
        float height2 = originalVersionObj.GetComponent<Collider>().bounds.size.y;

        Vector3 posStart = virtualTwin.transform.position + new Vector3(0, height1 / 2, 0);
        Vector3 posEnd = originalVersionObj.transform.position + new Vector3(0, height2 / 2, 0);

        comparisonLine.enabled = true;
        comparisonLine.positionCount = 4;
        comparisonLine.SetPositions(new[] {
            posStart,
            posStart + virtualTwin.transform.up * height2,
            posEnd +  originalVersionObj.transform.up * height1,
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
}
