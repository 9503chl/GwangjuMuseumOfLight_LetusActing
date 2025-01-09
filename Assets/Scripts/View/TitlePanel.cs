using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TitlePanel : View
{
    [NonSerialized]
    private Coroutine standByCoroutine;

    [SerializeField] private SuccessPanel successPanel;
    [SerializeField] private GameObject failurePanel;

    private Coroutine coroutine_Success;
    private Coroutine coroutine_Failure;

    public void OnSuccess()
    {
        coroutine_Success = StartCoroutine(ISuccess());
    }

    private IEnumerator ISuccess()
    {
        successPanel.gameObject.SetActive(true);
        successPanel.TextInit(DataSettings.PlayerName);
        yield return new WaitForSeconds(3);

        BaseManager.ActiveView = ViewKind.Content;
    }

    public void OnFail()
    {
        coroutine_Failure = StartCoroutine(IFail());
    }

    private IEnumerator IFail()
    {
        failurePanel.gameObject.SetActive(true);

        yield return new WaitForSeconds(3);

        failurePanel.gameObject.SetActive(false);
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
    }

    private void View_AfterShow()
    {

    }

    private void View_BeforeHide()
    {

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
