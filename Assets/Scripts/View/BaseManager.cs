using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using WebSocketSharp;
using DG.Tweening;

public enum ViewKind
{
    Title,
    Video,
    Content,
    Capture,
    Finish,
    Size
}

public class BaseManager : Singleton<BaseManager>
{
    [SerializeField] private TitlePanel titlePanel;
    [SerializeField] private VideoPanel videoPanel;
    [SerializeField] private ContentPanel contentPanel;
    [SerializeField] private CapturePanel capturePanel;
    [SerializeField] private FinishPanel finishPanel;
    [SerializeField] private BackToTitlePanel backToTitlePanel;
    [SerializeField] private Button backToTitleBtn;

    [SerializeField] private AudioSource[] audioSources;

    private static Dictionary<string, SoundData> audioDic = new Dictionary<string, SoundData>();
    private static float[] volumns;

    private void CopyAudio()
    {
        volumns = new float[audioSources.Length];

        for (int i = 0; i < audioSources.Length; i++)
        {
            SoundData soundData = new SoundData(audioSources[i]);

            audioDic.Add(audioSources[i].name, soundData);
            volumns[i] = audioSources[i].volume;
        }
    }

    public static void SoundPlay(string audioName)
    {
        if (audioDic.TryGetValue(audioName, out SoundData soundData))
        {
            soundData._AudioSource.DOFade(soundData.volumn, 0.5f).SetEase(Ease.InExpo);
            soundData._AudioSource.Play();
        }
    }

    public static void SoundStop(string audioName)
    {
        if (audioDic.TryGetValue(audioName, out SoundData soundData))
        {
            soundData._AudioSource.DOFade(0, 0.5f).SetEase(Ease.InExpo);
            soundData._AudioSource.Stop();
        }
    }

    public enum AvailableLanguage
    {
        Korean,
        English
    }

    [SerializeField]
    private AvailableLanguage language = AvailableLanguage.Korean;
    public AvailableLanguage Language
    {
        get
        {
            return language;
        }
        set
        {
            ApplyLanguage(value);
        }
    }

    [NonSerialized]
    private AvailableLanguage previousLanguage = AvailableLanguage.Korean;

    public event UnityAction OnChangeLanguage;

    [SerializeField]
    private SyncSerialPort serialPort;

    [NonSerialized]
    private static readonly Dictionary<ViewKind, View> views = new Dictionary<ViewKind, View>();

    [NonSerialized]
    private static ViewKind activeView = ViewKind.Size;

    private static Coroutine coroutine;
    private static float time = 0;

    public ViewKind ActiveView
    {
        get
        {
            return activeView;
        }
        set
        {
            ChangeActiveView(value);
        }
    }

    private void Awake()
    {
        // 시스템 설정을 로드
        if (!ProjectSettings.LoadFromXml())
        {
            ProjectSettings.SaveToXml();
        }

        CopyAudio();

        titlePanel = FindObjectOfType<TitlePanel>(true);
        backToTitleBtn = FindObjectOfType<BackToTitleBtn>(true).GetComponent<Button>();
        contentPanel = FindObjectOfType<ContentPanel>(true);
        capturePanel = FindObjectOfType<CapturePanel>(true);
        backToTitleBtn.onClick.AddListener(() => backToTitlePanel.Show());

        Application.targetFrameRate = 24;

        // 모든 뷰를 딕셔너리에 넣은 후 숨김
        views.Add(ViewKind.Title, titlePanel);
        views.Add(ViewKind.Video, videoPanel);
        views.Add(ViewKind.Content, contentPanel);
        views.Add(ViewKind.Capture, capturePanel);
        views.Add(ViewKind.Finish, finishPanel);

        foreach (KeyValuePair<ViewKind, View> view in views)
        {
            if (view.Value != null)
            {
                view.Value.SetActive(false);
            }
        }
    }

    private void Start()
    {
        if (serialPort != null)
        {
            serialPort.PortName = ProjectSettings.SerialPortName;
            serialPort.BaudRate = ProjectSettings.SerialBoundRate;
            serialPort.OnReadText += SerialPort_OnReadText;
            serialPort.Open();
        }

        // 타이틀 화면에서 시작
        ChangeActiveView(ViewKind.Title);

        backToTitlePanel.Hide();
        titlePanel.Show();
    }
    private void SerialPort_OnReadText(string text)
    {
        if (text.StartsWith("QR"))
        {
            text = text[2..];
        }
        DebugLog(text);
        if (titlePanel.gameObject.activeInHierarchy && ActiveView == ViewKind.Title)
        {
            titlePanel.LoadQRCode(text);
        }
    }

    public void StartTimer()
    {
        if(coroutine == null)
            coroutine = Instance.StartCoroutine(ITimer());
    }

    public void ResetTimer()
    {
        time = 0;
    }

    private IEnumerator ITimer()
    {
        while (time < ProjectSettings.BackToTitleTime)
        {
            time += Time.fixedDeltaTime;
            //Debug.Log(time);
            yield return null;
        }
        ActiveView = ViewKind.Title;
        coroutine = null;
    }

    public void StopTimer()
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ActiveView = activeView + 1;
        }

        if (Input.anyKeyDown)
        {
            ResetTimer();
            if (!(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject?.GetComponent<Button>() != null))
            {
                SoundPlay("Button");
            }
            else
            {
                SoundPlay("Touch");
            }
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            string temp = "Er/SrNK0ZKh/IRQXjSj3+91+ES0sIXFST8iq+80NWcVsgnPibUdvO72ce4BPOFUcIXvOTtsQAE5ukp5oFoe5PpTNYnw0rLyKQckfxUNH35YpowB8mjH9a31ifYvz2QdJtKvLlXwSoLCl7h+J81G7p+AHdFZoguu5n2N6Nb/8LGP0GoOJ53wFGR3oTnWIrqseA6qzSHGxpZhCuvXrMG65Z3Ia1T95NM9EoFfmQBzM64TQy8DYZXeX7vbWpKwIUXHvvgwfI5a4M7CqP5eoYGf5TYfKQRj2RV5fvb9yba+DU1Hl5NrcWM4X0f1JrrewSdds/8x9FGILEPh3+r4i2LHk/w==";

            SerialPort_OnReadText(temp);
        }
    }
    private void ApplicationQuit(bool isOK)
    {
        if (isOK)
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }

    private void ChangeActiveView(ViewKind targetView)
    {
        if(targetView >= ViewKind.Size)
        {
            targetView = 0;
        }
        if (activeView != targetView)
        {
            if (views.ContainsKey(activeView))
            {
                views[activeView].Hide();
            }
            activeView = targetView;
            if (views.ContainsKey(targetView))
            {
                if (targetView == ViewKind.Title || targetView == ViewKind.Finish)
                    backToTitleBtn.gameObject.SetActive(false);
                else 
                    backToTitleBtn.gameObject.SetActive(true);
                views[targetView].Show();
            }
        }
    }

    public void DebugLog(string text)
    {
        if (!text.IsNullOrEmpty())
        {
            Debug.Log(text);
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            if (previousLanguage != Language)
            {
                ApplyLanguage(Language);
            }
        }
    }
#endif

    private void ApplyLanguage(AvailableLanguage currentLanguage)
    {
        language = currentLanguage;
        if (previousLanguage != language)
        {
            SystemLanguage systemLanguage = SystemLanguage.Unknown;
            switch (language)
            {
                case AvailableLanguage.Korean:
                    systemLanguage = SystemLanguage.Korean;
                    break;
                case AvailableLanguage.English:
                    systemLanguage = SystemLanguage.English;
                    break;
            }
            TextAssetLoader[] textAssetLoaders = FindObjectsOfType<TextAssetLoader>();
            foreach (TextAssetLoader loader in textAssetLoaders)
            {
                loader.Language = systemLanguage;
            }
            SpriteLoader[] spriteLoaders = FindObjectsOfType<SpriteLoader>();
            foreach (SpriteLoader loader in spriteLoaders)
            {
                loader.Language = systemLanguage;
            }
            AudioClipLoader[] audioClipLoaders = FindObjectsOfType<AudioClipLoader>();
            foreach (AudioClipLoader loader in audioClipLoaders)
            {
                loader.Language = systemLanguage;
            }
            Debug.Log(string.Format("Language has changed : {0} -> {1}", previousLanguage, language));
            previousLanguage = language;
            if (OnChangeLanguage != null)
            {
                OnChangeLanguage.Invoke();
            }
        }
    }
}
