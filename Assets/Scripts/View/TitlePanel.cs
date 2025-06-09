using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TitlePanel : View
{
    [NonSerialized]
    private Coroutine standByCoroutine;

    [SerializeField] private View messagePopup;
    [SerializeField] private Text messageText;

    [SerializeField] private GameObject failurePanel;
    [SerializeField] private SuccessPanel successPanel;

    private Coroutine loadingRoutine;


    private IEnumerator Loading(string text)
    {
        // 사용자 이름은 모르지만 일단 팝업부터 표시
        successPanel.Show();
        successPanel.TextInit("\n환영합니다");
        yield return null;

        // API를 호출하여 사용자 데이터 수신
        WebServerData myData = WebServerUtility.E3Data;
        yield return StartCoroutine(WebServerUtility.E3Get(text));

        if (myData.success)
        {
            // 사용자 이름을 표시
           
            successPanel.TextInit(string.Format("<color=#F47E3D>{0}</color> 님\n환영합니다", myData.userName));
            yield return null;

            yield return StartCoroutine(WebServerUtility.E3Download(this));

            if (myData.hasMaterialTextures) // E3
            {
                yield return new WaitForSeconds(2);

                BaseManager.Instance.ActiveView = ViewKind.Content;
            }
            else
            {
                successPanel.Hide();
                failurePanel.gameObject.SetActive(true);

                yield return new WaitForSeconds(3);

                failurePanel.gameObject.SetActive(false);
            }
        }
        else
        {
            // API 서버 오류
            Debug.Log(string.Format("{0} (Error code : {1})", myData.message, myData.errorCode));
            
            messageText.text = myData.message;
            messagePopup.Show();
            yield return new WaitForSeconds(3f);
            messagePopup.Hide();
        }
        successPanel.Hide();
        loadingRoutine = null;
    }

    public void LoadQRCode(string text)
    {
        if (loadingRoutine == null)
        {
            loadingRoutine = StartCoroutine(Loading(text));
        }
    }


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

        messagePopup.Hide();
        successPanel.Hide();
        failurePanel.gameObject.SetActive(false);

        BaseManager.SoundStop("BGM 02");
        BaseManager.Instance.StopTimer();
    }

    private void View_AfterShow()
    {

    }

    private void View_BeforeHide()
    {

    }

    private void View_AfterHide()
    {
        BaseManager.SoundPlay("BGM 02");
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
