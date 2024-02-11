using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManger : MonoBehaviour
{
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void PlaySoundEffect(AudioClip soundEffect, float volume = 1.0f)
    {
        if (audioSource != null && soundEffect != null)
        {
            // Set the volume of the AudioSource
            audioSource.volume = Mathf.Clamp01(volume);

            // Play the sound effect
            audioSource.PlayOneShot(soundEffect);
        }
    }
}
