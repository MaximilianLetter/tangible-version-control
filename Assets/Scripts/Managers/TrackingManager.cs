using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class TrackingManager : MonoBehaviour
{
    public AudioClip foundClip;
    public AudioClip lostClip;

    private AudioSource source;

    private bool ready;

    // Start is called before the first frame update
    void Start()
    {
        source = GetComponent<AudioSource>();
        source.enabled = true;

        ready = true;
    }


    /// <summary>
    /// Called by Vuforia tracking system.
    /// </summary>
    public void OnFound()
    {
        source.clip = foundClip;
        source.Play();
    }

    /// <summary>
    /// Called by Vuforia tracking system.
    /// </summary>
    public void OnLost()
    {
        source.clip = lostClip;
        source.Play();
    }

    public bool IsReady()
    {
        return ready;
    }
}
