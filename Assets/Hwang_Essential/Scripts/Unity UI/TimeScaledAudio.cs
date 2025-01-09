using System;
using UnityEngine;

namespace UnityEngine.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AudioSource))]
    public sealed class TimeScaledAudio : MonoBehaviour
    {
        [NonSerialized]
        private AudioSource audioSource;

        [NonSerialized]
        private float timeScale;

        private void OnEnable()
        {
            timeScale = Time.timeScale;
            audioSource = GetComponent<AudioSource>();
            audioSource.pitch = timeScale;
        }

        private void Update()
        {
            if (timeScale != Time.timeScale)
            {
                timeScale = Time.timeScale;
                audioSource.pitch = timeScale;
            }
        }
    }
}
