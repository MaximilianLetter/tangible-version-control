using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities;

public class ObjectParts : MonoBehaviour
{
    private ComparisonManager comparisonManager;

    private MeshRenderer[] childRenderers;
    private Material[] childMats;
    private MeshOutline[] outlines;

    private bool pulseActive;
    private float pulseCadence;
    private float pulseHold;
    private GameObject[] pulseCloneObjs;

    private bool ready;

    void Start()
    {
        comparisonManager = AppManager.Instance.GetComparisonManager();

        CollectRenderersAndMaterials();

        foreach (var part in childRenderers)
        {
            part.gameObject.AddComponent<PreserveMaterial>();
        }

        ready = true;
    }

    /// <summary>
    /// Set up references to each part's renderer and get the default material.
    /// </summary>
    public void CollectRenderersAndMaterials(GameObject[] parts = null)
    {
        // This is the case for the comparison object only for collecting references while the old parts are being destroyed
        if (parts != null)
        {
            childRenderers = new MeshRenderer[parts.Length];
            for (var i = 0; i < parts.Length; i++)
            {
                childRenderers[i] = parts[i].GetComponent<MeshRenderer>();
            }
        }
        else
        {
            childRenderers = GetComponentsInChildren<MeshRenderer>();
            outlines = GetComponentsInChildren<MeshOutline>();
        }

        childMats = new Material[childRenderers.Length];
        for (int i = 0; i < childRenderers.Length; i++)
        {
            childMats[i] = childRenderers[i].material;
        }
    }

    /// <summary>
    /// Resets MeshRenderers, MeshOutlines, and stored materials.
    /// </summary>
    public void ResetRenderersAndMaterials()
    {
        outlines = new MeshOutline[0];
        childRenderers = new MeshRenderer[0];
        childMats = new Material[0];
    }

    /// <summary>
    /// Replaces the material of each part of the object with the given material.
    /// </summary>
    /// <param name="mat">Material to display.</param>
    public void SetMaterial(Material mat, GameObject[] parts = null)
    {
        // Special case to update just given parts of the object
        if (parts != null)
        {
            foreach (var part in parts)
            {
                part.GetComponent<MeshRenderer>().material = mat;
            }
            return;
        }

        // Default case
        foreach (var child in childRenderers)
        {
            if (child) child.material = mat;
        }
    }

    /// <summary>
    /// Reset the material of each part of the object to the default.
    /// </summary>
    /// <param name="getPreservedMaterial">Use the preserved material, this is currently only possible for the tracked object.</param>
    public void ResetMaterial(bool getPreservedMaterial = false)
    {
        if (getPreservedMaterial)
        {
            for (int i = 0; i < childRenderers.Length; i++)
            {
                if (!childRenderers[i]) continue;
                
                var preserveMat = childRenderers[i].GetComponent<PreserveMaterial>();
                if (preserveMat != null)
                {
                    childRenderers[i].material = preserveMat.GetBaseMat();
                }
                else
                {
                    childRenderers[i].material = childMats[i];
                }
            }
            return;
        }

        // In the default case, reset to the referenced materials
        for (int i = 0; i < childRenderers.Length; i++)
        {
            if (childRenderers[i]) childRenderers[i].material = childMats[i];
        }
    }

    /// <summary>
    /// Sets the outlines of the managed parts to the given material.
    /// </summary>
    /// <param name="mat">The material to display the outlines with.</param>
    public void SetOutlineMaterial(Material mat)
    {
        foreach (var line in outlines)
        {
            line.OutlineMaterial = mat;
        }
    }

    /// <summary>
    /// Toggle outlines on and off.
    /// </summary>
    /// <param name="state">True is on, false is off.</param>
    public void ToggleOutlines(bool state)
    {
        foreach (var line in outlines)
        {
            line.enabled = state;
        }
    }

    /// <summary>
    /// Starts the Coroutine of pulsing materials from transparent to solid.
    /// </summary>
    public void StartPulseParts(GameObject[] addedParts, GameObject[] modifiedParts)
    {
        CollectRenderersAndMaterials();

        GameObject[] specifiedParts = addedParts.Concat(modifiedParts).ToArray();

        foreach (var mat in childMats)
        {
            MaterialExtensions.ToFadeMode(mat);
        }

        pulseActive = true;
        pulseCadence = comparisonManager.pulseCadence;
        pulseHold = comparisonManager.pulseHold;

        // Clone all pulsing parts as phantom behind, so that outlines only become visible
        pulseCloneObjs = new GameObject[specifiedParts.Length];
        for (int i = 0; i < pulseCloneObjs.Length; i++)
        {
            var cloneObj = Instantiate(specifiedParts[i], transform);
            
            cloneObj.GetComponent<MeshOutline>().enabled = false;
            //cloneObj.GetComponent<MeshSmoother>().enabled = false;

            var rend = cloneObj.GetComponent<MeshRenderer>();
            rend.materials = new Material[1] { rend.material };

            pulseCloneObjs[i] = cloneObj;
        }
        SetMaterial(comparisonManager.phantomMat, pulseCloneObjs);

        StartCoroutine(PulseParts(specifiedParts));
        StartCoroutine(PulseOutlines(modifiedParts));
    }

    /// <summary>
    /// Coroutine of pulsing materials from transparent to solid.
    /// </summary>
    /// <returns></returns>
    private IEnumerator PulseParts(GameObject[] specifiedParts)
    {
        Material[] mats;
        if (specifiedParts == null)
        {
            mats = childMats;
        }
        else
        {
            mats = new Material[specifiedParts.Length];
            for (int i = 0; i < specifiedParts.Length; i++)
            {
                mats[i] = specifiedParts[i].GetComponent<MeshRenderer>().material;
            }
        }

        float passedTime = 0f;
        bool pulseDirection = false;

        while (pulseActive)
        {
            passedTime += Time.deltaTime;
            if (passedTime >= pulseCadence)
            {
                passedTime = 0f;
                pulseDirection = !pulseDirection;
                yield return new WaitForSeconds(pulseHold);
            }

            float alpha = pulseDirection ? (1.0f - Mathf.SmoothStep(0, 1, (passedTime / pulseCadence))) : Easing.Quartic.Out(passedTime / pulseCadence);

            foreach (var mat in mats)
            {
                Color col = mat.color;
                mat.color = new Color(col.r, col.g, col.b, alpha);
            }

            yield return null;
        }

        foreach (var mat in mats)
        {
            Color col = mat.color;
            mat.color = new Color(col.r, col.g, col.b, 1.0f);
        }
    }

    /// <summary>
    /// Coroutine of transitioning an outline between red and green.
    /// </summary>
    /// <param name="specifiedParts"></param>
    /// <returns></returns>
    IEnumerator PulseOutlines(GameObject[] specifiedParts)
    {
        Material[] outlineMats;
        Color32 col1 = comparisonManager.red;
        Color32 col2 = comparisonManager.green;

        outlineMats = new Material[specifiedParts.Length];
        for (int i = 0; i < specifiedParts.Length; i++)
        {
            outlineMats[i] = specifiedParts[i].GetComponent<MeshOutline>().OutlineMaterial;
        }

        float passedTime = 0f;
        bool pulseDirection = false;

        while (pulseActive)
        {
            passedTime += Time.deltaTime;
            if (passedTime >= pulseCadence)
            {
                passedTime = 0f;
                pulseDirection = !pulseDirection;
                yield return new WaitForSeconds(pulseHold);
            }

            Color32 outlCol = Color32.Lerp(col1, col2, pulseDirection ? (1.0f - (passedTime / pulseCadence)) : (passedTime / pulseCadence));

            foreach (var mat in outlineMats)
            {
                mat.color = outlCol;
            }

            yield return null;
        }
    }

    /// <summary>
    /// Stops the Coroutine of pulsing materials from transparent to solid.
    /// </summary>
    public void StopPulseParts()
    {
        // Quick escape when the function is called without an action necessary
        if (!pulseActive) return;

        foreach (var mat in childMats)
        {
            MaterialExtensions.ToOpaqueMode(mat);
        }

        StopAllCoroutines();

        foreach (var obj in pulseCloneObjs)
        {
            Destroy(obj);
        }
        pulseCloneObjs = new GameObject[0];

        pulseActive = false;
    }

    public bool IsReady()
    {
        return ready;
    }
}
