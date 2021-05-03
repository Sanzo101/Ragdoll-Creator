using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//LICENSE//
//using Zapsplat.com for sound effects//
public class SFXPlayer : MonoBehaviour
{
    public AudioSource rfootSound, lfootSound, FallingSound;
    bool Soundenabled=false;
    public GameObject Slave;
    // Update is called once per frame
    void Update()
    {
        Soundenabled = Slave.GetComponent<ConfigurableJoints>().isAlive;
    }
    private void RFootstepAudio()
    {
        if (Soundenabled)
            rfootSound.Play();
    }
    private void LFootstepAudio()
    {
        if (Soundenabled)
            lfootSound.Play();
    }
    public void FallingSFX()
    {
        FallingSound.Play();
    }

}
