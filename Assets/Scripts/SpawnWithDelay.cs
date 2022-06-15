using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnWithDelay : MonoBehaviour
{
    [SerializeField]
    float delay;

    // Start is called before the first frame update
    void Start()
    {
        Invoke("InvokedFunc", delay);
        gameObject.SetActive(false);
    }

    void InvokedFunc()
    {
        gameObject.SetActive(true);
    }
}
