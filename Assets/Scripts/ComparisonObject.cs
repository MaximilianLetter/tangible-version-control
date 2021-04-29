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
    private bool hoverNext;
    private bool hoverSide;
    private bool ready;

    void Start()
    {
        // Setup component references and starting material
        meshFilter = gameObject.GetComponent<MeshFilter>();
        meshRenderer = gameObject.GetComponent<MeshRenderer>();
        baseMat = meshRenderer.material;

        // Get references to necessary gameobjects
        trackedObjTransform = GameObject.Find("TrackedContainer").transform;

        ready = true;
    }

    void Update()
    {
        if (hoverNext)
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
            transform.SetPositionAndRotation(
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
        hoverNext = true;
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
        hoverNext = false;

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
}
