using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InformationPanel : MonoBehaviour
{
    // Text objects that can be populated
    public TMP_Text obj1_title;
    public TMP_Text obj1_desc;
    public TMP_Text obj2_title;
    public TMP_Text obj2_desc;
    public BoxCollider backPanel;

    // Internal variables used for calculating the floating distance
    private float floatingOffset;
    private float height;

    private Transform trackedObjTransform;

    private bool ready;

    void Start()
    {
        trackedObjTransform = GameObject.Find("TrackedContainer").transform;
        floatingOffset = 0f;
        height = backPanel.bounds.size.y;

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
    public void SetContents(VersionObject obj1, VersionObject obj2, float dist)
    {
        obj1_title.text = obj1.title;
        obj1_desc.text = obj1.description + "\n\n" + obj1.createdAt + "\n" + obj1.createdBy;

        obj2_title.text = obj2.title;
        obj2_desc.text = obj2.description + "\n\n" + obj2.createdAt + "\n" + obj2.createdBy;

        floatingOffset = dist + (height / 2);
    }

    void Update()
    {
        transform.position = trackedObjTransform.position + (Vector3.down * floatingOffset);
        // TODO: Also bring the panel closer to the camera and meanwhile decrease its size
    }
}
