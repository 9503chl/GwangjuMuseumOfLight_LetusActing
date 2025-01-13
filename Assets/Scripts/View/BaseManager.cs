using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using LitJson;
using WebSocketSharp;
using Unity.Content;

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

    private static BodyDataSaver bodyDataSaver;

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
    private SyncWebSocketClient WebSocketClient;

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
        bodyDataSaver = GetComponentInChildren<BodyDataSaver>(true);
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

    public static void SetPanelsModelOnOff(bool @true)
    {
        contentPanel.ModelsOnOff(@true);
        capturePanel.ModelOnOff(@true);
    }

    public override void OnStart()
    {
        if (WebSocketClient != null)
        {
            WebSocketClient.WebSocketURL = ProjectSettings.SignalWebSocketUrl;
            WebSocketClient.OnConnect += SignalWebSocketClient_OnConnect;
            WebSocketClient.OnDisconnect += SignalWebSocketClient_OnDisconnect;
            WebSocketClient.OnReceiveText += SignalWebSocketClient_OnReceiveText;
            WebSocketClient.Connect();
        }

        bodyDataSaver.gameObject.SetActive(false);

        // 타이틀 화면에서 시작
        ChangeActiveView(ViewKind.Title);
        StartCoroutine(Initialize());

        backToTitlePanel.Hide();
        titlePanel.Show();

        base.OnStart();
    }

    public static void StartTimer()
    {
        coroutine = Instance.StartCoroutine(ITimer());
    }

    private static void ResetTimer()
    {
        time = 0;
    }

    private static IEnumerator ITimer()
    {
        while (true)
        {
            if (time < ProjectSettings.BackToTitleTime)
            {
                ActiveView = ViewKind.Title;
                coroutine = null;
                break;
            }
            time += Time.deltaTime;
            yield return null;
        }
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

        if (Input.anyKey)
        {
            ResetTimer();
        }

        base.OnUpdate();
    }
    private void SignalWebSocketClient_OnConnect()
    {
        
    }

    private void SignalWebSocketClient_OnDisconnect()
    {
       
    }

    private void SignalWebSocketClient_OnReceiveText(string message)
    {
        string json = JsonUtility.ToJson(message, true);
        JsonData data = JsonMapper.ToObject(json);
        if (data.ContainsKey("user_id") && data.ContainsKey("student_id"))
        {
            ProjectSettings.PlayerID = data["user_id"].ToString();
            WebServerUtility.Instance.ApiGet();
        }
    }


    private IEnumerator Initialize()
    {
        yield return null;

        if (WebSocketClient != null)
        {
            while (WebSocketClient.Connecting)
            {
                yield return null;
            }

            if (!WebSocketClient.Connected)
            {
                int boxID = MessageBox.Show("웹소켓 서버에 연결할 수 없습니다.", "프로그램 종료", "무시하고 진행", "오류", 0f, ApplicationQuit);
                while (!WebSocketClient.Connected)
                {
                    yield return null;
                }
                MessageBox.Close(boxID);
            }
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
