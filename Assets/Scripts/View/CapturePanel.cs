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
    private Sprite[] BGSprites;

    [SerializeField]
    private Sprite[] smallBGSprites;


    [Tooltip("인덱스 별 변화할 배경 ex) ----동작을 취해보세요")]
    [SerializeField]
    private Image bgImage;

    [Tooltip("소배경")]
    [SerializeField]
    private Image smallBGImage;

    [Tooltip("캐릭터 애니메이션 이미지")]

    private Coroutine coroutine;

    private Animator animator_Sub;

    private int captrueIndex;
    private int typeIndex = 0;


    private void Awake()
    {
        OnBeforeShow += View_BeforeShow;
        OnAfterShow += View_AfterShow;
        OnBeforeHide += View_BeforeHide;
        OnAfterHide += View_AfterHide;
    }

    private void View_BeforeShow()
    {
        if (standByCoroutine != null)
        {
            StopCoroutine(standByCoroutine);
            standByCoroutine = null;
        }
        standByCoroutine = StartCoroutine(Standby());

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

        if (WebServerUtility.isAll)
            coroutine = StartCoroutine(IStart_All());
        else 
            coroutine = StartCoroutine(IStart_Seperate());
    }


    private IEnumerator IStart_All()
    {
        for(int i = 0; i < 5; i++)
        {
            WebServerUtility.captureIndex = i;

            //ObjectManager.Instance.AnimationInvoke();
            ObjectManager.Instance.IntializeObject();

            captrueIndex = i;

            //popupImage.sprite = popupSprites[captrueIndex];
            bgImage.sprite = BGSprites[captrueIndex];
            smallBGImage.sprite = smallBGSprites[captrueIndex];

            progressText.text = string.Format("00:00");
            slider.FillValue = 0;
            //TextChange(0);

            //popupImage.transform.parent.gameObject.SetActive(true);//부모 참조가 빠르다 -> 트리 구조
            //slider.gameObject.SetActive(false);
            //SubImage.gameObject.SetActive(false);

            //yield return new WaitForSeconds(3);

            //popupImage.transform.parent.gameObject.SetActive(false);
            //SubImage.gameObject.SetActive(true);

            if (animator_Sub != null)
            {
                switch (captrueIndex)
                {
                    case 0:
                        animator_Sub.Play("Yippee");
                        break;
                    case 1:
                        animator_Sub.Play("Dance");
                        break;
                    case 2:
                        animator_Sub.Play("Angry");
                        break;
                    case 3:
                        animator_Sub.Play("Surprise");
                        break;
                    case 4:
                        animator_Sub.Play("Sad");
                        break;
                }
            }
            BaseManager.SoundStop("SE01");
            BaseManager.SoundPlay("Countdown");

            for (int j = 0; j < 5; j++)
            {
                activeGroup.ActivedIndex = j;
                yield return new WaitForSeconds(1);
            }
            activeGroup.ActivedIndex = -1;

            slider.gameObject.SetActive(true);
            BaseManager.SoundPlay("SE01");
            saver.SaveData();

            float time = 0;

            while (time <= ProjectSettings.TargetTime)
            {
                progressText.text = string.Format("{0:00}:{1:00}", (int)time, time * 100 % 99);//정수 2자리 : 소수점 2자리
                time += Time.deltaTime;
                slider.FillValue = time / ProjectSettings.TargetTime;

                yield return null;
            }

            progressText.text = string.Format("{0:D2}:00", (int)(ProjectSettings.TargetTime));
            BaseManager.SoundStop("SE01");
        }
        BaseManager.Instance.ActiveView = ViewKind.Content;
        WebServerUtility.isAll = false;
        coroutine = null;
    }

    private IEnumerator IStart_Seperate()
    {
        ObjectManager.Instance.AnimationInvoke();
        ObjectManager.Instance.IntializeObject();

        captrueIndex = WebServerUtility.captureIndex;

        //popupImage.sprite = popupSprites[captrueIndex];
        bgImage.sprite = BGSprites[captrueIndex];
        smallBGImage.sprite = smallBGSprites[captrueIndex];

        progressText.text = string.Format("00:00");
        slider.FillValue = 0;

        //TextChange(0);

        //popupImage.transform.parent.gameObject.SetActive(true);//부모 참조가 빠르다 -> 트리 구조
        //slider.gameObject.SetActive(false);
        //SubImage.gameObject.SetActive(false);

        //yield return new WaitForSeconds(3);

        //popupImage.transform.parent.gameObject.SetActive(false);
        //SubImage.gameObject.SetActive(true);

        if (animator_Sub != null)
        {
            switch (captrueIndex)
            {
                case 0:
                    animator_Sub.Play("Yippee");
                    break;
                case 1:
                    animator_Sub.Play("Dance");
                    break;
                case 2:
                    animator_Sub.Play("Angry");
                    break;
                case 3:
                    animator_Sub.Play("Surprise");
                    break;
                case 4:
                    animator_Sub.Play("Sad");
                    break;
            }
        }
        BaseManager.SoundPlay("Countdown");

        for (int i = 0; i < 5; i++)
        {
            activeGroup.ActivedIndex = i;
            yield return new WaitForSeconds(1);
        }
        activeGroup.ActivedIndex = -1;

        slider.gameObject.SetActive(true);
        BaseManager.SoundPlay("SE01");
        saver.SaveData();

        float time = 0;

        while (time <= ProjectSettings.TargetTime)
        {
            progressText.text = string.Format("{0:00}:{1:00}", (int)time, time * 100 % 99);//정수 2자리 : 소수점 2자리
            if(Time.timeScale == 1)
                time += Time.deltaTime;
            slider.FillValue = time / ProjectSettings.TargetTime;

            yield return null;
        }

        progressText.text = string.Format("{0:D2}:00", (int)(ProjectSettings.TargetTime));

        BaseManager.Instance.ActiveView = ViewKind.Content;
        coroutine = null;
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
