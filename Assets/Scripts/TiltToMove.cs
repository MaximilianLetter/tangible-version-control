using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TiltToMove : MonoBehaviour
{
    public float tiltThresholdHorizontal;
    public float tiltThresholdVertical;
    public float maxTilt;
    public float speedHorizontal;
    public float speedVertical;

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

        // Calculate tilt in relation to the branch
        Vector3 rotObj = trackedTransform.localRotation.eulerAngles;

        Vector3 dirObjZ = trackedTransform.localRotation * Vector3.forward;
        Vector3 dirObjX = trackedTransform.localRotation * Vector3.right;

        //Vector3 dirBranch = branchContainer.localRotation * Vector3.forward;
        Vector3 dirBranch = branchContainer.forward;

        // Calculate alignment between the two directions
        float alignmentX = Mathf.Abs(Vector3.Dot(dirBranch, dirObjX));
        float alignmentZ = Mathf.Abs(Vector3.Dot(dirBranch, dirObjZ));

        float tiltHorizontal;
        float tiltVertical;
        float alignmentHorizontal;
        float alignmentVertical;

        if (Mathf.Abs(alignmentX) > Mathf.Abs(alignmentZ))
        {
            tiltHorizontal = rotObj.x;
            alignmentHorizontal = alignmentX;

            tiltVertical = rotObj.z;
            alignmentVertical = 1 - alignmentZ;
        }
        else
        {
            tiltHorizontal = rotObj.z;
            alignmentHorizontal = alignmentZ;

            tiltVertical = rotObj.x;
            alignmentVertical = 1 - alignmentX;
        }

        bool dirFlipHorizontal = true;

        if (tiltHorizontal > 180f)
        {
            tiltHorizontal = 360f - tiltHorizontal;
            dirFlipHorizontal = false;
        }

        tiltHorizontal *= alignmentHorizontal;

        // Check if tilt is beyond threshold
        if (tiltHorizontal > tiltThresholdHorizontal)
        {
            float dist = Mathf.SmoothStep(0, maxTilt, (tiltHorizontal / maxTilt) * speedHorizontal * Time.deltaTime);
            branchContainer.localPosition += new Vector3(dist * (dirFlipHorizontal ? -1 : 1), 0, 0);

            return; // Horizontal scrolling has priority
        }

        bool dirFlipVertical = false;

        if (tiltVertical > 180f)
        {
            tiltVertical = 360f - tiltVertical;
            dirFlipVertical = true;
        }

        tiltVertical *= alignmentVertical;

        if (tiltVertical > tiltThresholdVertical)
        {
            Debug.Log("theshold reached");
            float dist = Mathf.SmoothStep(0, maxTilt, (tiltVertical / maxTilt) * speedVertical * Time.deltaTime);
            branchContainer.localPosition += new Vector3(0, 0, dist * (dirFlipVertical ? -1 : 1));
        }
    }
}
