using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(ObjectParts))]
public class VersionObject : MonoBehaviour
{
    public bool virtualTwin;
    public string title;
    public string description;
    public string createdAt;
    public string createdBy;

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

        parts.ToggleOutlines(false);

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

    /// <summary>
    /// Set the text to the specified information.
    /// </summary>
    //private void SetTextInformation()
    //{
    //    textTitle.text = title;
    //    textDesc.text = description + "\n\n" + createdAt + "\n" + createdBy;
    //}

    /// <summary>
    /// Change the color of the referenced text to the given color.
    /// </summary>
    /// <param name="col">The color the text should be in, either default or highlight color.</param>
    //public void ChangeTextColor(Color32 col)
    //{
    //    textTitle.color = col;
    //    textDesc.color = col;
    //}

    public bool IsReady()
    {
        return ready;
    }
}
