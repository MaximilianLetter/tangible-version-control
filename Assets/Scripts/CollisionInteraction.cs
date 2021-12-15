using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionInteraction : MonoBehaviour
{
    private TimelineManager timelineManager;
    private ComparisonManager comparisonManager;

    void Start()
    {
        timelineManager = AppManager.Instance.GetTimelineManager();
        comparisonManager = AppManager.Instance.GetComparisonManager();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (timelineManager.GetInPlacement())
        {
            return;
        }

        if (other.gameObject.CompareTag("VersionObjectInside"))
        {
            comparisonManager.StartComparison(gameObject, other.gameObject);
            return;
        }

        if (other.gameObject.CompareTag("VersionObjectArea"))
        {
            var vo = other.gameObject.GetComponent<VersionObject>();
            timelineManager.SetCloseInteraction(true, vo);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("BranchContainer"))
        {
            timelineManager.SetCloseInteraction(false);
        }
    }
}
