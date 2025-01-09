using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TimerBar : MonoBehaviour
{
    public GameObject TimerObject;
    public Image ProgressImage;
    public RectTransform ProgressGauge;
    public RectTransform ProgressThumb;
    public Text RemainTimeText;

    public bool FillImageOverTime = false;
    public MinMaxValue GaugeMinMaxSize = new MinMaxValue(68f, 1584f);
    public MinMaxValue ThumbMinMaxPosX = new MinMaxValue(34f, 1550f);
    public bool ElapsedTimeAsText = false;
    public float LimitedTime = 30f;
    public float ShakingTime = 5f;
    public bool AutoProgress = false;

    public GameObject SecondHand;

    public UnityEvent onTimeOver;

    [NonSerialized]
    private float remainTime = 0f;

    public float RemainTime
    {
        get
        {
            return Mathf.Clamp(remainTime, 0f, LimitedTime);
        }
        set
        {
            remainTime = Mathf.Clamp(value, 0f, LimitedTime);
            Progress();
        }
    }

    [NonSerialized]
    private bool isShaking = false;

    [NonSerialized]
    private bool isTimeOver = false;

    private void OnEnable()
    {
        ResetTimer();
    }

    private void FixedUpdate()
    {
        if (AutoProgress && !isTimeOver)
        {
            if (remainTime > 0f)
            {
                remainTime -= Time.fixedDeltaTime;
                Progress();
            }
            else
            {
                isTimeOver = true;
                if (onTimeOver != null)
                {
                    onTimeOver.Invoke();
                }
            }
        }
    }

    private void Progress()
    {
        if (LimitedTime > 0f)
        {
            remainTime = Mathf.Clamp(remainTime, 0f, LimitedTime);
            if (remainTime < ShakingTime && !isShaking)
            {
                isShaking = true;
                if (TimerObject != null)
                {
                    TweenScale.Play(TimerObject, Vector3.one, Vector3.one * 1.25f, 0.25f, TweeningLoopType.PingPong, TweeningEaseType.ExpoEaseInOut);
                }
            }
            if (ProgressImage != null)
            {
                float progress = remainTime / LimitedTime;
                float t = Mathf.Lerp(0.1f, 0.9f, progress);
                //ProgressImage.fillAmount = FillImageOverTime ? 1 - progress : progress;
                ProgressImage.fillAmount = t;
            }
            if (ProgressGauge != null)
            {
                float width = Mathf.Round(GaugeMinMaxSize.Lerp(remainTime / LimitedTime));
                ProgressGauge.sizeDelta = new Vector2(width, ProgressGauge.sizeDelta.y);
            }
            if (ProgressThumb != null)
            {
                float posX = Mathf.Round(ThumbMinMaxPosX.Lerp(remainTime / LimitedTime));
                ProgressThumb.anchoredPosition = new Vector2(posX, ProgressThumb.anchoredPosition.y);
            }
            if (RemainTimeText != null)
            {
                int minutes = (int)Mathf.RoundToInt(ElapsedTimeAsText ? LimitedTime - remainTime : remainTime) / 60;
                int seconds = (int)Mathf.RoundToInt(ElapsedTimeAsText ? LimitedTime - remainTime : remainTime) % 60;
                RemainTimeText.text = string.Format("{0:D2}:{1:D2}", minutes, seconds);
            }
            if(SecondHand != null)//-45 ~ -315
            {
                float t = Mathf.Lerp(0, 1, remainTime / LimitedTime);
                SecondHand.transform.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(-45, -315, t));
            }
        }
    }

    public void ResetTimer()
    {
        if (TimerObject != null)
        {
            UITweener tweener = TimerObject.GetComponent<UITweener>();
            if (tweener != null)
            {
                tweener.Finish();
            }
        }
        remainTime = LimitedTime;
        isTimeOver = false;
        isShaking = false;
        Progress();
    }
}
