using System;
using UnityEngine;

namespace UnityEngine.UI
{
    public class AudioClipLoader : AssetLoader
    {
        protected override Type assetType
        {
            get { return typeof(AudioClip); }
        }

        protected override Type[] componentTypes
        {
            get { return new Type[] { typeof(AudioSource) }; }
        }

        public AudioClip audioClip
        {
            get { return asset as AudioClip; }
        }

#if UNITY_EDITOR
        private void Reset()
        {
            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource != null)
            {
                FindPathFromResources(audioSource.clip);
            }
            Load();
        }
#endif

        protected override void ApplyAsset()
        {
            if (audioClip != null)
            {
                AudioSource audioSource = GetComponent<AudioSource>();
                if (audioSource != null)
                {
                    audioSource.clip = audioClip;
                }
            }
        }
    }
}
