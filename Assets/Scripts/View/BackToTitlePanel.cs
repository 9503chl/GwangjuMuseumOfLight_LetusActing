using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackToTitlePanel : View
{
    [SerializeField] private Button backToTitleBtn;
    [SerializeField] private Button cancelBtn;

    private void Awake()
    {
        OnBeforeShow += BackToTitlePanel_OnBeforeShow;
        OnBeforeHide += BackToTitlePanel_OnBeforeHide;
        OnAfterHide += BackToTitlePanel_OnAfterHide;
        OnAfterShow += BackToTitlePanel_OnAfterShow;

        backToTitleBtn.onClick.AddListener(BackToTitle);
        cancelBtn.onClick.AddListener(Cancel);
    }

    private void BackToTitlePanel_OnAfterShow()
    {
        Time.timeScale = 0;
    }

    private void BackToTitlePanel_OnAfterHide()
    {

    }

    private void BackToTitle()
    {
        BaseManager.ActiveView = ViewKind.Title;
        WebServerUtility.E3Data.Clear();
        Hide();
    }
    private void Cancel()
    {
        Hide();
    }

    private void BackToTitlePanel_OnBeforeShow()
    {
    }
    private void BackToTitlePanel_OnBeforeHide()
    {
        Time.timeScale = 1;
    }
}
