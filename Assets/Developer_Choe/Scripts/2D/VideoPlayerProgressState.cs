using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoPlayerProgressState : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public VideoPlayer CurrentVideoPlayer;

    public Image FillImage;

    private Coroutine coroutine;

    private bool isStop = false;
    public bool isPaused = false;
    private double currentTime;

    private void Start()
    {
        if (CurrentVideoPlayer != null)
        {
            CurrentVideoPlayer.sendFrameReadyEvents = true;
            CurrentVideoPlayer.frameReady += CurrentVideoPlayer_frameReady;
        }
    }
    private void OnEnable()
    {
        coroutine = StartCoroutine(UpdateProgressive());
    }

    private void CurrentVideoPlayer_frameReady(VideoPlayer source, long frameIdx)
    {
        if(isStop && source.time >= currentTime)
        {
            isStop = false;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        TimerReset();
        FillImage.fillAmount = eventData.position.x / 3840;
        currentTime = CurrentVideoPlayer.length * FillImage.fillAmount;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        TimerReset();
        CurrentVideoPlayer.Pause();
        isStop = true;
    }
    private void TimerReset()
    {
        //PanelManager.Instance.CurrentTime = PanelSettings.ReturnTime;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        TimerReset();
        FillImage.fillAmount = eventData.position.x / 3840;
        currentTime = CurrentVideoPlayer.length * FillImage.fillAmount;
        CurrentVideoPlayer.time = currentTime;
        if(!isPaused)
            CurrentVideoPlayer.Play();
        FillImage.fillAmount = (float)currentTime / (float)CurrentVideoPlayer.length;
    }
    IEnumerator UpdateProgressive()
    {
        while (isActiveAndEnabled)
        {
            yield return new WaitForEndOfFrame();
            if (!isStop)
                FillImage.fillAmount = (float)CurrentVideoPlayer.time/ (float)CurrentVideoPlayer.length;
        }
    }
}
