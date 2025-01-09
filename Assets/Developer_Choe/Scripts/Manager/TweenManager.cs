using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class TweenManager : PivotalManager
{
    private static List<Tween> tweenList = new List<Tween>();
    public static bool DOColor(GameObject target, Color endValue, float duration)
    {
        Graphic graphic = target.GetComponent<Graphic>();

        Tween tween = null;

        if (graphic != null)
        {
            tween = DOTweenModuleUI.DOColor(graphic, endValue, duration);

            if (tweenList.Contains(tween))
            {
                return false;
            }
            tweenList.Add(tween);
        }
        return true;
    }

    public static bool DOFade(GameObject target, float endValue, float duration)
    {
        Graphic graphic = target.GetComponent<Graphic>();

        if (graphic != null)
        {
            Tween tween = DOTweenModuleUI.DOFade(graphic, endValue, duration);
            if (tweenList.Contains(tween))
            {
                return false;
            }
            tweenList.Add(tween);
        }
        return true;
    }

    public static bool DOFillAmount(GameObject target, float endValue, float duration)
    {
        Image image = target.GetComponent<Image>();

        if (image != null)
        {
            Tween tween = DOTweenModuleUI.DOFillAmount(image, endValue, duration);
            if (tweenList.Contains(tween))
            {
                return false;
            }
            tweenList.Add(tween);
        }

        return true;
    }

    public static bool DOMove(GameObject target, Vector3 endValue, float duration, bool isLocal)
    {
        Transform tf = target.transform;
        Rigidbody rigidbody = target.GetComponent<Rigidbody>();

        Tween tween = null;

        if (isLocal && tf != null)
        {
            tween = ShortcutExtensions.DOLocalMove(tf, endValue, duration);
        }
        else if(rigidbody != null) 
        {
            tween = DOTweenModulePhysics.DOMove(rigidbody, endValue, duration);
        }

        if (tween != null && !tweenList.Contains(tween))
        {
            tweenList.Add(tween);
        }
        else return false;

        return true;
    }

    public static bool DORotate(GameObject target, Vector3 endValue, float duration, bool isLocal)
    {
        Transform tf = target.transform;
        Rigidbody rigidbody = target.GetComponent<Rigidbody>();

        Tween tween = null;

        if (isLocal && tf != null)
        {
            tween = ShortcutExtensions.DOLocalRotate(tf, endValue, duration);
        }
        else if(rigidbody != null)
        {
            tween = DOTweenModulePhysics.DORotate(rigidbody, endValue, duration);
        }
        if (tween != null && !tweenList.Contains(tween))
        {
            tweenList.Add(tween);
        }
        else return false;

        return true;
    }

    public static bool DOScale(GameObject target, Vector3 endValue, float duration)
    {
        Transform tf = target.transform;

        Tween tween = null;

        if(tf != null)
        {
            tween = ShortcutExtensions.DOScale(tf, endValue, duration);
            if (tweenList.Contains(tween))
            {
                return false;
            }
            tweenList.Add(tween);
        }

        return true;
    }

    private void ListCheck()
    {
        int count = tweenList.Count;

        for (int i = 0; i< count; i++)
        {
            if(tweenList[i] == null)
            {
                //지워도 되나?
            }
        }
    }
}
