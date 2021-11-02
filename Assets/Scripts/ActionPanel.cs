using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ActionPanel : MonoBehaviour
{
    // Button groups that are toggled based on the current comparison mode
    public GameObject sideBySideOptions;
    public GameObject overlayOptions;
    public GameObject differencesOptions;

    public Transform cancelBtnTransform;

    /// <summary>
    /// Activates the fitting sub visualizations based on the active comparison mode.
    /// </summary>
    public void SetOptions()
    {
        ComparisonMode mode = AppManager.Instance.GetComparisonManager().mode;

        // Make sure the other options are disabled
        sideBySideOptions.SetActive(false);
        overlayOptions.SetActive(false);
        differencesOptions.SetActive(false);

        // Set active based on current comparison mode
        if (mode == ComparisonMode.SideBySide)
        {
            sideBySideOptions.SetActive(true);
        }
        else if (mode == ComparisonMode.Overlay)
        {
            overlayOptions.SetActive(true);
        }
        else if (mode == ComparisonMode.Differences)
        {
            differencesOptions.SetActive(true);
        }
    }

    /// <summary>
    /// Move the cancel comparison button to align with the current active index.
    /// </summary>
    /// <param name="index">The index of the active radio button.</param>
    public void MoveCancelButton(int index)
    {
        cancelBtnTransform.localRotation = Quaternion.identity;

        switch (index)
        {
            case 0:
                cancelBtnTransform.localPosition = new Vector3(-0.05f, 0.05f, 0f);
                break;
            case 1:
                cancelBtnTransform.localPosition = new Vector3(-0.05f, 0f, 0f);
                break;
            case 2:
                cancelBtnTransform.localPosition = new Vector3(-0.05f, -0.05f, 0f);
                break;
            default:
                cancelBtnTransform.localPosition = new Vector3(-0.05f, 0.05f, 0f);
                break;
        }
    }
}
