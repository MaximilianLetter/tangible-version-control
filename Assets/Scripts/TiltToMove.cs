using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TiltToMove : MonoBehaviour
{
    public int minBranchesZ = 2;
    public int minVersionsX = 6;

    public float tiltThresholdHorizontal;
    public float tiltThresholdVertical;
    public float maxTilt;
    public float speedHorizontal;
    public float speedVertical;

    private Transform branchContainer;
    private Transform trackedTransform;

    private bool xScrolling = false;
    private bool zScrolling = false;

    void Start()
    {
        branchContainer = AppManager.Instance.GetBranchContainer();
        trackedTransform = AppManager.Instance.GetTrackedTransform();
    }

    public void Initialize()
    {
        // Check if moving branches in Z is required
        int amountOfBranches = branchContainer.childCount - 1; // branchContainer holds one more object besides the branches
        if (amountOfBranches >= minBranchesZ)
        {
            zScrolling = true;
        }

        // NOTE: scrolling in branches direction disabled for now
        zScrolling = false;

        // Check if scrolling versions in X is required
        foreach (Transform branch in branchContainer)
        {
            if (branch.childCount >= minVersionsX)
            {
                xScrolling = true;
                break;
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
#if UNITY_EDITOR
        if (AppManager.Instance.GetComparisonManager().usePhysical) return;
#endif
        // Actually doesnt really matter
        if (!other.CompareTag("BranchContainer")) return;

        // Calculate tilt in relation to the branch
        Vector3 rotObj = trackedTransform.localRotation.eulerAngles;

        Vector3 dirObjZ = trackedTransform.localRotation * Vector3.forward;
        Vector3 dirObjX = trackedTransform.localRotation * Vector3.right;

        //Vector3 dirBranch = branchContainer.localRotation * Vector3.forward;
        Vector3 dirBranch = branchContainer.forward;

        // Calculate alignment between the two directions
        float alignmentX = Vector3.Dot(dirBranch, dirObjX);
        float alignmentZ = Vector3.Dot(dirBranch, dirObjZ);

        float tiltHorizontal;
        float tiltVertical;

        bool dirFlipHorizontal = false;
        bool dirFlipVertical = false;

        if (Mathf.Abs(alignmentX) > Mathf.Abs(alignmentZ))
        {
            tiltHorizontal = rotObj.x;
            tiltVertical = rotObj.z;

            if (alignmentX < 0)
            {
                dirFlipHorizontal = !dirFlipHorizontal;
                dirFlipVertical = false;
            }
        }
        else
        {
            tiltHorizontal = rotObj.z;
            tiltVertical = rotObj.x;

            if (alignmentZ < 0)
            {
                dirFlipHorizontal = !dirFlipHorizontal;
                dirFlipVertical = true;
            }
        }

        if (xScrolling)
        {
            // Check if tilt is to the left or to the right
            if (tiltHorizontal > 180f)
            {
                tiltHorizontal = 360f - tiltHorizontal;
                dirFlipHorizontal = !dirFlipHorizontal;
            }

            //tiltHorizontal *= alignmentHorizontal;

            // Check if tilt is beyond threshold
            if (tiltHorizontal > tiltThresholdHorizontal)
            {
                float dist = Mathf.SmoothStep(0, maxTilt, (tiltHorizontal / maxTilt) * speedHorizontal * Time.deltaTime);
                branchContainer.localPosition += new Vector3(dist * (dirFlipHorizontal ? -1 : 1), 0, 0);
            }
        }

        if (zScrolling)
        {
            // Check if tilt is towards back or front
            if (tiltVertical > 180f)
            {
                tiltVertical = 360f - tiltVertical;
                dirFlipVertical = !dirFlipVertical;
            }

            //tiltVertical *= alignmentVertical;

            if (tiltVertical > tiltThresholdVertical)
            {
                float dist = Mathf.SmoothStep(0, maxTilt, (tiltVertical / maxTilt) * speedVertical * Time.deltaTime);
                branchContainer.localPosition += new Vector3(0, 0, dist * (dirFlipVertical ? 1 : -1));
            }
        }
    }
}
