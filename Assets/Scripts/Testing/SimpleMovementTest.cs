﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMovementTest : MonoBehaviour
{
    public float speed;
    private TimelineManager timelineM;
    private ComparisonManager comparisonM;

    private void Start()
    {
        timelineM = AppManager.Instance.GetTimelineManager();
        comparisonM = AppManager.Instance.GetComparisonManager();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 dir = Vector3.zero;

        if (Input.GetKey("up"))
        {
            dir = Vector3.forward;
        }
        else if (Input.GetKey("down"))
        {
            dir = -Vector3.forward;
        }
        else if (Input.GetKey("left"))
        {
            dir = Vector3.left;
        }
        else if (Input.GetKey("right"))
        {
            dir = Vector3.right;
        }

        transform.position += dir * Time.deltaTime * speed;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            timelineM.FinishPlacement();
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            comparisonM.SwitchComparisonMode();
        }
    }
}