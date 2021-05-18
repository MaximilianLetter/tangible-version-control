using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionInteraction : MonoBehaviour
{
    private PlacementManager placementManager;

    void Start()
    {
        placementManager = GameObject.Find("PlacementManager").GetComponent<PlacementManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (placementManager.GetInPlacement())
        {
            return;
        }

        if (other.gameObject.CompareTag("VersionObject"))
        {
            Debug.Log("Trigger comparison script");
            ComparisonManager.Instance.StartComparison(gameObject, other.gameObject);
        }
    }
}
