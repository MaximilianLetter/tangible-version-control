using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(ObjectParts))]
public class VersionObject : MonoBehaviour
{
    public bool virtualTwin;
    public string description;
    public string createdAt;
    public string createdBy;
    [HideInInspector]
    public string id;

    [Space(14)]
    //public Transform textBlock;

    private ObjectParts parts;
    //private TMP_Text textTitle;
    //private TMP_Text textDesc;

    private bool ready = false;

    IEnumerator Start()
    {
        //textTitle = textBlock.GetChild(0).GetComponent<TMP_Text>();
        //textDesc = textBlock.GetChild(1).GetComponent<TMP_Text>();
        //SetTextInformation();

        parts = GetComponent<ObjectParts>();

        while (true)
        {
            if (parts.IsReady()) break;

            yield return null;
        }

        //if (virtualTwin)
        //{
        //    ChangeTextColor(ComparisonManager.Instance.textHighlight);
        //}

        ready = true;
    }

    /// <summary>
    /// Replaces every material of the object with a placement material.
    /// </summary>
    /// <param name="mat">Material to display.</param>
    public void SetMaterial(Material mat)
    {
        parts.SetMaterial(mat);
    }

    /// <summary>
    /// Reset all materials to the default materials.
    /// </summary>
    public void ResetMaterial()
    {
        parts.ResetMaterial();
    }

    public bool IsReady()
    {
        return ready;
    }
}
