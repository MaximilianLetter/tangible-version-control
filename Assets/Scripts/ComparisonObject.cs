using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities;


public enum DifferencesDisplayMode { OutlinesOnly, HighlightColor, OriginalColor, AnimatedHighlights }

struct Differences
{
    public GameObject[] added;
    public GameObject[] removed;
    public GameObject[] modified;

    public Differences(GameObject[] added, GameObject[] removed, GameObject[] modified)
    {
        this.added = added;
        this.removed = removed;
        this.modified = modified;
    }
}

[RequireComponent(typeof(ObjectParts))]
public class ComparisonObject : MonoBehaviour
{
    // External references
    private ComparisonManager comparisonManager;
    private Transform trackedObjTransform;
    private ObjectParts differencesMgmt;
    public Transform panel;

    // Internal references
    private GameObject[] parts;
    private ObjectParts partMgmt;

    // Side by side variables
    private float floatingDistance;
    private bool sideBySide;
    private bool hoverSide;

    // Overlay variables
    private int materialIndex;

    // Differences variables
    private Differences differences;
    private DifferencesDisplayMode diffMode;

    // Internal values
    private Transform transformInUse;
    private bool pivotCenter;
    private bool ready;

    void Start()
    {
        // Get references to necessary gameobjects
        comparisonManager = AppManager.Instance.GetComparisonManager();
        trackedObjTransform = AppManager.Instance.GetTrackedTransform();
        differencesMgmt =  GameObject.Find("DifferencesObject").GetComponent<ObjectParts>();
        partMgmt = GetComponent<ObjectParts>();

        // Side by side variables
        transformInUse = transform;
        pivotCenter = false;
        hoverSide = false;

        // Differences variables
        diffMode = DifferencesDisplayMode.AnimatedHighlights;

        ready = true;
    }

    void Update()
    {
        if (!comparisonManager.IsInComparison()) return;

        Vector3 offset, menuOffset;
        Transform camTransform = Camera.main.transform;
        Vector3 trackedPos = trackedObjTransform.position;

        // Get distance and direction relative to camera
        float distance = Vector3.Distance(camTransform.position, trackedPos);
        Vector3 direction = (trackedPos - camTransform.position).normalized;

        // Calculate angle based on triangulation
        float angleF = Mathf.Asin(floatingDistance / Vector3.Distance(camTransform.position, trackedPos));
        float yAngle = (angleF * 180) / Mathf.PI;

        // Flip the floating side if specified
        if (hoverSide) yAngle = 360 - yAngle;

        // Calculate offset based on angle, direction and distance
        // In case of a steep camera angle the positioning bugs out
        offset = Quaternion.Euler(0, yAngle, 0) * direction * distance;

        if (sideBySide)
        {
            // Apply calculated position and rotation
            transformInUse.SetPositionAndRotation(
                camTransform.position + offset,
                trackedObjTransform.rotation
            );
        }

        // Position the panel on the other side
        if (panel.gameObject.activeSelf)
        {
            menuOffset = Quaternion.Euler(0, 360 - yAngle, 0) * direction * distance;
            panel.position = camTransform.position + menuOffset;
        }
    }

    /// <summary>
    /// Activates the floating mode besides the tracked object.
    /// </summary>
    /// <param name="distance">Distance between the tracked and the compared object.</param>
    public void SetSideBySide(float distance)
    {
        floatingDistance = distance;
        sideBySide = true;
    }

    /// <summary>
    /// Cycles through the possible difference modes.
    /// </summary>
    public void CycleDifferencesDisplay()
    {
        diffMode = (DifferencesDisplayMode)(((int)diffMode + 1) % 4);

        SetDifferenceMode(diffMode);
    }

    /// <summary>
    /// Set the specified mode for displaying the differences.
    /// </summary>
    /// <param name="mode">Which mode shall be displayed.</param>
    private void SetDifferenceMode(DifferencesDisplayMode mode)
    {
        differencesMgmt.StopPulseParts();
        diffMode = mode;

        if (diffMode == DifferencesDisplayMode.OutlinesOnly)
        {
            differencesMgmt.SetMaterial(comparisonManager.phantomMat);
        }
        else if (diffMode == DifferencesDisplayMode.HighlightColor)
        {
            differencesMgmt.SetMaterial(comparisonManager.greenMat, differences.added);
            differencesMgmt.SetMaterial(comparisonManager.redMat, differences.removed);
            differencesMgmt.SetMaterial(comparisonManager.yellowMat, differences.modified);
        }
        else if (diffMode == DifferencesDisplayMode.OriginalColor)
        {
            differencesMgmt.ResetMaterial(true);
        }
        else if (diffMode == DifferencesDisplayMode.AnimatedHighlights)
        {
            differencesMgmt.ResetMaterial(true);
            differencesMgmt.StartPulseParts(differences.added, differences.modified);
            differencesMgmt.SetMaterial(comparisonManager.phantomMat, differences.removed);
        }
    }


    /// <summary>
    /// Resets any applied comparison operations to default.
    /// </summary>
    public void Reset()
    {
        sideBySide = false;

        partMgmt.ResetMaterial();
        ClearDifferenceHighlights();
    }

    /// <summary>
    /// Activates the object and applies the mesh and size of the given object to itself.
    /// </summary>
    /// <param name="toClone">Object to obtain mesh and scale from.</param>
    public void Activate(GameObject toClone)
    {
        parts = new GameObject[toClone.transform.childCount];

        for (int i = 0; i < toClone.transform.childCount; i++)
        {
            // Clone each part of the object, remove the MeshOutline Script
            GameObject original = toClone.transform.GetChild(i).gameObject;
            GameObject part = Instantiate(original, transform);

            // Make sure the real name of the part is kept for part-wise comparisons
            part.name = original.name;

            // Store necessary information about parts
            parts[i] = part;
        }
        partMgmt.CollectRenderersAndMaterials(parts);

        // Set pivot point according the the current mode
        if (pivotCenter) SetPivotPointCenter();
        else SetPivotPointBottom();

        gameObject.SetActive(true);
    }

    /// <summary>
    /// Deactivates the object and removes applied mesh and scale operations.
    /// </summary>
    public void Deactivate()
    {
        // Reset mesh and scale
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }

        parts = null;
        partMgmt.ResetRenderersAndMaterials();

        if (comparisonManager.mode == ComparisonMode.Differences)
        {
            ClearDifferenceHighlights();
        }

        gameObject.SetActive(false);
    }

    /// <summary>
    /// Highlight the detected differing object parts.
    /// </summary>
    public void HighlightDifferences()
    {
        // Get references to the now relevant differences obj
        GameObject actualObj = trackedObjTransform.GetChild(0).gameObject;
        differences = DetectDifferences(actualObj, parts);

        // NOTE: The boolean comparison is kind of a workaround to identify if overall objects need to be added or are missing
        actualObj.GetComponent<ObjectParts>().ResetMaterial(true);

        int i = 0;
        GameObject[] diffPartsAdded = new GameObject[differences.added.Length];
        foreach (var diff in differences.added)
        {
            var newGO = Instantiate(diff, differencesMgmt.transform);
            MeshOutline outL = newGO.GetComponent<MeshOutline>();
            if (outL == null)
            {
                outL = newGO.AddComponent<MeshOutline>();
                outL.OutlineWidth = comparisonManager.outlineWidth;
            }
            outL.OutlineMaterial = comparisonManager.greenHighlight;

            diffPartsAdded[i] = newGO;
            i++;
        }

        i = 0;
        GameObject[] diffPartsRemoved = new GameObject[differences.removed.Length];
        foreach (var diff in differences.removed)
        {
            var newGO = Instantiate(diff, differencesMgmt.transform);
            MeshOutline outL = newGO.GetComponent<MeshOutline>();
            if (outL == null)
            {
                outL = newGO.AddComponent<MeshOutline>();
                outL.OutlineWidth = comparisonManager.outlineWidth;
            }
            outL.OutlineMaterial = comparisonManager.redHighlight;

            // HACK: Unique to removed materials, this cause a lot of additional GetComponentCalls
            newGO.GetComponent<PreserveMaterial>().CopyPreservedMat(diff.GetComponent<PreserveMaterial>().GetBaseMat());

            diffPartsRemoved[i] = newGO;
            i++;
        }

        i = 0;
        GameObject[] diffPartsModified = new GameObject[differences.modified.Length];
        foreach (var diff in differences.modified)
        {
            var newGO = Instantiate(diff, differencesMgmt.transform);
            MeshOutline outL = newGO.GetComponent<MeshOutline>();
            if (outL == null)
            {
                outL = newGO.AddComponent<MeshOutline>();
                outL.OutlineWidth = comparisonManager.outlineWidth;
            }
            outL.OutlineMaterial = comparisonManager.transitionHighlight;

            diffPartsModified[i] = newGO;
            i++;
        }

        // Override the difference instance with the newly created objects
        // so they can be individually altered
        differences = new Differences(diffPartsAdded, diffPartsRemoved, diffPartsModified);

        // Set the differences obj as phantom with outlines
        // NOTE: order matters! collect > outlines > material
        differencesMgmt.CollectRenderersAndMaterials();
        differencesMgmt.ToggleOutlines(true);
        SetDifferenceMode(diffMode);

        // HACK
        actualObj.GetComponent<ObjectParts>().SetMaterial(comparisonManager.invisibleMat);

        // Set the comparison obj itself completely invisible
        partMgmt.SetMaterial(comparisonManager.invisibleMat);
    }

    /// <summary>
    /// Removes all children from the difference object and resets the overlay material of the comparison obj.
    /// </summary>
    private void ClearDifferenceHighlights()
    {
        foreach (Transform child in differencesMgmt.transform)
        {
            Destroy(child.gameObject);
        }

        SetOverlayMaterial(true);
    }

    /// <summary>
    /// Detect block differences between objects based on the part order, replacement of existing parts is not supported.
    /// </summary>
    /// <param name="obj1">The physical object.</param>
    /// <param name="obj2Parts">This is always the comparison object, which has its parts stored in a separate variable.</param>
    /// <returns>List of parts that differ between the two given objects.</returns>
    private Differences DetectDifferences(GameObject obj1, GameObject[] obj2Parts)
    {
        // Transfer child information in array to be in line with obj2Parts
        GameObject[] obj1Parts = new GameObject[obj1.transform.childCount];
        for (int i = 0; i < obj1.transform.childCount; i++)
        {
            obj1Parts[i] = obj1.transform.GetChild(i).gameObject;
        }

        List<GameObject> parts1 = new List<GameObject>(obj1Parts);
        List<GameObject> parts2 = new List<GameObject>(obj2Parts);
        List<GameObject> modifiedParts = new List<GameObject>();

        // Compare each part with each in a double loop
        GameObject part1, part2;
        bool restart = false;
        for (int i = 0; i < parts1.Count; i++)
        {
            // As the lists are updated restart iterating the reduced list after a match was found
            if (restart)
            {
                i = 0;
                restart = false;
            }

            part1 = parts1[i];

            for (int j = 0; j < parts2.Count; j++)
            {
                part2 = parts2[j];

                if (part1.transform.localPosition == part2.transform.localPosition &&
                    part1.transform.localRotation == part2.transform.localRotation)
                {
                    // If the name does not fit, the parts were modified
                    if (part1.name != part2.name)
                    {
                        modifiedParts.Add(part2);
                    }

                    parts1.RemoveAt(i);
                    parts2.RemoveAt(j);

                    restart = true;
                    i = -1; // HACK: so the for-loop does not abort if 1 part is missing and the increments sets i to 1 before it is reset to 0
                    break;
                }
            }
        }

        // The remaining parts2 must be additions to the existing parts, the remaining parts1 must be removed parts
        List<GameObject> addedParts = parts2;
        List<GameObject> removedParts = parts1;

        Differences diffs = new Differences(addedParts.ToArray(), removedParts.ToArray(), modifiedParts.ToArray());

        return diffs;
    }

    /// <summary>
    /// Flips the side the object floats from left to right and visa versa.
    /// </summary>
    public void FlipHoverSide()
    {
        hoverSide = !hoverSide;
    }

    /// <summary>
    /// Switch the pivot point between center and bottom.
    /// </summary>
    public void SwitchPivotPoint()
    {
        if (!pivotCenter) SetPivotPointCenter();
        else SetPivotPointBottom();
    }

    /// <summary>
    /// Set the pivot point to bottom by updating the parent transform and offsetting the object to match the tracked object's bottom point.
    /// </summary>
    private void SetPivotPointCenter()
    {
        if (pivotCenter) return;

        transformInUse = transform.parent;
        transform.localRotation = Quaternion.identity;

        // Calculate the needed offset to match the tracked object's bottom point
        float heightCompObj = CalculateParentBounds().size.y;
        float heightTrackedObj = trackedObjTransform.GetChild(0).GetComponent<Collider>().bounds.size.y;

        float offset;
        if (heightTrackedObj > heightCompObj)
        {
            offset = (heightTrackedObj / 2) - (heightCompObj / 2);
        }
        else
        {
            offset = (heightCompObj / 2) - (heightTrackedObj / 2);
        }

        // NOTE: The pivot point is the bottom of the object, regardless of orientation
        // This could result in unexpected behavior
        transform.localPosition = trackedObjTransform.localRotation *  new Vector3(0, offset, 0);

        pivotCenter = true;
    }

    /// <summary>
    /// Calculates bounds based on the children objects.
    /// Taken from https://stackoverflow.com/questions/11949463/how-to-get-size-of-parent-game-object
    /// </summary>
    /// <returns></returns>
    private Bounds CalculateParentBounds()
    {
        Renderer[] children = GetComponentsInChildren<Renderer>();

        // First find a center for your bounds
        Vector3 center = Vector3.zero;

        foreach (Renderer child in children)
        {
            center += child.bounds.center;
        }
        center /= transform.childCount; //center is average center of children

        //Now you have a center, calculate the bounds by creating a zero sized 'Bounds', 
        Bounds bounds = new Bounds(center, Vector3.zero);

        foreach (Renderer child in children)
        {
            bounds.Encapsulate(child.bounds);
        }

        return bounds;
    }

    /// <summary>
    /// Set the pivot point to center by directly modifying its transform position.
    /// </summary>
    public void SetPivotPointBottom()
    {
        if (!pivotCenter) return;

        transform.parent.position = Vector3.zero;
        transformInUse = transform;

        pivotCenter = false;
    }

    /// <summary>
    /// Cycle through the options for displaying the overlayed object.
    /// </summary>
    public void CycleMaterials()
    {
        materialIndex = (materialIndex + 1) % comparisonManager.overlayMats.Length;

        SetOverlayMaterial();
    }

    /// <summary>
    /// Sets the currently active overlay material.
    /// </summary>
    public void SetOverlayMaterial(bool reset = false)
    {
        if (reset) materialIndex = 0;

        if (materialIndex == 0) partMgmt.ResetMaterial();
        else partMgmt.SetMaterial(comparisonManager.overlayMats[materialIndex]);
    }
    public bool IsReady()
    {
        return ready;
    }
}
