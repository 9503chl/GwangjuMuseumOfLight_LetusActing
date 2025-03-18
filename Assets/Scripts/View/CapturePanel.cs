using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CapturePanel : View
{
    public static CapturePanel Instance;

    [NonSerialized]
    private Coroutine standByCoroutine;

    private BodyDataSaver saver;

    [SerializeField]
    private ActiveGroup activeGroup;

    [SerializeField]
    private ProgressSliderImage slider;

    [SerializeField]
    private Text progressText;

    [SerializeField]
    private Button GetCaptureBtn;

    [SerializeField]
    private Sprite[] textSprites;

    [SerializeField]
    private Sprite[] popupSprites;

    [SerializeField]
    private Sprite[] BGSprites;

    [Tooltip("인덱스 별 변화할 팝업 ex) ----포즈")]
    [SerializeField]
    private Image popupImage;

    [Tooltip("인덱스 별 변화할 배경 ex) ----동작을 취해보세요")]
    [SerializeField]
    private Image bgImage;

    [Tooltip("촬영 시 변화 할 텍스트 이미지")]
    [SerializeField]
    private Image textImage;

    [Tooltip("캐릭터 애니메이션 이미지")]

    private Animator[] characterAnimators;

    private Coroutine coroutine;


    private int captrueIndex;
    private int typeIndex = 0;


    private void Awake()
    {
        OnBeforeShow += View_BeforeShow;
        OnAfterShow += View_AfterShow;
        OnBeforeHide += View_BeforeHide;
        OnAfterHide += View_AfterHide;

        GetCaptureBtn.onClick.AddListener(Capture);
    }

    private void View_BeforeShow()
    {
        if (standByCoroutine != null)
        {
            StopCoroutine(standByCoroutine);
            standByCoroutine = null;
        }
        standByCoroutine = StartCoroutine(Standby());

        coroutine = StartCoroutine(IStart());
    }

    private IEnumerator IStart()
    {
        if (WebServerUtility.E3Data.characterType != string.Empty)
            switch (WebServerUtility.E3Data.characterType)
            {
                case "Girl_1": typeIndex = 0; break;
                case "Boy_2": typeIndex = 1; break;
                case "Girl_3": typeIndex = 2; break;
                case "Boy_4": typeIndex = 3; break;
                case "Girl_5": typeIndex = 4; break;
                case "Boy_6": typeIndex = 5; break;
            }

        saver = ObjectManager.Instance.groups[typeIndex].Saver;

        ObjectManager.Instance.AnimationInvoke();
        ObjectManager.Instance.IntializeObject();

        captrueIndex = WebServerUtility.captureIndex;

        popupImage.sprite = popupSprites[captrueIndex];
        bgImage.sprite = BGSprites[captrueIndex];

        TextChange(0);

        popupImage.transform.parent.gameObject.SetActive(true);//부모 참조가 빠르다 -> 트리 구조
        GetCaptureBtn.gameObject.SetActive(true);
        slider.gameObject.SetActive(false);

        yield return new WaitForSeconds(5);

        popupImage.transform.parent.gameObject.SetActive(false);
        coroutine = null;
    }
    private void TextChange(int index)
    {
        textImage.sprite = textSprites[index];
        textImage.SetNativeSize();
    }
    private void Capture()
    {
        GetCaptureBtn.gameObject.SetActive(false);
        StartCoroutine(ICapture());
    }

    private IEnumerator ICapture()
    {
        BaseManager.SoundPlay("Countdown");

        for(int i = 0; i < ProjectSettings.TargetTime; i++)
        {
            activeGroup.ActivedIndex = i;
            yield return new WaitForSeconds(1);
        }
        activeGroup.ActivedIndex = -1;

        TextChange(1);

        slider.gameObject.SetActive(true);
        BaseManager.SoundPlay("SE01");
        saver.SaveData();

        float time = 0;

        while(time <= ProjectSettings.TargetTime)
        {
            progressText.text = string.Format("{0:00}:{1:00}", (int)time, time * 100 % 99);//정수 2자리 : 소수점 2자리
            time += Time.deltaTime;
            slider.FillValue = time / ProjectSettings.TargetTime;

            yield return null;
        }

        progressText.text = string.Format("{0:D2}:00", (int)(ProjectSettings.TargetTime));

        BaseManager.Instance.ActiveView = ViewKind.Content;
    }

    private void View_AfterShow()
    {

    }

    private void View_BeforeHide()
    {
        if(coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
        BaseManager.SoundStop("SE01");
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
