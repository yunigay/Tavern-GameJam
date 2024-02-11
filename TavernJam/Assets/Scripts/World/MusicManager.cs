using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioClip backgroundMusic; // Assign your background music in the Unity Editor
    private AudioSource audioSource;

    private void Awake()
    {
        // Ensure that there is only one instance of MusicManager across scenes
        int numMusicManagers = FindObjectsOfType<MusicManager>().Length;
        if (numMusicManagers > 1)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // Set up audio source properties
        audioSource.clip = backgroundMusic;
        audioSource.loop = true;

        // Start playing the background music
        audioSource.Play();
    }
    public void SetVolume(float volume)
    {
        // Clamp the volume value to the valid range [0, 1]
        volume = Mathf.Clamp01(volume);

        // Set the volume of the AudioSource
        audioSource.volume = volume;
    }
}
