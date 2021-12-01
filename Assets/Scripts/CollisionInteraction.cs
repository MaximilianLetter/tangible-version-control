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

        if (other.gameObject.CompareTag("VersionObject"))
        {
            comparisonManager.StartComparison(gameObject, other.gameObject);
        }

        if (other.gameObject.CompareTag("Branch"))
        {
            var branch = other.gameObject.GetComponent<Branch>();
            timelineManager.SetActiveBranch(branch.index, true);
            branch.SetHighlightActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Branch"))
        {
            var branch = other.gameObject.GetComponent<Branch>();
            branch.SetHighlightActive(false);
            timelineManager.SetActiveBranch(99);
        }
    }
}
