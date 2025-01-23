using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CapturePanel : View
{
    [NonSerialized]
    private Coroutine standByCoroutine;

    [Tooltip("애니메이션 저장 할 모델")]
    [SerializeField]
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
    [SerializeField]
    private Animator characterAnimator;

    private Coroutine coroutine;

    private int index;

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
        index = ProjectSettings.captureIndex;

        popupImage.sprite = popupSprites[index];
        bgImage.sprite = BGSprites[index];

        switch (index)
        {
            case 0:
                characterAnimator.Play("Viva");
                break;
            case 1:
                characterAnimator.Play("Dance");
                break;
            case 2:
                characterAnimator.Play("Anger");
                break;
            case 3:
                characterAnimator.Play("Surprise");
                break;
            case 4:
                characterAnimator.Play("Sad");
                break;
        }

        TextChange(0);

        popupImage.transform.parent.gameObject.SetActive(true);
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
        for(int i = 0; i<3; i++)
        {
            activeGroup.ActivedIndex = i;
            yield return new WaitForSeconds(1);
        }
        activeGroup.ActivedIndex = -1;

        TextChange(1);

        slider.gameObject.SetActive(true);
        saver.SaveData();

        float time = 0;

        while(time <= ProjectSettings.TargetTime)
        {
            progressText.text = string.Format("{0:00}:{1:00}", (int)time, time * 100 % 99);//정수 2자리 : 소수점 2자리
            time += Time.deltaTime;
            slider.FillValue = time / ProjectSettings.TargetTime;

            yield return null;
        }

        progressText.text = "03:00";

        BaseManager.ActiveView = ViewKind.Content;
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

        characterAnimator.Rebind();//Play의 반대라 생각하면 된다.
        characterAnimator.Update(0f);
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
