using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace UnityEngine.UI
{
    public abstract class AssetLoader : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Whether to use asynchronous method at runtime")]
        protected bool useAsyncMethod = true;
        public bool UseAsyncMethod
        {
            get { return useAsyncMethod; }
            set { if (useAsyncMethod != value) { useAsyncMethod = value; } }
        }

        [SerializeField]
        [Tooltip("Whether to use language name as top-level folder")]
        protected bool localizable = true;
        public bool Localizable
        {
            get { return localizable; }
            set { if (localizable != value) { localizable = value; } }
        }

        [SerializeField]
        [Tooltip("Language refers to top-level folder in resource path\n('Unknown' means Application.systemLanguage)")]
        protected SystemLanguage language = SystemLanguage.Unknown;
        public SystemLanguage Language
        {
            get { return language; }
            set { if (language != value) { language = value; } }
        }

        [SerializeField]
        [Tooltip("Resource path and name without file extension")]
        protected string resourcePath;
        public string ResourcePath
        {
            get { return resourcePath; }
            set { if (resourcePath != value) { resourcePath = value; } }
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
                    if (previewTexture == null)
                    {
                        previewTexture = UnityEditor.AssetPreview.GetMiniThumbnail(_asset);
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

        protected void FindPathFromResources(UnityEngine.Object asset)
        {
            if (asset != null)
            {
                string path = UnityEditor.AssetDatabase.GetAssetPath(asset);
                if (!string.IsNullOrEmpty(path))
                {
                    int index = -1;
                    StringBuilder sb = new StringBuilder();
                    string[] parts = path.Split('/');
                    for (int i = 0; i < parts.Length; i++)
                    {
                        if (index == -1)
                        {
                            if (string.Compare(parts[i], "Resources", true) == 0)
                            {
                                index = i + 1;
                            }
                        }
                        else if (index == i && localizable)
                        {
                            try
                            {
                                language = (SystemLanguage)Enum.Parse(typeof(SystemLanguage), parts[i], true);
                            }
                            catch (Exception)
                            {
                                language = SystemLanguage.Unknown;
                                localizable = false;
                                sb.Append(parts[i]);
                            }
                        }
                        else
                        {
                            if (sb.Length > 0)
                            {
                                sb.Append('/');
                            }
                            sb.Append(parts[i]);
                        }
                    }
                    resourcePath = Path.ChangeExtension(sb.ToString(), null);
                }
                else
                {
                    resourcePath = asset.name;
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

        protected string GetResourcePath()
        {
            string result = resourcePath;
            if (!string.IsNullOrEmpty(result))
            {
                if (localizable)
                {
                    if (language == SystemLanguage.Unknown)
                    {
                        result = string.Format("{0}/{1}", Application.systemLanguage, resourcePath);
                    }
                    else
                    {
                        result = string.Format("{0}/{1}", language, resourcePath);
                    }
                }
                result = result.Replace('\\', '/');
            }
            return result;
        }

        protected virtual UnityEngine.Object LoadAsset()
        {
            return null;
        }

        protected abstract void ApplyAsset();

        protected IEnumerator Loading(string path)
        {
            ResourceRequest request = Resources.LoadAsync(path, assetType);
            if (request != null)
            {
                while (!request.isDone)
                {
                    if (OnLoading != null)
                    {
                        OnLoading.Invoke(request.progress);
                    }
                    yield return null;
                }
                if (OnLoading != null)
                {
                    OnLoading.Invoke(1f);
                }
                yield return null;
                _asset = request.asset;
                wasLoaded = true;
                if (OnLoaded != null)
                {
                    OnLoaded.Invoke(_asset != null);
                }
            }
            loadingRoutine = null;
        }

        protected IEnumerator Applying()
        {
            while (loadingRoutine != null)
            {
                yield return null;
            }
            ApplyAsset();
        }

        public bool Load()
        {
            Unload();
            UnityEngine.Object unknown = LoadAsset();
            if (unknown != null)
            {
                _asset = unknown;
                wasLoaded = true;
                return true;
            }
            if (!string.IsNullOrEmpty(resourcePath))
            {
                if (gameObject.activeInHierarchy && useAsyncMethod)
                {
                    loadingRoutine = StartCoroutine(Loading(GetResourcePath()));
                    return true;
                }
                else
                {
                    _asset = Resources.Load(GetResourcePath(), assetType);
                    wasLoaded = true;
                    return _asset != null;
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
                //Resources.UnloadAsset(_asset);
                _asset = null;
                Resources.UnloadUnusedAssets();
            }
        }

        public void Apply()
        {
            if (gameObject.activeInHierarchy && useAsyncMethod)
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
