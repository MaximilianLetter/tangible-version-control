﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities;

[RequireComponent(typeof(ObjectParts))]
public class ComparisonObject : MonoBehaviour
{
    // External references
    private Transform trackedObjTransform;

    // Internal references
    private GameObject[] parts;
    private ObjectParts partMgmt;

    // Side by side variables
    private float floatingDistance;
    private bool sideBySide;
    private bool hoverSide;

    // Overlay variables
    private int materialIndex;

    private Transform transformInUse;
    private bool bottomPivot;
    private bool ready;

    void Start()
    {
        // Get references to necessary gameobjects
        trackedObjTransform = GameObject.Find("TrackedContainer").transform;
        partMgmt = GetComponent<ObjectParts>();

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

    /// <summary>
    /// Resets any applied comparison operations to default.
    /// </summary>
    public void Reset()
    {
        sideBySide = false;

        partMgmt.CollectRenderersAndMaterials();
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
            
            // NOTE: Outlines are FOR NOW not necessary on comparison objects
            Destroy(part.GetComponent<MeshOutline>());
            
            // Store necessary information about parts
            parts[i] = part;
        }
        partMgmt.CollectRenderersAndMaterials();

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
        foreach (var part in parts)
        {
            Destroy(part);
        }

        parts = null;
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
        transform.localRotation = Quaternion.identity;

        // Calculate the needed offset to match the tracked object's bottom point
        float heightCompObj = transform.GetComponent<MeshFilter>().mesh.bounds.size.y * transform.localScale.y;
        float heightTrackedObj = trackedObjTransform.GetComponent<Collider>().bounds.size.y * trackedObjTransform.localScale.y;
        float offset = (heightCompObj / 2) - (heightTrackedObj / 2);

        // NOTE: The pivot point is the bottom of the object, regardless of orientation
        // This could result in unexpected behavior
        transform.localPosition = trackedObjTransform.localRotation *  new Vector3(0, offset, 0);
    }

    /// <summary>
    /// Set the pivot point to center by directly modifying its transform position.
    /// </summary>
    private void SetPivotPointCenter()
    {
        transform.parent.position = Vector3.zero;
        transformInUse = transform;
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
    public void SetOverlayMaterial()
    {
        if (materialIndex == 0) partMgmt.ResetMaterial();
        else partMgmt.SetMaterial(ComparisonManager.Instance.overlayMats[materialIndex]);
    }
}
