using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundData
{
    public float volumn;
    public float pitch;
    public AudioSource _AudioSource;

    public SoundData(AudioSource audioSource)
    {
        _AudioSource = audioSource;
        volumn = audioSource.volume;
        pitch = audioSource.pitch;
    }
}
