using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

public class ContentPanel : View
{
    [NonSerialized]
    private Coroutine standByCoroutine;

    [SerializeField] private BodyDataLoader[] loaders;

    [SerializeField] private ButtonGroup CheckAnimationBtnGroup;
    [SerializeField] private ButtonGroup CaptureFirstBtnGroup;
    [SerializeField] private ButtonGroup CaptureAgainBtnGroup;

    [SerializeField] private Button saveBtn;

    [SerializeField] private GameObject savePopup;

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
    }

    private void View_BeforeShow()
    {
        if (standByCoroutine != null)
        {
            StopCoroutine(standByCoroutine);
            standByCoroutine = null;
        }
        standByCoroutine = StartCoroutine(Standby());

        PanelSetting();
    }

    private void PanelSetting()
    {
        saveBtn.gameObject.SetActive(false);
        savePopup.gameObject.SetActive(false);

        for (int i = 0; i < 5; i++)
        {
            CheckAnimationBtnGroup[i].interactable = false;
            CaptureFirstBtnGroup[i].gameObject.SetActive(true);
            CaptureAgainBtnGroup[i].gameObject.SetActive(false);
        }
        ModelsOnOff(false);

        BodyDataList[] dataList = ProjectSettings.dataArray;

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

    public void ModelsOnOff(bool @true)
    {
        for (int i = 0; i < loaders.Length; i++)
        {
            loaders[i].gameObject.SetActive(@true);
        }
    }

    private void ButtonInit(int index)
    {
        CheckAnimationBtnGroup[index].interactable = true;
        CaptureFirstBtnGroup[index].gameObject.SetActive(false);
        CaptureAgainBtnGroup[index].gameObject.SetActive(true);
    }

    private void PlayAnimation(int index)
    {
        if (CheckAnimationBtnGroup[index].interactable == true)
            loaders[index].PlayData();
    }

    private void Capture(int index)
    {
        ProjectSettings.captureIndex = index;
        BaseManager.ActiveView = ViewKind.Capture;
    }

    private void Save()
    {
        ModelsOnOff(false);

        savePopup.SetActive(true);

        WebServerUtility.Instance.ApiPost();
    }


    private void View_AfterShow()
    {
        ModelsOnOff(true);

        BaseManager.StartTimer();
    }

    private void View_BeforeHide()
    {
        ModelsOnOff(false);
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
