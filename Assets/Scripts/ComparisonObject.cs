﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComparisonObject : MonoBehaviour
{
    private MeshRenderer meshRenderer;
    private Material baseMat;

    private Transform trackedObjTransform;

    public float floatingDistance = 0.15f;
    public int minAngle = 10;
    public bool hoverNext;

    // Start is called before the first frame update
    void Start()
    {
        meshRenderer = gameObject.GetComponent<MeshRenderer>();
        baseMat = meshRenderer.material;

        trackedObjTransform = GameObject.Find("TrackedContainer").transform;
    }

    // Update is called once per frame
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

    public void SetOverlayMaterial(Material mat)
    {
        if (meshRenderer != null)
        {
            meshRenderer.material = mat;
        }
    }

    public void Reset()
    {
        hoverNext = false;

        if (meshRenderer != null)
        {
            meshRenderer.material = baseMat;
        }
    }
}
