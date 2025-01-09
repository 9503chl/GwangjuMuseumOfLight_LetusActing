using System;
using UnityEngine;
using UnityEngine.Video;

namespace UnityEngine.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(VideoPlayer))]
    public class TimeScaledVideo : MonoBehaviour
    {
        [NonSerialized]
        private VideoPlayer videoPlayer;

        [NonSerialized]
        private float timeScale;

        private void OnEnable()
        {
            timeScale = Time.timeScale;
            videoPlayer = GetComponent<VideoPlayer>();
            if (videoPlayer.canSetPlaybackSpeed)
            {
                //videoPlayer.skipOnDrop = true;
                videoPlayer.playbackSpeed = timeScale;
            }
            else
            {
                Debug.LogWarning("Cannot set playback speed of this video!");
            }
        }

        private void Update()
        {
            if (timeScale != Time.timeScale)
            {
                timeScale = Time.timeScale;
                if (videoPlayer.canSetPlaybackSpeed)
                {
                    //videoPlayer.skipOnDrop = true;
                    videoPlayer.playbackSpeed = timeScale;
                }
            }
        }
    }
}
