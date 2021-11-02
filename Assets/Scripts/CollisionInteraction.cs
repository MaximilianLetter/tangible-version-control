using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionInteraction : MonoBehaviour
{
    private TimelineManager timelineManager;

    void Start()
    {
        timelineManager = GameObject.Find("TimelineManager").GetComponent<TimelineManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (timelineManager.GetInPlacement())
        {
            return;
        }

        if (other.gameObject.CompareTag("VersionObject"))
        {
            AppManager.Instance.GetComparisonManager().StartComparison(gameObject, other.gameObject);
        }
    }
}
