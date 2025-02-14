using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using LitJson;
using WebSocketSharp;

public enum ViewKind
{
    Title,
    Content,
    Capture,
    Finish,
    Size
}

public class BaseManager : PivotalManager
{
    public static TitlePanel titlePanel;
    private static ContentPanel contentPanel;
    private static CapturePanel capturePanel;
    [SerializeField] private FinishPanel finishPanel;
    [SerializeField] private BackToTitlePanel backToTitlePanel;
    private static Button backToTitleBtn;

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

    public static ViewKind ActiveView
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

    public override void OnAwake()
    {
        // 시스템 설정을 로드
        if (!ProjectSettings.LoadFromXml())
        {
            ProjectSettings.SaveToXml();
        }
        titlePanel = FindObjectOfType<TitlePanel>(true);
        backToTitleBtn = FindObjectOfType<BackToTitleBtn>(true).GetComponent<Button>();
        contentPanel = FindObjectOfType<ContentPanel>(true);
        capturePanel = FindObjectOfType<CapturePanel>(true);
        backToTitleBtn.onClick.AddListener(() => backToTitlePanel.Show());

        Application.targetFrameRate = 24;

        // 모든 뷰를 딕셔너리에 넣은 후 숨김
        views.Add(ViewKind.Title, titlePanel);
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

        base.OnAwake();
    }

    public override void OnStart()
    {
        if (serialPort != null)
        {
            serialPort.PortName = ProjectSettings.SerialPortName;
            serialPort.BaudRate = ProjectSettings.SerialBoundRate;
            serialPort.OnRead += SerialPort_OnRead;
            serialPort.OnOpen += SerialPort_OnOpen;
            serialPort.OnClose += SerialPort_OnClose;
            serialPort.OnReadText += SerialPort_OnReadText1;
            serialPort.Open();
        }

        // 타이틀 화면에서 시작
        ChangeActiveView(ViewKind.Title);

        backToTitlePanel.Hide();
        titlePanel.Show();

        base.OnStart();
    }

    private void SerialPort_OnReadText1(string text)
    {
        if (titlePanel.gameObject.activeInHierarchy)
        {
            titlePanel.LoadQRCode(text);
        }
    }

    private void SerialPort_OnClose()
    {
        DebugLog(string.Format("{0} is Closed", serialPort.PortName));
    }

    private void SerialPort_OnOpen()
    {
        DebugLog(string.Format("{0} is Opened", serialPort.PortName));
    }

    private void SerialPort_OnReadText(string text)
    {
       
    }

    private void SerialPort_OnRead(byte[] buffer)
    {
        string @string = Encoding.ASCII.GetString(buffer);
        if (titlePanel.gameObject.activeInHierarchy)
        {
            titlePanel.LoadQRCode(@string);
        }
    }

    public static void StartTimer()
    {
        if(coroutine != null)
            coroutine = Instance.StartCoroutine(ITimer());
    }

    public static void ResetTimer()
    {
        time = 0;

    }

    private static IEnumerator ITimer()
    {
        while (time < ProjectSettings.BackToTitleTime)
        {
            time += Time.fixedDeltaTime;
            Debug.Log(time);
            yield return null;
        }
        ActiveView = ViewKind.Title;
        coroutine = null;
    }

    public static void StopTimer()
    {
        if (coroutine != null)
        {
            Instance.StopCoroutine(coroutine);
            coroutine = null;
        }
    }


    public override void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ActiveView = activeView + 1;
        }

        if (Input.anyKeyDown)
        {
            ResetTimer();
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            if (titlePanel.gameObject.activeInHierarchy)
            {
                string temp = "OrjdVf8OBblHFDxQvPENnb3BIXY39xAPY9ec4uZ9z7scfN3p7J913LFVliX3INTzEW1vdsH87BGhZZ+4d8jl6WlRvchcqNRUGpKWHKendc8CwMwh/q3xhn6yl/0S8zKyqjN2ei+KhbxiaXREl2bYouvjC1vOZWFK3x2/E+5ufuPdhufqflkw2CjNM3PWQlDXCE868EZT4LiFW4piiSz6CiIcHC4U1LVIhlHxCJwxAcWSkdfYKrFc8OuzxebppJAHevuqbUGylh06Q/8WpW2bK/i4d9oX3tz6ERyJEcnD1aZ52urhfDUhIDMMydGIpXFg2fR1w4XHxtzcLSglR6+ILg==";
                WebServerUtility.user_info = temp;
                titlePanel.LoadQRCode(temp);
            }
        }

        base.OnUpdate();
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

    public static void ChangeActiveView(ViewKind targetView)
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
