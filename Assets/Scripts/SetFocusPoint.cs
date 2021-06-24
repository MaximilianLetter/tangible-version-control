﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetFocusPoint : MonoBehaviour
{
    public GameObject focusedObject;
    void Update()
    {
        // Normally the normal is best set to be the opposite of the main camera's
        // forward vector.
        // If the content is actually all on a plane (like text), set the normal to
        // the normal of the plane and ensure the user does not pass through the
        // plane.
        var normal = -Camera.main.transform.forward;
        var position = focusedObject.transform.position;
        UnityEngine.XR.WSA.HolographicSettings.SetFocusPointForFrame(position, normal);
    }
}
