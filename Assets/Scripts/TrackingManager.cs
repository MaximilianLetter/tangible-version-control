using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackingManager : MonoBehaviour
{
    public AudioClip foundClip;
    public AudioClip lostClip;

    private AudioSource source;

    // Start is called before the first frame update
    void Start()
    {
        source = GetComponent<AudioSource>();
        source.enabled = true;
    }

    public void OnFound()
    {
        source.clip = foundClip;
        source.Play();
    }
    
    public void OnLost()
    {
        source.clip = lostClip;
        source.Play();
    }
}
