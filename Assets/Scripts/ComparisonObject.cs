using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComparisonObject : MonoBehaviour
{
    // External references
    private Transform trackedObjTransform;

    // Internal references
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Material baseMat;

    // Internal variables
    private float floatingDistance;
    private bool sideBySide;
    private bool hoverSide;

    private Transform transformInUse;
    private bool bottomPivot;
    private bool ready;

    void Start()
    {
        // Setup component references and starting material
        meshFilter = gameObject.GetComponent<MeshFilter>();
        meshRenderer = gameObject.GetComponent<MeshRenderer>();
        baseMat = meshRenderer.material;

        // Get references to necessary gameobjects
        trackedObjTransform = GameObject.Find("TrackedContainer").transform;

        transformInUse = transform;
        bottomPivot = false;
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

    public void SetOverlayMaterial(Material mat)
    {
        meshRenderer.material = mat;
    }

    /// <summary>
    /// Resets any applied comparison operations to default.
    /// </summary>
    public void Reset()
    {
        sideBySide = false;

        if (meshRenderer != null)
        {
            meshRenderer.material = baseMat;
        }
    }

    /// <summary>
    /// Activates the object and applies the mesh and size of the given object to itself.
    /// </summary>
    /// <param name="toClone">Object to obtain mesh and scale from.</param>
    public void Activate(GameObject toClone)
    {
        // Copy information from original object
        meshFilter.mesh = toClone.GetComponent<MeshFilter>().mesh;
        transform.localScale = toClone.transform.localScale;

        // Set pivot point according the the current mode
        if (bottomPivot) SetPivotPointBottom();
        else SetPivotPointCenter();

        gameObject.SetActive(true);
    }

    /// <summary>
    /// Deactivates the object and removes applied mesh and scale operations.
    /// </summary>
    public void Deactivate()
    {
        // Reset mesh and scale
        meshFilter.mesh = null;
        transform.localScale = Vector3.one;

        gameObject.SetActive(false);
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
        bottomPivot = !bottomPivot;

        // Set pivot point according to the new mode
        if (bottomPivot) SetPivotPointBottom();
        else SetPivotPointCenter();
    }

    /// <summary>
    /// Set the pivot point to bottom by updating the parent transform and offsetting the object to match the tracked object's bottom point.
    /// </summary>
    private void SetPivotPointBottom()
    {
        transformInUse = transform.parent;

        // Calculate the needed offset to match the tracked object's bottom point
        float heightCompObj = transform.GetComponent<MeshFilter>().mesh.bounds.size.y * transform.localScale.y;
        float heightTrackedObj = trackedObjTransform.GetComponent<Collider>().bounds.size.y * trackedObjTransform.localScale.y;
        float offset = (heightCompObj / 2) - (heightTrackedObj / 2);

        transform.localPosition = new Vector3(0, offset, 0);
    }

    /// <summary>
    /// Set the pivot point to center by directly modifying its transform position.
    /// </summary>
    private void SetPivotPointCenter()
    {
        transform.parent.position = Vector3.zero;
        transformInUse = transform;
    }
}
