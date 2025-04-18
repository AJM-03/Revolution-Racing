using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIToggle : MonoBehaviour
{
    public bool music;

    public void Toggle()
    {
        if (music) SoundManager.Instance.MusicToggle();
    }
}
