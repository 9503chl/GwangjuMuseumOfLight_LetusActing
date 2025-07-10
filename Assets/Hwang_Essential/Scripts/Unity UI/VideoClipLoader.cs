using System;
using UnityEngine;
using UnityEngine.Video;

namespace UnityEngine.UI
{
    public class VideoClipLoader : AssetLoader
    {
        protected override Type assetType
        {
            get { return typeof(VideoClip); }
        }

        protected override Type[] componentTypes
        {
            get { return new Type[] { typeof(VideoPlayer) }; }
        }

        public VideoClip videoClip
        {
            get { return asset as VideoClip; }
        }

#if UNITY_EDITOR
        private void Reset()
        {
            VideoPlayer videoPlayer = GetComponent<VideoPlayer>();
            if (videoPlayer != null)
            {
                FindPathFromResources(videoPlayer.clip);
            }
            Load();
        }
#endif

        protected override void ApplyAsset()
        {
            if (videoClip != null)
            {
                VideoPlayer videoPlayer = GetComponent<VideoPlayer>();
                if (videoPlayer != null)
                {
                    videoPlayer.source = VideoSource.VideoClip;
                    videoPlayer.clip = videoClip;
                    if (Application.isPlaying)
                    {
                        videoPlayer.Prepare();
                    }
                }
            }
        }
    }
}
