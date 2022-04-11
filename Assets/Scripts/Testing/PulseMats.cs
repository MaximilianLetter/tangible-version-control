using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulseMats : MonoBehaviour
{
    public float pulseCadence;
    public float pulseHold;

    public float maxAlpha = 1.0f;

    public Material diffMatAdded;
    public Material diffMatSubtracted;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(PulseMaterials());
    }

    /// <summary>
    /// Pulses the difference materials between visible and transparent.
    /// </summary>
    IEnumerator PulseMaterials()
    {
        // Initialize values
        float passedTime = 0.0f;
        bool invertDirection = false;

        while (true)
        {
            passedTime += Time.deltaTime;
            if (passedTime >= pulseCadence)
            {
                passedTime = 0f;
                invertDirection = !invertDirection;

                // Hold state
                yield return new WaitForSeconds(pulseHold);
            }

            float alpha = invertDirection ? Mathf.SmoothStep(0f, maxAlpha, passedTime / pulseCadence) : Mathf.SmoothStep(maxAlpha, 0f, passedTime / pulseCadence);

            var subColor = diffMatSubtracted.color;
            diffMatSubtracted.color = new Color(subColor.r, subColor.g, subColor.b, alpha);

            var addColor = diffMatAdded.color;
            diffMatAdded.color = new Color(addColor.r, addColor.g, addColor.b, alpha);

            yield return null;
        }
    }

    void OnDestroy()
    {
        StopAllCoroutines();
        Debug.Log("Reset material values");

        var subColor = diffMatSubtracted.color;
        diffMatSubtracted.color = new Color(subColor.r, subColor.g, subColor.b, maxAlpha);

        var addColor = diffMatAdded.color;
        diffMatAdded.color = new Color(addColor.r, addColor.g, addColor.b, maxAlpha);
    }
}
