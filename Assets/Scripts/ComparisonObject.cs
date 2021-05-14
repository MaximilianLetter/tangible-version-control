using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities;

[RequireComponent(typeof(ObjectParts))]
public class ComparisonObject : MonoBehaviour
{
    // External references
    private Transform trackedObjTransform;
    private GameObject differencesObj;

    // Internal references
    private GameObject[] parts;
    private ObjectParts partMgmt;

    // Side by side variables
    private float floatingDistance;
    private bool sideBySide;
    private bool hoverSide;

    // Differences variables
    private Material diffOutline;

    // Overlay variables
    private int materialIndex;

    private Transform transformInUse;
    private bool pivotCenter;
    private bool ready;

    void Start()
    {
        // Get references to necessary gameobjects
        trackedObjTransform = GameObject.Find("TrackedContainer").transform;
        differencesObj = trackedObjTransform.Find("DifferencesObject").gameObject;
        partMgmt = GetComponent<ObjectParts>();

        transformInUse = transform;
        pivotCenter = false;
        hoverSide = false;
        ready = true;
    }

    void Update()
    {
        if (sideBySide)
        {
            Vector3 offset;
            Transform camTransform = Camera.main.transform;
            Vector3 trackedPos = trackedObjTransform.position;

            // Get distance and direction relative to camera
            float distance = Vector3.Distance(camTransform.position, trackedPos);
            Vector3 direction = (trackedPos - camTransform.position).normalized;
            
            //Vector2 splitDirection = new Vector2(direction.x, direction.z).normalized;
            //direction.x = splitDirection.x;
            //direction.z = splitDirection.y;

            // Calculate angle based on triangulation
            float angleF = Mathf.Asin(floatingDistance / Vector3.Distance(camTransform.position, trackedPos));
            float yAngle = (angleF * 180) / Mathf.PI;

            // Flip the floating side if specified
            if (hoverSide) yAngle = 360 - yAngle;

            // Calculate offset based on angle, direction and distance
            // In case of a steep camera angle the positioning bugs out
            offset = Quaternion.Euler(0, yAngle, 0) * direction * distance;

            // Apply calculated position and rotation
            transformInUse.SetPositionAndRotation(
                camTransform.position + offset,
                trackedObjTransform.rotation
            );
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
            GameObject part = Instantiate(toClone.transform.GetChild(i).gameObject, transform);
            
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
            Destroy(child.gameObject);
        }

        parts = null;
        partMgmt.CollectRenderersAndMaterials(new GameObject[0]);

        gameObject.SetActive(false);
    }

    /// <summary>
    /// 
    /// </summary>
    public void HighlightDifferences()
    {
        // Get references to the now relevant differences obj
        GameObject actualObj = trackedObjTransform.GetChild(0).gameObject;
        var diffPartsScript = differencesObj.GetComponent<ObjectParts>();
        var differences = DetectDifferences(actualObj, gameObject);

        foreach (var diff in differences)
        {
            var newGO = Instantiate(diff, differencesObj.transform);
            if (newGO.GetComponent<MeshOutline>() == null)
            {
                var outline = newGO.AddComponent<MeshOutline>();
                outline.OutlineWidth = 0.001f;
            }
        }
        // Set the differences obj as phantom with outlines
        // NOTE: order matters! collect > outlines > material
        diffPartsScript.CollectRenderersAndMaterials();
        diffPartsScript.SetOutlineMaterial(diffOutline);
        diffPartsScript.ToggleOutlines(true);
        diffPartsScript.SetMaterial(ComparisonManager.Instance.phantomMat);

        // Set the comparison obj itself completely invisible
        partMgmt.SetMaterial(ComparisonManager.Instance.invisibleMat);
    }

    /// <summary>
    /// Removes all children from the difference object and resets the overlay material of the comparison obj.
    /// </summary>
    private void ClearDifferenceHighlights()
    {
        foreach (Transform child in differencesObj.transform)
        {
            Destroy(child.gameObject);
        }

        SetOverlayMaterial(true);
    }

    /// <summary>
    /// Detect block differences between objects based on the part order.
    /// </summary>
    /// <param name="obj1">The physical object.</param>
    /// <param name="obj2">The virtual version object.</param>
    /// <returns>List of parts that differ between the two given objects.</returns>
    private GameObject[] DetectDifferences(GameObject obj1, GameObject obj2)
    {
        GameObject[] differences;

        int length1 = obj1.transform.childCount;
        int length2 = obj2.transform.childCount;

        int start = Mathf.Min(length1, length2);
        int end = Mathf.Max(length1, length2);

        // Decide if the objects are added to the current physical object or subtracted.
        Transform higherVersion;

        if (length1 > length2)
        {
            higherVersion = obj1.transform;
            diffOutline = ComparisonManager.Instance.redHighlight;
        }
        else
        {
            higherVersion = obj2.transform;
            diffOutline = ComparisonManager.Instance.greenHighlight;
        }

        // Fill array with gameobjects that differ
        differences = new GameObject[Mathf.Abs(length1 - length2)];

        int diffIndex = 0;
        for (int i = start; i < end; i++)
        {
            differences[diffIndex] = higherVersion.GetChild(i).gameObject;
            diffIndex++;
        }

        return differences;
    }

    public bool IsReady()
    {
        return ready;
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
        pivotCenter = !pivotCenter;

        // Set pivot point according to the new mode
        if (pivotCenter) SetPivotPointCenter();
        else SetPivotPointBottom();
    }

    /// <summary>
    /// Set the pivot point to bottom by updating the parent transform and offsetting the object to match the tracked object's bottom point.
    /// </summary>
    private void SetPivotPointCenter()
    {
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
        materialIndex = (materialIndex + 1) % ComparisonManager.Instance.overlayMats.Length;

        SetOverlayMaterial();
    }

    /// <summary>
    /// Sets the currently active overlay material.
    /// </summary>
    public void SetOverlayMaterial(bool reset = false)
    {
        if (reset) materialIndex = 0;

        if (materialIndex == 0) partMgmt.ResetMaterial();
        else partMgmt.SetMaterial(ComparisonManager.Instance.overlayMats[materialIndex]);
    }
}
