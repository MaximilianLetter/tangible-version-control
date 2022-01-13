using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionLine : MonoBehaviour
{
    public float fadeDuration;

    // External references
    private Transform physObj;
    private Transform virtualTwin;
    private TimelineManager timelineManager;

    // Internal references
    private LineRenderer lineRend;
    private Material lineMat;
    private Color lineCol;

    private bool isActive;

    public void Initialize()
    {
        lineRend = GetComponent<LineRenderer>();
        lineMat = lineRend.material;
        lineCol = lineMat.color;
        lineRend.positionCount = 2;

        physObj = AppManager.Instance.GetTrackedObjectLogic().transform;
        timelineManager = AppManager.Instance.GetTimelineManager();
    }

    public void ConnectVirtualAndPhysical()
    {
        lineRend.SetPositions(new[]
        {
             physObj.position,
             virtualTwin.position
        });
    }

    public IEnumerator FadeLine(bool fadeIn)
    {
        float passedTime = 0f;
        float alpha;

        while (passedTime < fadeDuration)
        {
            passedTime += Time.deltaTime;

            alpha = Mathf.Lerp(0f, 1f, passedTime / fadeDuration);

            if (!fadeIn) alpha = 1f - alpha;

            lineCol.a = alpha;
            lineMat.color = lineCol;

            yield return null;
        }

        alpha = fadeIn ? 1f : 0f;

        lineCol.a = alpha;
        lineMat.color = lineCol;
    }


    /// <summary>
    /// Resets the line positions to zero, zero which results in an invisible line.
    /// </summary>
    public void Reset()
    {
        lineRend.SetPositions(new Vector3[2] { Vector3.zero, Vector3.zero });
    }

    public void SetActive(bool state)
    {
        isActive = state;
    }

    /// <summary>
    /// Iterate through objects in the timeline to find the virtual twin and set it as reference.
    /// </summary>
    public void SetVirtualTwinTransform(VersionObject vo)
    {
        virtualTwin = vo.transform;
    }
}
