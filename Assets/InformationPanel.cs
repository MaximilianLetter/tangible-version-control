using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InformationPanel : MonoBehaviour
{
    // try to reduce getComponent calls in comparison manager, directly talk to verisonObjects maybe
    // let panel float below the phys object (position of tracked object, but radial view)
    public TMP_Text obj1_title;
    public TMP_Text obj1_desc;
    public TMP_Text obj2_title;
    public TMP_Text obj2_desc;
    public BoxCollider backPanel;

    private float floatingOffset;
    private float height;

    private Transform trackedObjTransform;

    void Start()
    {
        trackedObjTransform = GameObject.Find("TrackedContainer").transform;
        floatingOffset = 0f;
        height = backPanel.bounds.size.y;
    }

    public void SetContents(VersionObject obj1, VersionObject obj2, float dist)
    {
        obj1_title.text = obj1.title;
        obj1_desc.text = obj1.description + "\n\n" + obj1.createdAt + "\n" + obj1.createdBy;

        obj2_title.text = obj2.title;
        obj2_desc.text = obj2.description + "\n\n" + obj2.createdAt + "\n" + obj2.createdBy;

        floatingOffset = dist + height;
    }

    void Update()
    {
        transform.position = trackedObjTransform.position - (trackedObjTransform.up * floatingOffset);
    }
}
