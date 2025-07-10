using System;
using UnityEngine;
using UnityEngine.Events;

namespace UnityEngine.UI
{
    [DisallowMultipleComponent]
    public class AssetLanguageManager : MonoBehaviour
    {
        [SerializeField]
        private bool useAsyncMethod = true;
        public bool UseAsyncMethod
        {
            get { return useAsyncMethod; }
            set { if (useAsyncMethod != value) { useAsyncMethod = value; } }
        }

        [SerializeField]
        private SystemLanguage language = SystemLanguage.Unknown;
        public SystemLanguage Language
        {
            get { return language; }
            set { if (language != value) { language = value; } }
        }

        [NonSerialized]
        private SystemLanguage previousLanguage = SystemLanguage.Unknown;

        public event UnityAction OnChangeLanguage;

        private static AssetLanguageManager instance;
        public static AssetLanguageManager Instance
        {
            get
            {
                if (instance == null)
                {
#if UNITY_2022_2_OR_NEWER || UNITY_2021_3
                    AssetLanguageManager[] templates = FindObjectsByType<AssetLanguageManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#elif UNITY_2020_1_OR_NEWER
                    AssetLanguageManager[] templates = FindObjectsOfType<AssetLanguageManager>(true);
#else
                    AssetLanguageManager[] templates = FindObjectsOfType<AssetLanguageManager>();
#endif
                    if (templates.Length > 0)
                    {
                        instance = templates[0];
                        instance.enabled = true;
                        instance.gameObject.SetActive(true);
                    }
                }
                return instance;
            }
        }

        private void Awake()
        {
            instance = this;
        }

        private void OnEnable()
        {
            Apply();
        }

        private void Update()
        {
            if (previousLanguage != language)
            {
                previousLanguage = language;
                ApplyLanguage(language);
            }
        }

        private void ApplyLanguage(SystemLanguage targetLanguage)
        {
            AssetLoader[] loaders = Resources.FindObjectsOfTypeAll<AssetLoader>();
            foreach (AssetLoader loader in loaders)
            {
                loader.UseAsyncMethod = useAsyncMethod;
                loader.Language = targetLanguage;
                if (loader.Localizable)
                {
                    loader.LoadAndApply();
                }
            }
            previousLanguage = language;
            if (language != targetLanguage)
            {
                language = targetLanguage;
                if (OnChangeLanguage != null)
                {
                    OnChangeLanguage.Invoke();
                }
            }
        }

        public void Apply()
        {
            if (Application.isPlaying)
            {
                ApplyLanguage(language);
            }
        }
    }
}
