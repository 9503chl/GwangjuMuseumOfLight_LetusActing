using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace UnityEngine.UI
{
    [DisallowMultipleComponent]
    public class AudioClipLoader : MonoBehaviour
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

        [NonSerialized]
        private bool applied;

        [NonSerialized]
        private AudioClip audioClip;

#if UNITY_EDITOR
        public GUIContent Preview
        {
            get
            {
                if (audioClip != null)
                {
                    return new GUIContent(UnityEditor.AssetPreview.GetAssetPreview(audioClip));
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
            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource != null && audioSource.clip != null)
            {
                FindAssetFromResources(audioSource.clip);
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
                request = Resources.LoadAsync<AudioClip>(fileName);
            }
            else
            {
                request = Resources.LoadAsync<AudioClip>(string.Format("{0}/{1}", language, fileName));
            }
            yield return request;
            if (request.asset == null && defaultLanguage != language)
            {
                request = Resources.LoadAsync<AudioClip>(string.Format("{0}/{1}", defaultLanguage, fileName));
                yield return request;
            }
            audioClip = request.asset as AudioClip;
            if (audioClip != null)
            {
                Apply();
            }
        }

        public bool Load()
        {
            Unload();
            if (language == SystemLanguage.Unknown)
            {
                audioClip = Resources.Load<AudioClip>(fileName);
            }
            else
            {
                audioClip = Resources.Load<AudioClip>(string.Format("{0}/{1}", language, fileName));
            }
            if (audioClip == null && defaultLanguage != language)
            {
                audioClip = Resources.Load<AudioClip>(string.Format("{0}/{1}", defaultLanguage, fileName));
            }
            return (audioClip != null);
        }

        public void Unload()
        {
            if (audioClip != null)
            {
                Resources.UnloadAsset(audioClip);
                audioClip = null;
            }
            applied = false;
        }

        public void Apply()
        {
            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.clip = audioClip;
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
