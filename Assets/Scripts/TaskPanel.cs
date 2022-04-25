using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskPanel : MonoBehaviour
{
    [SerializeField]
    private GameObject goBtn;
    [SerializeField]
    private GameObject understoodBtn;
    [SerializeField]
    private Transform modelContainer;
    [SerializeField]
    private TMPro.TextMeshPro textCounter;
    [SerializeField]
    private TMPro.TextMeshPro textDescription;
    [SerializeField]
    private TMPro.TextMeshPro textHeader;

    private ExperimentManager experimentManager;

    private void Start()
    {
        experimentManager = AppManager.Instance.GetExperimentManager();   
    }

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

    public void Go()
    {
        if (experimentManager.readyForExperiment)
        {
            goBtn.SetActive(false);
            experimentManager.SetExperimentRunning(true);
        }
        else
        {
            experimentManager.SetupExperiment();

            ToggleBetweenButtons(true);
        }
    }

    public Transform GetModelContainer()
    {
        return modelContainer;
    }

    public void SetTextCounter(int counter)
    {
        textCounter.text = counter.ToString() + ". trial";
    }

    public void SetTextDescription(string content)
    {
        textDescription.text = content;
    }

    public void SetTextHeader(string content)
    {
        textHeader.text = content;
    }

    public void SetStartInformation(ExperimentMode mode)
    {
        if (mode == ExperimentMode.Timeline)
        {
            SetTextHeader("Selection experiment");
            SetTextDescription("After the experiment starts, look for the shown version in the timeline. Select the correct version by moving the physical artifact in.");
        }
        else
        {
            SetTextHeader("Comparison experiment");
            SetTextDescription("After you start the experiment, the physical object will be in comparison with an alternative version.");
        }
    }

    public void ToggleBetweenButtons(bool state)
    {
        goBtn.SetActive(state);
        understoodBtn.SetActive(!state);
    }
}
