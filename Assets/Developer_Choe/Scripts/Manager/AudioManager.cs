using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : PivotalManager
{
    private static List<AudioSource> nonAnnounceList = new List<AudioSource>();

    private static AudioSource[] AnnounceSounds;

    public override void OnAwake()
    {
        base.OnAwake();

        AudioSource[] audioSources = GetComponentsInChildren<AudioSource>();

        nonAnnounceList.AddRange(audioSources);

        for (int i = 0; i < AnnounceSounds.Length; i++)
        {
            nonAnnounceList.Remove(AnnounceSounds[i]);
        }
    }

    public static void AnnouncePlay(float time)
    {
        for(int i = 0; i< nonAnnounceList.Count; i++)
        {
            nonAnnounceList[i].volume = 0.1f;
            nonAnnounceList[i].DOFade(0.7f, time + 1).SetEase(Ease.InExpo);
        }
    }
}
