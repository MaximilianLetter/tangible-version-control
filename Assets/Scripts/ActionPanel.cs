using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ActionPanel : MonoBehaviour
{
    // Text objects that can be populated
    public TMP_Text obj1_title;
    public TMP_Text obj1_desc;
    public TMP_Text obj2_title;
    public TMP_Text obj2_desc;
    public BoxCollider backPanel;

    // Button groups that are toggled based on the current comparison mode
    public GameObject sideBySideOptions;
    public GameObject overlayOptions;
    public GameObject differencesOptions;

    public Transform cancelBtnTransform;

    // Internal variables used for calculating the floating distance
    //public float floatingOffsetZ;
    //private float floatingOffset;
    //private float height;

    private Transform trackedObjTransform;

    private bool ready;

    void Start()
    {
        trackedObjTransform = GameObject.Find("TrackedContainer").transform;
        //floatingOffset = 0f;
        //height = backPanel.bounds.size.y;

        ready = true;
    }

    public bool IsReady()
    {
        return ready;
    }

    /// <summary>
    /// Sets the content of the information panel aswell as the floating distance to the tracked object.
    /// </summary>
    /// <param name="obj1">Static information about the movable object, most propably the virtual twin of the physical object.</param>
    /// <param name="obj2">Static information about the compared version.</param>
    /// <param name="dist">Distance between the two objects.</param>
    //public void SetContents(VersionObject obj1, VersionObject obj2, float dist)
    //{
    //    obj1_title.text = obj1.title;
    //    obj1_desc.text = obj1.description + "\n\n" + obj1.createdAt + "\n" + obj1.createdBy;

    //    obj2_title.text = obj2.title;
    //    obj2_desc.text = obj2.description + "\n\n" + obj2.createdAt + "\n" + obj2.createdBy;

    //    //floatingOffset = dist; // + (height / 2);
    //    transform.localPosition = new Vector3(0, -dist / 2, transform.localPosition.z);

    //    SetOptions();
    //}

    /// <summary>
    /// Activates the fitting options based on the active comparison mode.
    /// </summary>
    public void SetOptions()
    {
        ComparisonMode mode = ComparisonManager.Instance.mode;

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
