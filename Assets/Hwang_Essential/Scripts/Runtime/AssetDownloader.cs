using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace UnityEngine.UI
{
    public abstract class AssetDownloader : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Asset location to download")]
        protected AssetLocation assetLocation = AssetLocation.PathOrUrl;
        public AssetLocation AssetLocation
        {
            get { return assetLocation; }
            set { if (assetLocation != value) { assetLocation = value; } }
        }

        [SerializeField]
        [Tooltip("Relative path to asset location, absolute path or URL")]
        protected string pathOrUrl;
        public string PathOrUrl
        {
            get { return pathOrUrl; }
            set { if (pathOrUrl != value) { pathOrUrl = value; } }
        }

        [NonSerialized]
        protected Coroutine savingRoutine;
        public bool IsSaving
        {
            get { return savingRoutine != null; }
        }

        [NonSerialized]
        protected bool wasSaved;
        public bool WasSaved
        {
            get { return wasSaved; }
        }

        [NonSerialized]
        protected Coroutine loadingRoutine;
        public bool IsLoading
        {
            get { return loadingRoutine != null; }
        }

        [NonSerialized]
        protected bool wasLoaded;
        public bool WasLoaded
        {
            get { return wasLoaded; }
        }

        [NonSerialized]
        protected UnityEngine.Object _asset;
        protected UnityEngine.Object asset
        {
            get { return wasLoaded ? _asset : null; }
        }

        public event UnityAction<string, float> OnSaving;
        public event UnityAction<string, bool> OnSaved;

        public event UnityAction<float> OnLoading;
        public event UnityAction<bool> OnLoaded;

        protected abstract Type assetType { get; }

        protected abstract Type[] componentTypes { get; }

#if UNITY_EDITOR
        [NonSerialized]
        private Texture2D resizedTexture;

        public virtual GUIContent PreviewContent
        {
            get
            {
                if (wasLoaded && _asset != null)
                {
                    Texture2D previewTexture = UnityEditor.AssetPreview.GetAssetPreview(_asset);
                    if (previewTexture == null || assetType == typeof(Texture))
                    {
                        if (assetType == typeof(Sprite))
                        {
                            previewTexture = UnityEditor.AssetPreview.GetMiniThumbnail((_asset as Sprite).texture);
                        }
                        else
                        {
                            previewTexture = UnityEditor.AssetPreview.GetMiniThumbnail(_asset);
                        }
                    }
                    if (previewTexture != null)
                    {
                        previewTexture = ResizeTexture(previewTexture, 128, 128);
                    }
                    else
                    {
                        previewTexture = ResizeTexture(UnityEditor.AssetPreview.GetMiniTypeThumbnail(assetType), 64, 64);
                    }
                    return new GUIContent(previewTexture);
                }
                return null;
            }
        }

        protected Texture2D ResizeTexture(Texture2D texture, int targetWidth, int targetHeight)
        {
            if (texture != null)
            {
                if (texture.width > targetWidth || texture.height > targetHeight)
                {
                    Texture2D clonedTexture = Texture2DUtility.Duplicate(texture);
                    Texture2D result = Texture2DUtility.ResizeTo(clonedTexture, targetWidth, targetHeight, true);
                    DestroyImmediate(clonedTexture);
                    if (resizedTexture != null)
                    {
                        DestroyImmediate(resizedTexture);
                    }
                    resizedTexture = result;
                    return result;
                }
                return texture;
            }
            return null;
        }

        public bool HasRequiredComponent()
        {
            if (componentTypes != null)
            {
                for (int i = 0; i < componentTypes.Length; i++)
                {
                    if (GetComponent(componentTypes[i]) != null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public string GetAssetTypeName()
        {
            return string.Format("{0}", assetType.Name);
        }

        public string GetComponentTypeName()
        {
            if (componentTypes != null)
            {
                string componentNames = string.Empty;
                for (int i = 0; i < componentTypes.Length; i++)
                {
                    componentNames += string.Format("{0}", componentTypes[i].Name);
                    if (i < componentTypes.Length - 1)
                    {
                        if (i == componentTypes.Length - 2)
                        {
                            componentNames += " or ";
                        }
                        else
                        {
                            componentNames += ", ";
                        }
                    }
                }
                return componentNames;
            }
            return null;
        }

        protected void FindPathFromAssets(UnityEngine.Object asset, string fileExtension)
        {
            if (asset != null)
            {
                string path = UnityEditor.AssetDatabase.GetAssetPath(asset);
                if (!string.IsNullOrEmpty(path))
                {
                    assetLocation = AssetLocation.PathOrUrl;
                    pathOrUrl = path;
                }
                else
                {
                    pathOrUrl = string.Format("Assets/{0}{1}", asset.name, fileExtension);
                }
            }
        }
#endif

        protected virtual void OnEnable()
        {
            if (!wasLoaded)
            {
                LoadAndApply();
            }
        }

        protected virtual void OnDisable()
        {
            savingRoutine = null;
            loadingRoutine = null;
#if UNITY_EDITOR
            if (resizedTexture != null)
            {
                DestroyImmediate(resizedTexture);
                resizedTexture = null;
            }
#endif
        }

        protected virtual void OnDestory()
        {
            Unload();
        }

        protected string GetAssetPath()
        {
            string result = pathOrUrl;
            if (!string.IsNullOrEmpty(result))
            {
                if (assetLocation == AssetLocation.StreamingAssets)
                {
                    result = string.Format("{0}/{1}", Application.streamingAssetsPath, result);
                }
                else if (assetLocation == AssetLocation.PersistentData)
                {
                    result = string.Format("{0}/{1}", Application.persistentDataPath, result);
                }
                else if (!result.Contains(":"))
                {
                    result = string.Format("{0}/{1}", Path.GetDirectoryName(Application.dataPath), result);
                }
                if (!result.Contains("://"))
                {
                    result = result.Replace('\\', '/');
                }
            }
            return result;
        }

        protected string GetAssetURL()
        {
            string result = pathOrUrl;
            if (!string.IsNullOrEmpty(result))
            {
                if (assetLocation == AssetLocation.StreamingAssets)
                {
                    result = string.Format("{0}/{1}", Application.streamingAssetsPath, result);
                }
                else if (assetLocation == AssetLocation.PersistentData)
                {
                    result = string.Format("{0}/{1}", Application.persistentDataPath, result);
                }
                else if (!result.Contains(":"))
                {
                    result = string.Format("{0}/{1}", Path.GetDirectoryName(Application.dataPath), result);
                }
                if (!result.Contains("://"))
                {
                    result = string.Format(string.Format("file://{0}", result.Replace('\\', '/')));
                }
            }
            return result;
        }

        protected virtual UnityEngine.Object LoadAsset()
        {
            return null;
        }

        protected abstract void ApplyAsset();

        private IEnumerator Saving(string url, string savePath)
        {
            UnityWebRequest request = UnityWebRequest.Get(url);
            if (url.StartsWith("http://") || url.StartsWith("https://"))
            {
                request.SetRequestHeader("Cache-Control", "no-cache, no-store, max-age=0");
                request.SetRequestHeader("Pragma", "no-cache");
            }
            request.disposeDownloadHandlerOnDispose = true;
#if UNITY_2017_2_OR_NEWER
            DownloadHandlerFile downloadHandlerFile = new DownloadHandlerFile(savePath);
            downloadHandlerFile.removeFileOnAbort = true;
            request.downloadHandler = downloadHandlerFile;
            request.SendWebRequest();
#else
            request.downloadHandler = new DownloadHandlerBuffer();
            request.Send();
#endif
            bool success = false;
            while (!request.isDone)
            {
                if (OnSaving != null)
                {
                    OnSaving.Invoke(savePath, request.downloadProgress);
                }
                yield return null;
            }
            if (OnSaving != null)
            {
                OnSaving.Invoke(savePath, 1f);
            }
            yield return null;
            if (request.error == null)
            {
#if UNITY_2017_2_OR_NEWER
                success = true;
#else
                try
                {
                    File.WriteAllBytes(savePath, request.downloadHandler.data);
                    success = true;
                }
                catch
                {
                }
#endif
            }
            else
            {
                if (File.Exists(savePath))
                {
                    try
                    {
                        File.Delete(savePath);
                    }
                    catch
                    {
                    }
                }
            }
            request.Dispose();
            wasSaved = true;
            if (OnSaved != null)
            {
                OnSaved(savePath, success);
            }
            savingRoutine = null;
        }

        private IEnumerator Loading(string url)
        {
            UnityWebRequest request = UnityWebRequest.Get(url);
            if (url.StartsWith("http://") || url.StartsWith("https://"))
            {
                request.SetRequestHeader("Cache-Control", "no-cache, no-store, max-age=0");
                request.SetRequestHeader("Pragma", "no-cache");
            }
            request.disposeDownloadHandlerOnDispose = true;
            if (assetType == typeof(Texture) || assetType == typeof(Sprite))
            {
                request.downloadHandler = new DownloadHandlerTexture();
            }
            else if (assetType == typeof(AudioClip))
            {
                request.downloadHandler = new DownloadHandlerAudioClip(url, AudioType.UNKNOWN);
            }
            else if (assetType == typeof(AssetBundle))
            {
                request.downloadHandler = new DownloadHandlerAssetBundle(url, 0);
            }
            else
            {
                request.downloadHandler = new DownloadHandlerBuffer();
            }
#if UNITY_2017_2_OR_NEWER
            request.SendWebRequest();
#else
            request.Send();
#endif
            while (!request.isDone)
            {
                if (OnLoading != null)
                {
                    OnLoading.Invoke(request.downloadProgress);
                }
                yield return null;
            }
            if (OnLoading != null)
            {
                OnLoading.Invoke(1f);
            }
            yield return null;
            if (request.error == null)
            {
                if (assetType == typeof(Texture))
                {
                    _asset = DownloadHandlerTexture.GetContent(request);
                }
                else if (assetType == typeof(Sprite))
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(request);
                    if (texture != null)
                    {
                        texture.name = Path.GetFileNameWithoutExtension(url);
                        _asset = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
                    }
                }
                else if (assetType == typeof(AudioClip))
                {
                    _asset = DownloadHandlerAudioClip.GetContent(request);
                }
                else if (assetType == typeof(AssetBundle))
                {
                    _asset = DownloadHandlerAssetBundle.GetContent(request);
                }
                else if (assetType == typeof(TextAsset))
                {
#if UNITY_2018_1_OR_NEWER
                    _asset = new TextAsset(DownloadHandlerBuffer.GetContent(request));
#else
                    _asset = new BufferAsset(DownloadHandlerBuffer.GetContent(request));
#endif
                }
                else
                {
                    _asset = new BufferAsset(request.downloadHandler.data);
                }
                wasLoaded = true;
                if (_asset != null)
                {
                    _asset.name = Path.GetFileNameWithoutExtension(url);
                }
            }
            request.Dispose();
            if (OnLoaded != null)
            {
                OnLoaded.Invoke(_asset != null);
            }
            loadingRoutine = null;
        }

        private IEnumerator Applying()
        {
            while (loadingRoutine != null)
            {
                yield return null;
            }
            ApplyAsset();
        }

        public bool Save(string path)
        {
            if (savingRoutine != null)
            {
                StopCoroutine(savingRoutine);
                savingRoutine = null;
            }
            wasSaved = false;
            if (!string.IsNullOrEmpty(pathOrUrl))
            {
                if (gameObject.activeInHierarchy)
                {
                    if (pathOrUrl.Contains("://"))
                    {
                        assetLocation = AssetLocation.PathOrUrl;
                    }
                    if (string.IsNullOrEmpty(path))
                    {
                        path = string.Format("{0}/{1}", Application.persistentDataPath, Path.GetFileName(pathOrUrl));
                    }
                    else if (!path.Contains(":"))
                    {
                        path = string.Format("{0}/{1}", Application.persistentDataPath, path);
                    }
                    string dir = Path.GetDirectoryName(path);
                    if (!Directory.Exists(dir))
                    {
                        try
                        {
                            Directory.CreateDirectory(dir);
                        }
                        catch (Exception)
                        {
                            Debug.LogWarning(string.Format("Failed to create directory : ", dir));
                            return false;
                        }
                    }
                    string url = GetAssetURL();
                    try
                    {
                        Uri uri = new Uri(url);
                        loadingRoutine = StartCoroutine(Saving(uri.OriginalString, path));
                        return true;
                    }
                    catch (Exception)
                    {
                        Debug.LogWarning(string.Format("Invalid URL : ", url));
                    }
                }
                else
                {
                    Debug.LogWarning("Unable to save asset from path or URL while GameObject is not actived.");
                }
            }
            return false;
        }

        public bool Load()
        {
            Unload();
            if (!string.IsNullOrEmpty(pathOrUrl))
            {
                if (pathOrUrl.Contains("://"))
                {
                    assetLocation = AssetLocation.PathOrUrl;
                }
            }
            UnityEngine.Object unknown = LoadAsset();
            if (unknown != null)
            {
                _asset = unknown;
                wasLoaded = true;
                return true;
            }
            if (!string.IsNullOrEmpty(pathOrUrl))
            {
                if (gameObject.activeInHierarchy)
                {
                    string url = GetAssetURL();
                    try
                    {
                        Uri uri = new Uri(url);
                        loadingRoutine = StartCoroutine(Loading(uri.OriginalString));
                        return true;
                    }
                    catch (Exception)
                    {
#if !UNITY_EDITOR
                        Debug.LogWarning(string.Format("Invalid URL : ", url));
#endif
                    }
                }
                else
                {
#if !UNITY_EDITOR
                    Debug.LogWarning("Unable to load asset from path or url while GameObject is not actived.");
#endif
                }
            }
            return false;
        }

        public void Unload()
        {
            if (loadingRoutine != null)
            {
                StopCoroutine(loadingRoutine);
                loadingRoutine = null;
            }
            wasLoaded = false;
            if (_asset != null)
            {
                if (Application.isEditor)
                {
                    DestroyImmediate(_asset);
                }
                else
                {
                    Destroy(_asset);
                }
                _asset = null;
            }
        }

        public void Apply()
        {
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(Applying());
            }
            else
            {
                ApplyAsset();
            }
        }

        public void LoadAndApply()
        {
            if (Load())
            {
                Apply();
            }
        }
    }
}
