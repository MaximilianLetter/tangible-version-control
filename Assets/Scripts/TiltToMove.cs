using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TiltToMove : MonoBehaviour
{
    public float tiltThreshold;
    public float maxTilt;
    public float speed;

    private Transform branchContainer;
    private Transform trackedTransform;

    void Start()
    {
        branchContainer = AppManager.Instance.GetBranchContainer();
        trackedTransform = AppManager.Instance.GetTrackedTransform();
    }

    void OnTriggerStay(Collider other)
    {
        // Actually doesnt really matter
        if (!other.CompareTag("Branch")) return;

        // Get rotation of cube -- TODO later of tracked obj
        float rotZ = trackedTransform.eulerAngles.z;
       
        bool dirFlip = false;
        if (rotZ > 180)
        {
            rotZ = 360 - rotZ;
            dirFlip = true;
        }

        // Check if tilt of z direction is beyond threshold
        if (Mathf.Abs(rotZ) < tiltThreshold) return;

        float dist = Mathf.SmoothStep(0, speed * Time.deltaTime, (rotZ / maxTilt));

        branchContainer.transform.position += new Vector3(dist * (dirFlip ? -1 : 1), 0, 0);
    }
}
