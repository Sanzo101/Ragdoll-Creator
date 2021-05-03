using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFX : MonoBehaviour
{
    AudioSource ThisAudioSource;
    public AudioSource Swinging;
    // Start is called before the first frame update
    void Start()
    {
        ThisAudioSource = GetComponent<AudioSource>();     
    }
    public void PlayBombSound()
    {
        ThisAudioSource.Play();
    }
    public void PlayBodyFallSound()
    {
            ThisAudioSource.Play();
    }
    public void SwingingPendulum()
    {
        Swinging.Play();
    }
}
