using UnityEngine;

public class PlayBackground : MonoBehaviour
{
    public AudioSource backgroundAudio;
    public AudioClip backgroundMusic;

    void Start()
    {
        if (backgroundAudio != null && backgroundMusic != null)
        {
            backgroundAudio.clip = backgroundMusic;
            backgroundAudio.loop = true;
            backgroundAudio.Play();
        }
    }
}
