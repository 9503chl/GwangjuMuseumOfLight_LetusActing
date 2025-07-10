using System;
using UnityEngine;

namespace UnityEngine.UI
{
    public class AudioClipDownloader : AssetDownloader
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
            const string assetExtension = ".mp3";
            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource != null)
            {
                FindPathFromAssets(audioSource.clip, assetExtension);
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
