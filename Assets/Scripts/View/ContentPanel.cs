using DG.Tweening;
using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;
using static UnityEngine.Rendering.VolumeComponent;

public class ContentPanel : View
{
    public static ContentPanel Instance;

    [NonSerialized]
    private Coroutine standByCoroutine;

    [SerializeField] private BodyDataLoader[] loaders;

    [SerializeField] private ButtonGroup CheckAnimationBtnGroup;
    [SerializeField] private ButtonGroup CaptureFirstBtnGroup;
    [SerializeField] private ButtonGroup CaptureAgainBtnGroup;

    [SerializeField] private Button saveBtn;

    [SerializeField] private GameObject savePopup;

    [SerializeField] private GameObject[] PlayBtnGroup;
    private CanvasGroup canvasGroup;

    private int count = 0;

    private void Awake()
    {
        OnBeforeShow += View_BeforeShow;
        OnAfterShow += View_AfterShow;
        OnBeforeHide += View_BeforeHide;
        OnAfterHide += View_AfterHide;

        CheckAnimationBtnGroup.onClick.AddListener(() => PlayAnimation(CheckAnimationBtnGroup.SelectedIndex));
        CaptureFirstBtnGroup.onClick.AddListener(() => Capture(CaptureFirstBtnGroup.SelectedIndex));
        CaptureAgainBtnGroup.onClick.AddListener(() => Capture(CaptureAgainBtnGroup.SelectedIndex));

        saveBtn.onClick.AddListener(Save);

        canvasGroup = PlayBtnGroup[0].GetComponentInParent<CanvasGroup>();

        Instance = FindObjectOfType<ContentPanel>(true);
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
        saveBtn.gameObject.SetActive(false);
        savePopup.gameObject.SetActive(false);
        BaseManager.ResetTimer();

        for (int i = 0; i < 5; i++)
        {
            CheckAnimationBtnGroup[i].interactable = false;
            PlayBtnOnOff(i, false);
            CaptureFirstBtnGroup[i].gameObject.SetActive(true);
            CaptureAgainBtnGroup[i].gameObject.SetActive(false);
        }

        BodyDataList[] dataList = WebServerData.dataArray;

        int count = 0;

        for (int i = 0; i < dataList.Length; i++)
        {
            if (dataList[i].FrameCount != 0)
            {
                ButtonInit(i);
                count++;
            }
        }

        if (count == 5) saveBtn.gameObject.SetActive(true);
    }
    private void ButtonInit(int index)
    {
        CheckAnimationBtnGroup[index].interactable = true;
        PlayBtnOnOff(index, true);
        CaptureFirstBtnGroup[index].gameObject.SetActive(false);
        CaptureAgainBtnGroup[index].gameObject.SetActive(true);
    }

    public void PlayBtnOnOff(int index, bool @true)
    {
        PlayBtnGroup[index].SetActive(@true);
    }


    private void PlayAnimation(int index)
    {
        if (CheckAnimationBtnGroup[index].interactable == true)
            loaders[index].PlayData();
    }

    private void Capture(int index)
    {
        WebServerData.captureIndex = index;
        BaseManager.ActiveView = ViewKind.Capture;
    }

    private void Save()
    {
        savePopup.SetActive(true);

        WebServerUtility.Instance.ApiE3Post(WebServerData.userId, WebServerData.studentId, WebServerData.dataArray);
    }


    private void View_AfterShow()
    {
        BaseManager.StartTimer();
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
