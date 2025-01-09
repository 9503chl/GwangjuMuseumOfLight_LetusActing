using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace UnityEngine.UI
{
    [DisallowMultipleComponent]
    public class SpriteLoader : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Language name represents resource folder name.")]
        private SystemLanguage language = SystemLanguage.Unknown;
        public SystemLanguage Language
        {
            get
            {
                return language;
            }
            set
            {
                if (language != value)
                {
                    language = value;
                    LoadAndApply();
                }
            }
        }

        [NonSerialized]
        private SystemLanguage defaultLanguage = SystemLanguage.Unknown;

        [SerializeField]
        [Tooltip("Resource name without file extenstion.")]
        private string fileName;
        public string FileName
        {
            get
            {
                return fileName;
            }
            set
            {
                if (string.Compare(fileName, value, true) != 0)
                {
                    fileName = value;
                    LoadAndApply();
                }
            }
        }

        [SerializeField]
        private bool autoSize;
        public bool AutoSize
        {
            get
            {
                return autoSize;
            }
            set
            {
                autoSize = value;
            }
        }

        [NonSerialized]
        private bool applied;

        [NonSerialized]
        private Sprite sprite;

#if UNITY_EDITOR
        public GUIContent Preview
        {
            get
            {
                if (sprite != null)
                {
                    return new GUIContent(UnityEditor.AssetPreview.GetAssetPreview(sprite));
                }
                return null;
            }
        }

        private const string ResourcesPath = "Assets/Resources/";

        private void FindAssetFromResources(UnityEngine.Object asset)
        {
            if (asset != null)
            {
                string assetPath = UnityEditor.AssetDatabase.GetAssetPath(asset);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    if (assetPath.Length >= ResourcesPath.Length && string.Compare(assetPath.Substring(0, ResourcesPath.Length), ResourcesPath, true) == 0)
                    {
                        fileName = Path.ChangeExtension(assetPath.Substring(ResourcesPath.Length, assetPath.Length - ResourcesPath.Length - 1), null);
                        string[] parts = fileName.Split('/');
                        if (parts.Length > 1)
                        {
                            try
                            {
                                language = (SystemLanguage)Enum.Parse(typeof(SystemLanguage), parts[0], true);
                                fileName = fileName.Remove(0, parts[0].Length + 1);
                            }
                            catch (Exception)
                            {
                            }
                        }
                        return;
                    }
                }
                fileName = asset.name;
            }
        }

        public void Reset()
        {
            Image image = GetComponent<Image>();
            if (image != null && image.sprite != null)
            {
                FindAssetFromResources(image.sprite);
            }
            RawImage rawImage = GetComponent<RawImage>();
            if (rawImage != null && rawImage.texture != null)
            {
                FindAssetFromResources(rawImage.texture);
            }
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                FindAssetFromResources(spriteRenderer.sprite);
            }
            Load();
        }
#endif

        private void Awake()
        {
            if (defaultLanguage == SystemLanguage.Unknown)
            {
                defaultLanguage = Application.systemLanguage;
            }
        }

        private void OnEnable()
        {
            if (!applied)
            {
                LoadAndApply();
            }
        }

        private IEnumerator Loading()
        {
            ResourceRequest request;
            if (language == SystemLanguage.Unknown)
            {
                request = Resources.LoadAsync<Sprite>(fileName);
            }
            else
            {
                request = Resources.LoadAsync<Sprite>(string.Format("{0}/{1}", language, fileName));
            }
            yield return request;
            if (request.asset == null && defaultLanguage != language)
            {
                request = Resources.LoadAsync<Sprite>(string.Format("{0}/{1}", defaultLanguage, fileName));
                yield return request;
            }
            sprite = request.asset as Sprite;
            if (sprite != null)
            {
                Apply();
            }
        }

        public bool Load()
        {
            Unload();
            if (language == SystemLanguage.Unknown)
            {
                sprite = Resources.Load<Sprite>(fileName);
            }
            else
            {
                sprite = Resources.Load<Sprite>(string.Format("{0}/{1}", language, fileName));
            }
            if (sprite == null && defaultLanguage != language)
            {
                sprite = Resources.Load<Sprite>(string.Format("{0}/{1}", defaultLanguage, fileName));
            }
            return (sprite != null);
        }

        public void Unload()
        {
            if (sprite != null)
            {
                Resources.UnloadAsset(sprite);
                sprite = null;
            }
            applied = false;
        }

        public void Apply()
        {
            Image image = GetComponent<Image>();
            if (image != null)
            {
                image.sprite = sprite;
                if (autoSize)
                {
                    image.SetNativeSize();
                }
            }
            RawImage rawImage = GetComponent<RawImage>();
            if (rawImage != null)
            {
                rawImage.texture = sprite.texture;
                if (autoSize)
                {
                    rawImage.SetNativeSize();
                }
            }
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = sprite;
                if (autoSize)
                {
                    //Debug.Log("sprite.rect.size = " + sprite.rect.size);
                }
            }
            applied = true;
        }

        public void LoadAndApply()
        {
            if (isActiveAndEnabled)
            {
                StartCoroutine(Loading());
            }
            else
            {
                applied = false;
            }
        }
    }
}
