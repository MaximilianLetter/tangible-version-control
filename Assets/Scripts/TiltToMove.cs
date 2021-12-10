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


        // Calculate tilt in relation to the branch
        Vector3 rotObj = trackedTransform.localRotation.eulerAngles;
        Vector3 dirObjZ = trackedTransform.localRotation * Vector3.forward;
        Vector3 dirObjX = trackedTransform.localRotation * Vector3.right;

        //Vector3 dirBranch = branchContainer.localRotation * Vector3.forward;
        Vector3 dirBranch = branchContainer.forward;


        float alignmentX = Mathf.Abs(Vector3.Dot(dirBranch, dirObjX));
        float alignmentZ = Mathf.Abs(Vector3.Dot(dirBranch, dirObjZ));

        //Debug.Log("alignment X : " + alignmentX);
        //Debug.Log("alignment Z : " + alignmentZ);

        float tilt;
        float alignment;

        if (Mathf.Abs(alignmentX) > Mathf.Abs(alignmentZ))
        {
            alignmentX -= alignmentZ;
            // TODO alignmentX and Z could be subtracted from each other to promote alignment
            tilt = rotObj.x;
            alignment = alignmentX;
            //Debug.Log("TILT-X: " + tilt);
            //Debug.Log("rot: " + rotObj.x);
        }
        else
        {
            alignmentZ -= alignmentX;
            tilt = rotObj.z;
            alignment = alignmentZ;
            //Debug.Log("TILT-Z: " + tilt);
            //Debug.Log("rot: " + rotObj.z);
        }

        // if tilt < 180 but high
        bool dirFlip = true;

        if (tilt > 180f) // 360 -> 300 -> 40 ist zu hoch, reduktion durch alignment erzeugt das problem
        {
            tilt = 360f - tilt;
            dirFlip = false;
        }

        tilt *= alignment;

        //Debug.Log("TILT After: " + tilt);

        // Check if tilt is beyond threshold
        if (Mathf.Abs(tilt) < tiltThreshold) return;

        float dist = Mathf.SmoothStep(0, speed * Time.deltaTime, (tilt / maxTilt));

        branchContainer.localPosition += new Vector3(dist * (dirFlip ? -1 : 1), 0, 0);
    }


}
