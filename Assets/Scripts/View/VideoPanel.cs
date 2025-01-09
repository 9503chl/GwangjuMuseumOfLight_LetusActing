using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoPanel : View //이 친구는 미리 만들어놔서 쓰는게 좋음. 동적으로 생성하면 오류가 잦음.
{
    public Text NoticeText;
    public VideoViewer IntroVideoViewer;
    public Button SkipButton;

    [NonSerialized]
    private VideoPlayer introVideoPlayer;

    [NonSerialized]
    private Coroutine standbyRoutine;

    private void Awake()
    {
        OnBeforeShow += View_BeforeShow;
        OnAfterShow += View_AfterShow;
        OnBeforeHide += View_BeforeHide;
        OnAfterHide += View_AfterHide;

        introVideoPlayer = IntroVideoViewer.SourceVideoPlayer;
        introVideoPlayer.loopPointReached += MissionVideoPlayer_loopPointReached;
        IntroVideoViewer.gameObject.SetActive(true);

        introVideoPlayer.started += IntroVideoPlayer_started;

        SkipButton.onClick.AddListener(SkipButton_Click);
    }

    private void IntroVideoPlayer_started(VideoPlayer source)
    {
        IntroVideoViewer.gameObject.SetActive(true);
    }

    private void View_BeforeShow()
    {
        introVideoPlayer.Prepare();
        IntroVideoViewer.gameObject.SetActive(false);
    }

    private void View_AfterShow()
    {

        if (introVideoPlayer.clip != null)
        {
            introVideoPlayer.Play();
        }
        else
        {
            Debug.LogWarning("No intro video clip assigned. Skip to next mission step.");
            SkipButton_Click();
        }

        standbyRoutine = StartCoroutine(Standby());
    }

    private void View_BeforeHide()
    {
        if (standbyRoutine != null)
        {
            StopCoroutine(standbyRoutine);
            standbyRoutine = null;
        }

        introVideoPlayer.Stop();
    }

    private void View_AfterHide()
    {
    }

    private IEnumerator Standby()
    {
        while (true)
        {
            yield return new WaitForSeconds(60f);
        }
    }

    private void MissionVideoPlayer_loopPointReached(VideoPlayer source)
    {
        //SkipButton_Click();
    }

    private void SkipButton_Click()
    {
        //FutureEducationManager.Instance.ActiveView = ViewKind.Content;
    }
}
