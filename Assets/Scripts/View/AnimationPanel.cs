using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimationPanel : View
{
    [SerializeField] private AnimatorTarget animatorTarget;

    private Coroutine coroutine;


    private void Awake()
    {
        OnBeforeShow += AnimationPanel_OnBeforeShow;
        OnBeforeHide += AnimationPanel_OnBeforeHide;
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

        yield return new WaitForSeconds(time);

        BaseManager.Instance.ActiveView = ViewKind.Content;
        coroutine = null;
    }
}
