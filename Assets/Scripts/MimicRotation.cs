using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MimicRotation : MonoBehaviour
{
    [SerializeField] private Transform refObj;

    // Update is called once per frame
    void Update()
    {
        transform.rotation = refObj.rotation;
    }
}
