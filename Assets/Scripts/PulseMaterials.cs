using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulseMaterials : MonoBehaviour
{
    public Material subtracted, added;
    public float pulseCadence;

    private float passedTime;
    private bool invertDirection;

    private void Start()
    {
        passedTime = 0.0f;
        invertDirection = false;
    }

    void Update()
    {
        passedTime += Time.deltaTime;
        if (passedTime >= pulseCadence)
        {
            passedTime = 0f;
            invertDirection = !invertDirection;
            // or hold in one state
            return;
        }

        float alpha = invertDirection ? Mathf.SmoothStep(0f, 1.0f, passedTime / pulseCadence) : Mathf.SmoothStep(1.0f, 0f, passedTime / pulseCadence);

        var subColor = subtracted.color;
        subtracted.color = new Color(subColor.r, subColor.g, subColor.b, alpha);

        var addColor = added.color;
        added.color = new Color(addColor.r, addColor.g, addColor.b, alpha);
    }
}
