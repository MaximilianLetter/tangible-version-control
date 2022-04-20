using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComparisonObject : MonoBehaviour
{
    // External references
    private ComparisonManager comparisonManager;
    private Transform trackedObjTransform;
    //private Transform panel;

    // Side by side variables
    private float floatingDistance;
    private bool sideBySide;
    private bool hoverSide;

    // Internal values
    private MeshRenderer meshRenderer;
    private Material[] baseMat;
    private Transform transformInUse;
    private bool pivotCenter;
    private Transform modelContainer;

    void Start()
    {
        // Get references to necessary gameobjects
        comparisonManager = AppManager.Instance.GetComparisonManager();
        trackedObjTransform = AppManager.Instance.GetTrackedTransform();
        //panel = AppManager.Instance.GetActionPanel().transform;

        // Side by side variables
        transformInUse = transform;
        pivotCenter = false;
        hoverSide = false;
    }

    //void Update()
    //{
    //    if (!comparisonManager.IsInComparison()) return;

    //    Vector3 offset;
    //    Transform camTransform = Camera.main.transform;
    //    Vector3 trackedPos = trackedObjTransform.position;

    //    // Get distance and direction relative to camera
    //    float distance = Vector3.Distance(camTransform.position, trackedPos);
    //    Vector3 direction = (trackedPos - camTransform.position).normalized;

    //    // Calculate angle based on triangulation
    //    float angleF = Mathf.Asin(floatingDistance / Vector3.Distance(camTransform.position, trackedPos));
    //    float yAngle = (angleF * 180) / Mathf.PI;

    //    // Flip the floating side if specified
    //    if (hoverSide) yAngle = 360 - yAngle;

    //    // Calculate offset based on angle, direction and distance
    //    // In case of a steep camera angle the positioning bugs out
    //    offset = Quaternion.Euler(0, yAngle, 0) * direction * distance;

    //    if (sideBySide)
    //    {
    //        // Apply calculated position and rotation
    //        transformInUse.SetPositionAndRotation(
    //            camTransform.position + offset,
    //            trackedObjTransform.rotation
    //        );
    //    }
    //}

    /// <summary>
    /// Activates the floating mode besides the tracked object.
    /// </summary>
    /// <param name="distance">Distance between the tracked and the compared object.</param>
    //public void SetSideBySide(bool state, float distance)
    //{
    //    floatingDistance = distance;
    //    sideBySide = state;

    //    transformInUse.SetParent(rightAnchor);
    //    transformInUse.localPosition = Vector3.zero;
    //}

    public void SetOnAnchor(Transform anchor, float distance, bool flipSide = false)
    {
        anchor.localPosition = new Vector3(distance, 0, 0);

        transform.SetParent(anchor);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        if (flipSide)
        {
            anchor.localPosition *= -1;
        }

        SetModelOffset(true);
    }

    public void SetModelOffset(bool state)
    {
        var markerOffset = new Vector3(0, state ? 0.015f : 0, 0); // AppManager.Instance.GetTrackedObjectLogic().transform.parent.localPosition;
        modelContainer.localPosition = markerOffset;
    }

    /// <summary>
    /// Resets the material of the comparison object.
    /// </summary>
    public void ResetMaterial()
    {
        meshRenderer.materials = baseMat;
    }

    /// <summary>
    /// Activates the object and clones the model from the given VersionObject.
    /// <param name="voToClone">VersionObject to obtain mesh and scale from.</param>
    /// <returns>The primary model used for comparison.</returns>
    public GameObject Initialize(VersionObject voToClone)
    {
        modelContainer = Instantiate(voToClone.GetModelContainer(), transform);
        Destroy(modelContainer.GetComponent<BoxCollider>());
        modelContainer.tag = "Untagged";

        meshRenderer = GetComponentInChildren<MeshRenderer>();
        // Get base material directly from version Object;
        baseMat = voToClone.GetBaseMaterial();
        ResetMaterial();

        // Set pivot point according the the current mode
        if (pivotCenter) SetPivotPointCenter();
        else SetPivotPointBottom();

        gameObject.SetActive(true);

        return modelContainer.gameObject;
    }

    public void ReInitialize(GameObject mainModel)
    {
        Debug.Log("ReInitialize called");
        meshRenderer = mainModel.GetComponentInChildren<MeshRenderer>();
        Debug.Log(meshRenderer);
    }

    /// <summary>
    /// Flips the side the object floats from left to right and vice versa.
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
}
