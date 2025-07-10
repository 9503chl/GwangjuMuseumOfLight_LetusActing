using System;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UnityEngine
{
    [DisallowMultipleComponent]
    public class SceneLoader : MonoBehaviour
    {
        public string HomeSceneName;
        public string PreviousSceneName;
        public string NextSceneName;

        public Slider ProgressSlider;

        public bool ReloadActiveScene = false;
        public bool UseAsyncOperation = false;
        public bool UseSingleInstance = false;

        [Serializable]
        public class LoadingEvent : UnityEvent<float> { }

        public LoadingEvent onLoading;

        private static SceneLoader instance;
        public static SceneLoader Instance
        {
            get
            {
#if UNITY_2022_2_OR_NEWER || UNITY_2021_3
                SceneLoader[] templates = FindObjectsByType<SceneLoader>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#elif UNITY_2020_1_OR_NEWER
                SceneLoader[] templates = FindObjectsOfType<SceneLoader>(true);
#else
                SceneLoader[] templates = FindObjectsOfType<SceneLoader>();
#endif
                if (templates.Length > 0)
                {
                    instance = templates[0];
                    instance.enabled = true;
                    instance.gameObject.SetActive(true);
                }
                return instance;
            }
        }

        private void Awake()
        {
            if (UseSingleInstance)
            {
                if (instance != null)
                {
                    Destroy(gameObject);
                    return;
                }
                DontDestroyOnLoad(gameObject);
            }
            instance = this;
            if (ProgressSlider != null)
            {
                ProgressSlider.gameObject.SetActive(false);
            }
        }

        private IEnumerator Loading(string sceneName, LoadSceneMode loadSceneMode)
        {
            if (ProgressSlider != null)
            {
                ProgressSlider.normalizedValue = 0f;
                ProgressSlider.gameObject.SetActive(true);
                yield return null;
            }
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, loadSceneMode);
            if (operation != null)
            {
                while (!operation.isDone)
                {
                    if (ProgressSlider != null)
                    {
                        ProgressSlider.normalizedValue = operation.progress;
                    }
                    if (onLoading != null)
                    {
                        onLoading.Invoke(operation.progress);
                    }
                    yield return null;
                }
                if (onLoading != null)
                {
                    onLoading.Invoke(1f);
                }
            }
            if (ProgressSlider != null)
            {
                ProgressSlider.normalizedValue = 1f;
                yield return null;
                ProgressSlider.gameObject.SetActive(false);
            }
        }

        private IEnumerator Loading(int sceneIndex, LoadSceneMode loadSceneMode)
        {
            if (ProgressSlider != null)
            {
                ProgressSlider.normalizedValue = 0f;
                ProgressSlider.gameObject.SetActive(true);
                yield return null;
            }
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex, loadSceneMode);
            if (operation != null)
            {
                while (!operation.isDone)
                {
                    if (ProgressSlider != null)
                    {
                        ProgressSlider.normalizedValue = operation.progress;
                    }
                    if (onLoading != null)
                    {
                        onLoading.Invoke(operation.progress);
                    }
                    yield return null;
                }
                if (onLoading != null)
                {
                    onLoading.Invoke(1f);
                }
            }
            if (ProgressSlider != null)
            {
                ProgressSlider.normalizedValue = 1f;
                yield return null;
                ProgressSlider.gameObject.SetActive(false);
            }
        }

        public void LoadScene(string sceneName, LoadSceneMode loadSceneMode)
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (string.Compare(sceneName, activeScene.name, true) == 0)
            {
                if (!ReloadActiveScene)
                {
                    Debug.LogError("Scene is already active!");
                    return;
                }
            }
            if (UseAsyncOperation && isActiveAndEnabled)
            {
                StartCoroutine(Loading(sceneName, loadSceneMode));
            }
            else
            {
                SceneManager.LoadScene(sceneName, loadSceneMode);
            }
        }

        public void LoadScene(string sceneName)
        {
            LoadScene(sceneName, LoadSceneMode.Single);
        }

        public void LoadScene(int sceneIndex, LoadSceneMode loadSceneMode)
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (sceneIndex == activeScene.buildIndex)
            {
                if (!ReloadActiveScene)
                {
                    Debug.LogError("Scene is already active!");
                    return;
                }
            }
            if (UseAsyncOperation && isActiveAndEnabled)
            {
                StartCoroutine(Loading(sceneIndex, loadSceneMode));
            }
            else
            {
                SceneManager.LoadScene(sceneIndex, loadSceneMode);
            }
        }

        public void LoadScene(int sceneIndex)
        {
            LoadScene(sceneIndex, LoadSceneMode.Single);
        }

        public void LoadHomeScene()
        {
            if (!string.IsNullOrEmpty(HomeSceneName))
            {
                LoadScene(HomeSceneName);
            }
            else
            {
                LoadScene(0);
            }
        }

        public void LoadPreviousScene()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (!string.IsNullOrEmpty(PreviousSceneName))
            {
                LoadScene(PreviousSceneName);
            }
            else
            {
                LoadScene(activeScene.buildIndex - 1);
            }
        }

        public void LoadNextScene()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (!string.IsNullOrEmpty(NextSceneName))
            {
                LoadScene(NextSceneName);
            }
            else
            {
                LoadScene(activeScene.buildIndex + 1);
            }
        }
    }
}
