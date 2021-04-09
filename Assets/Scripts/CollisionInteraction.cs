using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionInteraction : MonoBehaviour
{

    private PlacementManager placementManager;
    private ComparisonManager comparisonManager;

    void Start()
    {
        placementManager = GameObject.Find("PlacementManager").GetComponent<PlacementManager>();
        comparisonManager = GameObject.Find("ComparisonManager").GetComponent<ComparisonManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (placementManager.GetInPlacement())
        {
            return;
        }

        if (other.gameObject.CompareTag("VersionObject"))
        {
            Debug.Log("Start comparison");
            comparisonManager.StartComparison(gameObject, other.gameObject);
        }
    }
}
