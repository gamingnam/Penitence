using System.Collections;
using UnityEngine;

public class Footsteps : MonoBehaviour
{
    public AudioClip[] footstepSounds;
    public float footstepInterval = 0.5f;

    public AudioSource audioSourcePrefab; // Prefab to use for pooling

    private ObjectPooler<AudioSource> audioPool;
    private float nextFootstepTime = 0f;
    private AudioClip lastFootstepSound;

    private void Start()
    {
        // Initialize the object pool for AudioSources
        audioPool = new ObjectPooler<AudioSource>(audioSourcePrefab, footstepSounds.Length, transform);
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
        AudioSource audioSource = audioPool.Get(); // Get an AudioSource from the pool

        AudioClip footstepSound;
        do
        {
            footstepSound = footstepSounds[Random.Range(0, footstepSounds.Length)];
        }
        while (footstepSound == lastFootstepSound);

        lastFootstepSound = footstepSound;
        audioSource.clip = footstepSound;
        audioSource.Play();

        // Return the AudioSource to the pool after it finishes playing
        StartCoroutine(ReturnToPool(audioSource, footstepSound.length));
    }

    IEnumerator ReturnToPool(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);
        audioPool.ReturnToPool(source);
    }
}
