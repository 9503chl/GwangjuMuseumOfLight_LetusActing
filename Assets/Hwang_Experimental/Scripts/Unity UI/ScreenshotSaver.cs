using System;
using System.IO;
using UnityEngine.SceneManagement;

namespace UnityEngine
{
    [DisallowMultipleComponent]
    public class ScreenshotSaver : MonoBehaviour
    {
        private const string DefaultScreenshotName = "Screenshot/{P}_{D}_{T}";
        private const string DefaultDateFormat = "yyyyMMdd";
        private const string DefaultTimeFormat = "HHmmss_ffffff";

        public Camera TargetCamera = null;
        public KeyCode CaptureHotKey = KeyCode.F12;
        public bool WithShiftKey = false;
        public bool WithControlKey = false;
        [Tooltip("ScreenshotName can contain folder name or special names.\n{C} : company name\n{P} : product name\n{S} : active scene name\n{N} : number of screenshot\n{D} : current date\n{T} : current time")]
        public string ScreenshotName = DefaultScreenshotName;
        public string DateFormat = DefaultDateFormat;
        public string TimeFormat = DefaultTimeFormat;
        public bool SaveAsPNG = false;
        public AudioSource ShutterSound = null;

        private static int screenshotNumber = 0;

        private static ScreenshotSaver instance;
        public static ScreenshotSaver Instance
        {
            get
            {
                if (instance == null)
                {
#if UNITY_2020_1_OR_NEWER
                    ScreenshotSaver[] templates = FindObjectsOfType<ScreenshotSaver>(true);
#else
                    ScreenshotSaver[] templates = FindObjectsOfType<ScreenshotSaver>();
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

        private void Update()
        {
            if (Input.GetKeyDown(CaptureHotKey) &&
                WithShiftKey == (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) &&
                WithControlKey == (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            {
                SaveScreenshot();
            }
        }

#if UNITY_EDITOR
        public void Reset()
        {
            screenshotNumber = 0;
        }
#endif

        private string RevealSpecialNames(string path)
        {
            DateTime now = DateTime.Now;
            if (path.Contains("{Date}"))
            {
                if (string.IsNullOrEmpty(DateFormat))
                {
                    path = path.Replace("{Date}", now.ToString(DefaultDateFormat));
                }
                else
                {
                    path = path.Replace("{Date}", now.ToString(DateFormat));
                }
            }
            if (path.Contains("{D}"))
            {
                if (string.IsNullOrEmpty(DateFormat))
                {
                    path = path.Replace("{D}", now.ToString(DefaultDateFormat));
                }
                else
                {
                    path = path.Replace("{D}", now.ToString(DateFormat));
                }
            }
            if (path.Contains("{Time}"))
            {
                if (string.IsNullOrEmpty(TimeFormat))
                {
                    path = path.Replace("{Time}", now.ToString(DefaultTimeFormat));
                }
                else
                {
                    path = path.Replace("{Time}", now.ToString(TimeFormat));
                }
            }
            if (path.Contains("{T}"))
            {
                if (string.IsNullOrEmpty(TimeFormat))
                {
                    path = path.Replace("{T}", now.ToString(DefaultTimeFormat));
                }
                else
                {
                    path = path.Replace("{T}", now.ToString(TimeFormat));
                }
            }
            if (path.Contains("{C}"))
            {
                path = path.Replace("{C}", Application.companyName);
            }
            if (path.Contains("{P}"))
            {
                path = path.Replace("{P}", Application.productName);
            }
            if (path.Contains("{S}"))
            {
                path = path.Replace("{S}", SceneManager.GetActiveScene().name);
            }
            if (path.Contains("{N}"))
            {
                path = path.Replace("{N}", string.Format("{0:D6}", screenshotNumber));
            }
            return path;
        }

        public string GetScreenshotPath()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            string dir = Path.GetDirectoryName(Path.GetDirectoryName(Application.streamingAssetsPath)).Replace('\\', '/');
#else
            string dir = Application.persistentDataPath;
#endif
            string name = ScreenshotName;
            string ext = "jpg";
            if (SaveAsPNG)
            {
                ext = "png";
            }
            if (string.IsNullOrEmpty(name))
            {
                name = DefaultScreenshotName;
            }
            return RevealSpecialNames(string.Format("{0}/{1}.{2}", dir, name, ext));
        }

        public void SaveScreenshot(string path)
        {
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
            {
                try
                {
                    Directory.CreateDirectory(dir);
                }
                catch (Exception)
                {
                    Debug.LogError(string.Format("Failed to create screenshot folder : {0}", dir));
                    return;
                }
            }
            if (ShutterSound != null)
            {
                ShutterSound.Play();
            }
            if (TargetCamera != null)
            {
                RenderTexture renderTexture = new RenderTexture(TargetCamera.pixelWidth, TargetCamera.pixelHeight, 24);
                RenderTexture targetTexture = TargetCamera.targetTexture;
                RenderTexture activeTexture = RenderTexture.active;
                TargetCamera.targetTexture = renderTexture;
                TargetCamera.Render();
                RenderTexture.active = renderTexture;
                Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
                texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
                texture.Apply();
                TargetCamera.targetTexture = targetTexture;
                RenderTexture.active = activeTexture;
                if (Application.isPlaying)
                {
                    Destroy(renderTexture);
                }
                else
                {
                    DestroyImmediate(renderTexture);
                }
                if (File.Exists(path))
                {
                    try
                    {
                        File.Delete(path);
                    }
                    catch (Exception)
                    {
                    }
                }
                if (SaveAsPNG)
                {
                    File.WriteAllBytes(path, texture.EncodeToPNG());
                }
                else
                {
                    File.WriteAllBytes(path, texture.EncodeToJPG());
                }
            }
            else
            {
                ScreenCapture.CaptureScreenshot(path);
            }
            Debug.Log(string.Format("Screenshot saved : {0}", path));
        }

        public void SaveScreenshot()
        {
            screenshotNumber++;
            if (screenshotNumber > 999999)
            {
                screenshotNumber = 0;
            }
            SaveScreenshot(GetScreenshotPath());
        }
    }
}
