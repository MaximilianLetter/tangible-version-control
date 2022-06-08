using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomOnClick : MonoBehaviour
{
    public ExperimentManager expM;
    public string type;
    public bool variant;

    public void DoOnClick()
    {
        expM.SelectExperimentCondition(type, variant);
    }
}
