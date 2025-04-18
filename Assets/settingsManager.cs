using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class settingsManager : MonoBehaviour
{
    public Slider volume;

    public void Start()
    {
        volume.value = 1;
        volume.value = PlayerPrefs.GetFloat("volume");
    }

    public void ChangeMasterVolume(float value)
    {
        SoundManager.Instance.ChangeMasterVolume(value);
    }

    public void MusicToggle()
    {
        SoundManager.Instance.MusicToggle();
    }

    public void ResetProgress()
    {
        SaveManager.Instance.DeleteSave();
    }
}
