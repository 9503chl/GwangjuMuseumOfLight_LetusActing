using System;
using UnityEngine;
using UnityEngine.Video;

namespace UnityEngine.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RawImage))]
    public class VideoViewer : MonoBehaviour
    {
        public VideoPlayer SourceVideoPlayer;
        public bool PlayOnEnable = true;
        public bool ShowBannerOnStop = false;
        public bool ResizeToFitVideo = false;
        public bool OverrideVideoSize = false;
        public int VideoWidth = 1280;
        public int VideoHeight = 720;
        public bool RenderAlphaChannel = false;

        [NonSerialized]
        private RawImage renderTarget;

        [NonSerialized]
        private Texture targetTexture;

        [NonSerialized]
        private Texture bannerTexture;

        [NonSerialized]
        private Color bannerColor;

        [NonSerialized]
        private bool playerStarted = false;

        [NonSerialized]
        private bool playerStopped = false;

        [NonSerialized]
        private RenderTexture renderTexture;

        private void Awake()
        {
            renderTarget = GetComponent<RawImage>();
            bannerTexture = renderTarget.texture;
            bannerColor = renderTarget.color;
            if (SourceVideoPlayer == null)
            {
                SourceVideoPlayer = GetComponent<VideoPlayer>();
            }
            if (SourceVideoPlayer != null)
            {
                if (SourceVideoPlayer.targetTexture != null)
                {
                    renderTarget.texture = SourceVideoPlayer.targetTexture;
                    renderTarget.color = Color.white;
                }
                else
                {
                    SourceVideoPlayer.renderMode = VideoRenderMode.RenderTexture;
                    SourceVideoPlayer.prepareCompleted += SourceVideoPlayer_PrepareCompleted;
                }
                SourceVideoPlayer.started += SourceVideoPlayer_Started;
            }
        }

        private void OnEnable()
        {
            if (PlayOnEnable && SourceVideoPlayer != null)
            {
                SourceVideoPlayer.Play();
            }
        }

        private void OnDisable()
        {
            if (PlayOnEnable && SourceVideoPlayer != null)
            {
                SourceVideoPlayer.Stop();
            }
        }

        private void Update()
        {
            if (ShowBannerOnStop && targetTexture != null && playerStarted)
            {
#if UNITY_2018_3_OR_NEWER
                if (!SourceVideoPlayer.isPlaying && !SourceVideoPlayer.isPaused)
#else
                if (!SourceVideoPlayer.isPlaying)
#endif
                {
                    if (!playerStopped)
                    {
                        renderTarget.texture = bannerTexture;
                        renderTarget.color = bannerColor;
                        if (ResizeToFitVideo)
                        {
                            renderTarget.SetNativeSize();
                        }
                        playerStopped = true;
                    }
                }
                else
                {
                    if (playerStopped)
                    {
                        renderTarget.texture = targetTexture;
                        renderTarget.color = Color.white;
                        if (ResizeToFitVideo)
                        {
                            renderTarget.SetNativeSize();
                        }
                        playerStopped = false;
                    }
                }
            }
        }

        private void OnDestroy()
        {
            if (renderTexture != null)
            {
                renderTexture.Release();
            }
        }

        private void SourceVideoPlayer_PrepareCompleted(VideoPlayer source)
        {
            if (source.renderMode == VideoRenderMode.RenderTexture)
            {
#if UNITY_2018_3_OR_NEWER
                int sourceWidth = (int)source.width;
                int sourceHeight = (int)source.height;
#else
                int sourceWidth = 0;
                int sourceHeight = 0;
#endif
                if (source.clip != null)
                {
                    sourceWidth = (int)source.clip.width;
                    sourceHeight = (int)source.clip.height;
                }
                int sourceDepth = 24;
                if (OverrideVideoSize)
                {
                    sourceWidth = VideoWidth;
                    sourceHeight = VideoHeight;
                }
                if (RenderAlphaChannel)
                {
                    sourceDepth = 32;
                }
                if (renderTexture == null)
                {
                    if (source.targetTexture != null)
                    {
                        targetTexture = source.targetTexture;
                    }
                    else
                    {
                        renderTexture = new RenderTexture(sourceWidth, sourceHeight, sourceDepth);
                        source.targetTexture = renderTexture;
                        targetTexture = renderTexture;
                    }
                    renderTarget.texture = targetTexture;
                    renderTarget.color = Color.white;
                    if (ResizeToFitVideo)
                    {
                        renderTarget.SetNativeSize();
                    }
                }
                else if (renderTexture.width != sourceWidth || renderTexture.height != sourceHeight || renderTexture.depth != sourceDepth)
                {
                    if (renderTexture != null)
                    {
                        renderTexture.Release();
                        renderTexture = null;
                    }
                    renderTexture = new RenderTexture(sourceWidth, sourceHeight, sourceDepth);
                    source.targetTexture = renderTexture;
                    targetTexture = renderTexture;
                    renderTarget.texture = targetTexture;
                    renderTarget.color = Color.white;
                    if (ResizeToFitVideo)
                    {
                        renderTarget.SetNativeSize();
                    }
                }
            }
        }

        private void SourceVideoPlayer_Started(VideoPlayer source)
        {
            playerStarted = true;
        }
    }
}
