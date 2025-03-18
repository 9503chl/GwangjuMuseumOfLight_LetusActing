using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FinishPanel : View
{
    [NonSerialized]
    private Coroutine standByCoroutine;

    private Button button;

    private void Awake()
    {
        OnBeforeShow += View_BeforeShow;
        OnAfterShow += View_AfterShow;
        OnBeforeHide += View_BeforeHide;
        OnAfterHide += View_AfterHide;

        button = GetComponentInChildren<Button>();
        button.onClick.AddListener(delegate { BaseManager.Instance.ActiveView = ViewKind.Title; });
    }

    private void View_BeforeShow()
    {
        BaseManager.SoundStop("BGM 02");

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
        if (standByCoroutine != null)
        {
            StopCoroutine (standByCoroutine);
            standByCoroutine = null;
        }
    }

    private void View_AfterHide()
    {
        WebServerUtility.Clear();
    }

    private IEnumerator Standby()
    {
        while (isActiveAndEnabled)
        {
            yield return new WaitForSeconds(ProjectSettings.FinishTime);

            BaseManager.Instance.ActiveView = ViewKind.Title;
        }
    }
}
