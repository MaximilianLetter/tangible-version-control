using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundRay : MonoBehaviour
{
    public float minDist = 0.008f;

    private LineRenderer line;
    private GameObject glow;

    void Start()
    {
        glow = transform.GetChild(0).gameObject;
        glow.SetActive(false);

        line = GetComponent<LineRenderer>();
        line.enabled = false;
    }

    public void DrawLine(Vector3 origin)
    {
        RaycastHit hit;

        if (Physics.Raycast(origin, transform.TransformDirection(-Vector3.up), out hit, Mathf.Infinity))
        {
            float dist = Vector3.Distance(origin, hit.point);
            if (dist < minDist)
            {
                return;
            }

            line.SetPositions(new Vector3[2] { origin, hit.point });
            line.enabled = true;

            glow.transform.SetPositionAndRotation(hit.point, hit.transform.rotation);
            glow.SetActive(true);
        }
    }

    public void HideLine()
    {
        line.enabled = false;
        glow.SetActive(false);
    }
}
