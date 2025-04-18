using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    public AudioSource effectSource;
    public bool muteMusic;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);

        if (SaveManager.Instance != null)
            muteMusic = SaveManager.Instance.muteMusic;
    }


    public void PlaySound(AudioClip clip, float volume)
    {
        if (clip != null)
        {
            effectSource.volume = volume;
            effectSource.PlayOneShot(clip);
        }
    }

    public void ChangeMasterVolume(float value)
    {
        AudioListener.volume = value;
        SaveManager.Instance.SaveSettings();
    }

    public void MusicToggle()
    {
        muteMusic = !muteMusic;
        SaveManager.Instance.SaveSettings();
    }
}
