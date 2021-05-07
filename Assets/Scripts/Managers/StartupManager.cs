using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartupManager : MonoBehaviour
{
    public GameObject startupPanel;

    public GameObject[] sceneObjects;

    private PlacementManager placementManager;
    private InformationPanel informationPanel;
    private ComparisonObject comparisonObject;

    IEnumerator Start()
    {
        // Wait one frame for other objects to instantiate
        yield return null;

        // Get necessary references
        placementManager = FindObjectOfType<PlacementManager>();
        informationPanel = FindObjectOfType<InformationPanel>();
        comparisonObject = FindObjectOfType<ComparisonObject>();

        // Wait for other objects getting ready
        while (true)
        {
            if (placementManager.IsReady() &&
                informationPanel.IsReady() &&
                ComparisonManager.Instance.IsReady() &&
                comparisonObject.IsReady()
                )
            {
                break;
            }
            yield return null;
        }

        // Setup the scene
        foreach (var obj in sceneObjects)
        {
            obj.SetActive(false);
        }

        startupPanel.SetActive(true);
    }
}
