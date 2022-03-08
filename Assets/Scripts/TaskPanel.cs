using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskPanel : MonoBehaviour
{
    [SerializeField]
    private GameObject goBtn;
    [SerializeField]
    private Transform modelContainer;
    [SerializeField]
    private TMPro.TextMeshPro textContainer;

    public void StartExperiment()
    {
        goBtn.SetActive(false);
        AppManager.Instance.GetTimelineManager().ToggleDummyModels(true);
        AppManager.Instance.GetExperimentManager().SetExperimentRunning(true);
    }

    public void ResetTaskPanel()
    {
        goBtn.SetActive(true);

        // Destroy model of last searched for object
        if (modelContainer.childCount > 0)
        {
            var oldLookedForVersion = modelContainer.GetChild(0);
            Destroy(oldLookedForVersion.gameObject);
        }
    }

    public Transform GetModelContainer()
    {
        return modelContainer;
    }

    public void SetCounterText(int counter)
    {
        textContainer.text = counter.ToString() + ". trial";
    }
}
