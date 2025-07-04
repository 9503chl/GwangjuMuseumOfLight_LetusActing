using NUnit.Framework.Constraints;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimationPanel : View
{
    [SerializeField] private AnimatorTarget animatorTarget;

    [SerializeField] private Button skipBtn;

    private Coroutine coroutine;


    private void Awake()
    {
        OnBeforeShow += AnimationPanel_OnBeforeShow;
        OnBeforeHide += AnimationPanel_OnBeforeHide;

        if (skipBtn != null)
            skipBtn.onClick.AddListener(Skip);
    }

    private void AnimationPanel_OnBeforeHide()
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
    }

    private void AnimationPanel_OnBeforeShow()
    {
        if(coroutine == null)
            coroutine = StartCoroutine(WaitForAnimation());
    }

    private IEnumerator WaitForAnimation()
    {
        float time = 0.5f;

        if (animatorTarget != null)
            time = animatorTarget.GetAnimationTime();

        yield return new WaitForEndOfFrame();

        yield return new WaitForSeconds(time);

        coroutine = null;
        Skip();
    }

    private void Skip()
    {
        BaseManager.Instance.ActiveView = ViewKind.Content;
    }
}
