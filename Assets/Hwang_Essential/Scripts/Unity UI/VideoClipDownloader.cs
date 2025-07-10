using System;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

namespace UnityEngine.UI
{
    public class VideoClipDownloader : AssetDownloader
    {
        protected override Type assetType
        {
            get { return typeof(VideoClip); }
        }

        protected override Type[] componentTypes
        {
            get { return new Type[] { typeof(VideoPlayer) }; }
        }

        //public VideoClip videoClip
        //{
        //    get { return asset as VideoClip; }
        //}

        public string videoUrl
        {
            get { return asset != null ? (asset as BufferAsset).text : null; }
        }

#if UNITY_EDITOR
        public override GUIContent PreviewContent
        {
            get
            {
                if (VideoExists())
                {
                    Texture2D previewTexture = ResizeTexture(UnityEditor.AssetPreview.GetMiniTypeThumbnail(assetType), 64, 64);
                    return new GUIContent(previewTexture);
                }
                return null;
            }
        }

        private bool VideoExists()
        {
            string path = GetAssetPath();
            if (!string.IsNullOrEmpty(path))
            {
                switch (assetLocation)
                {
                    case AssetLocation.PersistentData:
                        return File.Exists(path);
                    case AssetLocation.StreamingAssets:
                        return !Application.isEditor || File.Exists(path);
                    case AssetLocation.PathOrUrl:
                        return path.StartsWith("http://") || path.StartsWith("https://") || File.Exists(path);
                }
            }
            return false;
        }

        private void Reset()
        {
            const string assetExtension = ".mp4";
            VideoPlayer videoPlayer = GetComponent<VideoPlayer>();
            if (videoPlayer != null && videoPlayer.clip != null)
            {
                FindPathFromAssets(videoPlayer.clip, assetExtension);
            }
            Load();
        }

        protected override UnityEngine.Object LoadAsset()
        {
            if (VideoExists())
            {
                string url = GetAssetURL();
                try
                {
                    Uri uri = new Uri(url);
                    BufferAsset bufferAsset = new BufferAsset(uri.OriginalString);
                    bufferAsset.name = Path.GetFileNameWithoutExtension(GetAssetPath());
                    return bufferAsset;
                }
                catch (Exception)
                {
#if !UNITY_EDITOR
                    Debug.LogWarning(string.Format("Invalid URL : ", url));
#endif
                }
            }
            return null;
        }
#endif

        protected override void ApplyAsset()
        {
            if (!string.IsNullOrEmpty(videoUrl))
            {
                VideoPlayer videoPlayer = GetComponent<VideoPlayer>();
                if (videoPlayer != null)
                {
                    videoPlayer.source = VideoSource.Url;
                    videoPlayer.url = videoUrl;
                    if (Application.isPlaying)
                    {
                        videoPlayer.Prepare();
                    }
                }
            }
        }
    }
}
