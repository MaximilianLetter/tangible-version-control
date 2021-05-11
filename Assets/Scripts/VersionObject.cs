using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ObjectParts))]
public class VersionObject : MonoBehaviour
{
    public bool virtualTwin;
    public string title;
    public string description;
    public string createdAt;
    public string createdBy;

    private ObjectParts parts;

    IEnumerator Start()
    {
        parts = GetComponent<ObjectParts>();

        while (true)
        {
            if (parts.IsReady()) break;

            yield return null;
        }

        if (!virtualTwin) parts.ToggleOutlines(false);
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
}
