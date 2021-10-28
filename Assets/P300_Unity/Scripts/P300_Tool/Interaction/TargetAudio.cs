using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class TargetAudio : MonoBehaviour
{
    public AudioClip targetSound;
    public AudioClip nonTargetSound;

    private AudioSource audioSource;
    

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        //Extending capabilities to include audio and different 'events' on target flashes
        P300Events.current.OnTargetFlash += OnTargetFlash;
        P300Events.current.OnNonTargetFlash += OnNonTargetFlash;
    }


    private void OnTargetFlash()
    {
        audioSource.clip = targetSound;
        audioSource.Play();
    }

    private void OnNonTargetFlash()
    {
        audioSource.clip = nonTargetSound;
        audioSource.Play();
    }

}
