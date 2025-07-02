using DG.Tweening;
using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class ContentPanel : View
{
    public static ContentPanel Instance;

    [NonSerialized]
    private Coroutine standByCoroutine;

    private BodyDataLoader[] loaders;

    [SerializeField] private ButtonGroup CheckAnimationBtnGroup;
    //[SerializeField] private ButtonGroup CaptureFirstBtnGroup;
    [SerializeField] private ButtonGroup CaptureAgainBtnGroup;

    [SerializeField] private Button captureBtn;
    [SerializeField] private Button saveBtn;

    [SerializeField] private GameObject savePopup;
    [SerializeField] private GameObject randomPopup;

    [SerializeField] private GameObject[] PlayBtnGroup;

    private CanvasGroup canvasGroup;

    private int typeIndex = 0;


    private void Awake()
    {
        OnBeforeShow += View_BeforeShow;
        OnAfterShow += View_AfterShow;
        OnBeforeHide += View_BeforeHide;
        OnAfterHide += View_AfterHide;

        CheckAnimationBtnGroup.onClick.AddListener(() => PlayAnimation(CheckAnimationBtnGroup.SelectedIndex));
        ///CaptureFirstBtnGroup.onClick.AddListener(() => Capture(CaptureFirstBtnGroup.SelectedIndex));
        CaptureAgainBtnGroup.onClick.AddListener(() => Capture(CaptureAgainBtnGroup.SelectedIndex));

        captureBtn.onClick.AddListener(delegate { BaseManager.Instance.ActiveView = ViewKind.Capture; });
        saveBtn.onClick.AddListener(Save);

        canvasGroup = PlayBtnGroup[0].GetComponentInParent<CanvasGroup>();

        Instance = this;
    }

    private void View_BeforeShow()
    {
        if (standByCoroutine != null)
        {
            StopCoroutine(standByCoroutine);
            standByCoroutine = null;
        }
        standByCoroutine = StartCoroutine(Standby());

        canvasGroup.DOFade(1, FadeDuration);

        PanelSetting();
    }

    private void PanelSetting()
    {
        if (WebServerUtility.E3Data.characterType != string.Empty)
        {
            switch (WebServerUtility.E3Data.characterType)
            {
                case "Girl_1": typeIndex = 0; break;
                case "Boy_2": typeIndex = 1; break;
                case "Girl_3": typeIndex = 2; break;
                case "Boy_4": typeIndex = 3; break;
                case "Girl_5": typeIndex = 4; break;
                case "Boy_6": typeIndex = 5; break;
            }
        }

        ObjectManager.Instance.IntializeObject();
        ObjectManager.Instance.TextureInitialize();

        loaders = ObjectManager.Instance.groups[typeIndex].Loaders;

        captureBtn.gameObject.SetActive(true);
        saveBtn.gameObject.SetActive(false);
        savePopup.gameObject.SetActive(false);
        randomPopup.SetActive(false);

        for (int i = 0; i < 5; i++)
        {
            CheckAnimationBtnGroup[i].interactable = false;
            PlayBtnOnOff(i, false);
            //CaptureFirstBtnGroup[i].gameObject.SetActive(true);
            CaptureAgainBtnGroup[i].gameObject.SetActive(false);
        }

        BodyDataList[] dataList = WebServerUtility.dataArray;

        int count = 0;

        for (int i = 0; i < dataList.Length; i++)
        {
            if (dataList[i].FrameCount != 0)
            {
                ButtonInit(i);
                count++;
            }
        }

        if (count == 5)
        {
            captureBtn.gameObject.SetActive(false);
            saveBtn.gameObject.SetActive(true);
        }
    }
    private void ButtonInit(int index)
    {
        CheckAnimationBtnGroup[index].interactable = true;
        PlayBtnOnOff(index, true);
        //CaptureFirstBtnGroup[index].gameObject.SetActive(false);
        CaptureAgainBtnGroup[index].gameObject.SetActive(true);
    }

    public void PlayBtnOnOff(int index, bool @true)
    {
        PlayBtnGroup[index].SetActive(@true);
    }


    private void PlayAnimation(int index)
    {
        if (CheckAnimationBtnGroup[index].interactable == true)
        {
            loaders[index].PlayData();
            WebServerUtility.captureIndex = index;
        }
    }

    private void Capture(int index)
    {
        WebServerUtility.captureIndex = index;
        BaseManager.Instance.ActiveView = ViewKind.Capture;
    }

    private void Save()
    {
        savePopup.SetActive(true);

        string[] strings = new string[5];

        int index = 0;

        switch (WebServerUtility.E3Data.characterType)
        {
            case "Girl_1": index = 0; break;
            case "Boy_2": index = 1; break;
            case "Girl_3": index = 2; break;
            case "Boy_4": index = 3; break;
            case "Girl_5": index = 4; break;
            case "Boy_6": index = 5; break;
        }

        for (int i = 0; i < strings.Length; i++)
        {
            strings[i] = WebServerUtility.dataArray[i].json;
        }

        StartCoroutine(WaitForSave());
    }

    private IEnumerator WaitForSave()
    {
        yield return new WaitForSeconds(1.5f);

        savePopup.SetActive(false);
        randomPopup.SetActive(true);
    }


    private void View_AfterShow()
    {
        BaseManager.Instance.StartTimer();
    }

    private void View_BeforeHide()
    {
        canvasGroup.DOFade(0, FadeDuration);
        for (int i = 0; i < 5; i++)
        {
            PlayBtnOnOff(i, false);
        }
    }

    private void View_AfterHide()
    {

    }

    private IEnumerator Standby()
    {
        while (isActiveAndEnabled)
        {
            yield return new WaitForSeconds(60f);
            //BaseManager.Instance.ActiveView = ViewKind.Title;
        }
    }
}
