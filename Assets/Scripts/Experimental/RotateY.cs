using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateY : MonoBehaviour
{
    public float speed = 20;

    // Update is called once per frame
    void Update()
    {
        Vector3 rotEuler = new Vector3(0, speed * Time.deltaTime, 0);
        Vector3 newRot = transform.rotation.eulerAngles + rotEuler;
        transform.rotation = Quaternion.Euler(newRot);
    }
}
