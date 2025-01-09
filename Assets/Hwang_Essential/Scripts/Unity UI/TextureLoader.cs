using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace UnityEngine.UI
{
    [DisallowMultipleComponent]
    public class TextureLoader : MonoBehaviour
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
        [Tooltip("Material index to change texture.")]
        private int materialIndex;
        public int MaterialIndex
        {
            get
            {
                return materialIndex;
            }
            set
            {
                if (materialIndex != value)
                {
                    materialIndex = value;
                    LoadAndApply();
                }
            }
        }

        [SerializeField]
        [Tooltip("Texture property name in shader. (default is main texture)")]
        private string textureName;
        public string TextureName
        {
            get
            {
                return textureName;
            }
            set
            {
                if (string.Compare(textureName, value, true) != 0)
                {
                    textureName = value;
                    LoadAndApply();
                }
            }
        }

        [NonSerialized]
        private bool applied;

        [NonSerialized]
        private Texture texture;

#if UNITY_EDITOR
        public GUIContent Preview
        {
            get
            {
                if (texture != null)
                {
                    return new GUIContent(UnityEditor.AssetPreview.GetAssetPreview(texture));
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
            Material material = GetMaterial();
            if (material != null)
            {
                Texture materialTexture = material.mainTexture;
                if (!string.IsNullOrEmpty(textureName))
                {
                    materialTexture = material.GetTexture(textureName);
                }
                if (materialTexture != null)
                {
                    FindAssetFromResources(materialTexture);
                }
            }
            RawImage rawImage = GetComponent<RawImage>();
            if (rawImage != null && rawImage.texture != null)
            {
                FindAssetFromResources(rawImage.texture);
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

        private void OnDestroy()
        {
            Unload();
        }

        public Material GetMaterial()
        {
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                if (Application.isPlaying)
                {
                    if (materialIndex >= 0 && materialIndex < renderer.materials.Length)
                    {
                        return renderer.materials[materialIndex];
                    }
                }
                else
                {
                    if (materialIndex >= 0 && materialIndex < renderer.sharedMaterials.Length)
                    {
                        return renderer.sharedMaterials[materialIndex];
                    }
                }
            }
            return null;
        }

        private IEnumerator Loading()
        {
            ResourceRequest request;
            if (language == SystemLanguage.Unknown)
            {
                request = Resources.LoadAsync<Texture>(fileName);
            }
            else
            {
                request = Resources.LoadAsync<Texture>(string.Format("{0}/{1}", language, fileName));
            }
            yield return request;
            if (request.asset == null && defaultLanguage != language)
            {
                request = Resources.LoadAsync<Texture>(string.Format("{0}/{1}", defaultLanguage, fileName));
                yield return request;
            }
            texture = request.asset as Texture;
            if (texture != null)
            {
                Apply();
            }
        }

        public bool Load()
        {
            Unload();
            if (language == SystemLanguage.Unknown)
            {
                texture = Resources.Load<Texture>(fileName);
            }
            else
            {
                texture = Resources.Load<Texture>(string.Format("{0}/{1}", language, fileName));
            }
            if (texture == null && defaultLanguage != language)
            {
                texture = Resources.Load<Texture>(string.Format("{0}/{1}", defaultLanguage, fileName));
            }
            return (texture != null);
        }

        public void Unload()
        {
            if (texture != null)
            {
                // Unloading texture will lose allocated textures that are in use elsewhere
                //Resources.UnloadAsset(texture);
                Resources.UnloadUnusedAssets();
                texture = null;
            }
            applied = false;
        }

        public void Apply()
        {
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                if (Application.isPlaying)
                {
                    Material material = GetMaterial();
                    if (material != null)
                    {
                        if (string.IsNullOrEmpty(textureName))
                        {
                            material.mainTexture = texture;
                        }
                        else
                        {
                            material.SetTexture(textureName, texture);
                        }
                    }
                }
                //MaterialPropertyBlock mpb = new MaterialPropertyBlock();
                //Material material = GetMaterial();
                //if (material != null)
                //{
                //    if (string.IsNullOrEmpty(textureName))
                //    {
                //        mpb.SetTexture("_MainTex", texture);
                //    }
                //    else
                //    {
                //        mpb.SetTexture(textureName, texture);
                //    }
                //    renderer.SetPropertyBlock(mpb);
                //}
            }
            RawImage rawImage = GetComponent<RawImage>();
            if (rawImage != null)
            {
                rawImage.texture = texture;
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
