using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Footsteps : MonoBehaviour
{
    public AudioClip[] footstepSounds;
    private AudioSource audioSource;
    public float footstepInterval; 
    private float nextFootstepTime = 0f;
    private AudioClip lastFootstepSound;
    private float ObjectX;
    private float ObjectY;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        lastFootstepSound = null;
        
    }

    private void Update()
    {
        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
        {
            if (Time.time >= nextFootstepTime)
            {
                PlayFootstepSound();
                nextFootstepTime = Time.time + footstepInterval;
            }
        }
    }

    void PlayFootstepSound()
    {
        AudioClip footstepSound;
        do
        {
            footstepSound = footstepSounds[Random.Range(0, footstepSounds.Length)];
        } while (footstepSound == lastFootstepSound);

        lastFootstepSound = footstepSound;
        audioSource.clip = footstepSound;
        AudioSource.PlayClipAtPoint(footstepSound,transform.position,1f);
    }
}
