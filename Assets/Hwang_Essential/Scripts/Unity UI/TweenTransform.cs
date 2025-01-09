using System;
using UnityEngine;

namespace UnityEngine.UI
{
    public sealed class TweenTransform : UITweener
    {
        public Transform FromValue = null;

        public Transform ToValue = null;

        private Transform GetValue()
        {
            return null;
        }

        private void SetValue(Vector3 position, Vector3 eulerAngles, Vector3 scale)
        {
            transform.position = position;
            transform.eulerAngles = eulerAngles;
            transform.localScale = scale;
        }

        private void SetValue(Transform value)
        {
            if (value != null)
            {
                transform.position = value.position;
                transform.eulerAngles = value.eulerAngles;
                transform.localScale = value.localScale;
            }
        }

        protected override void UpdateProgress(float progress)
        {
            if (FromValue != null && ToValue != null)
            {
                Vector3 position = Vector3.Lerp(FromValue.position, ToValue.position, progress);
                Vector3 eularAngles = Vector3.Lerp(FromValue.eulerAngles, ToValue.eulerAngles, progress);
                Vector3 scale = Vector3.Lerp(FromValue.localScale, ToValue.localScale, progress);
                SetValue(position, eularAngles, scale);
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Swap 'From' And 'To'")]
        private void SwapValues()
        {
            Transform temp = FromValue;
            FromValue = ToValue;
            ToValue = temp;
        }

        private void Reset()
        {
            FromValue = GetValue();
            ToValue = GetValue();
        }
#endif

        public static TweenTransform Create(GameObject target, Transform fromValue, Transform toValue, float duration, TweeningLoopType loopType = TweeningLoopType.Once, TweeningEaseType easeType = TweeningEaseType.Linear)
        {
            if (target != null)
            {
                TweenTransform tweener = target.GetComponent<TweenTransform>();
                if (tweener == null)
                {
                    tweener = target.AddComponent<TweenTransform>();
                    tweener.PlayOnEnable = false;
                }
                else
                {
                    tweener.Stop();
                }
                tweener.FromValue = fromValue;
                tweener.ToValue = toValue;
                tweener.Duration = duration;
                tweener.LoopType = loopType;
                tweener.EaseType = easeType;
                tweener.enabled = true;
                return tweener;
            }
            return null;
        }

        public static TweenTransform Play(GameObject target, Transform fromValue, Transform toValue, float duration, TweeningLoopType loopType = TweeningLoopType.Once, TweeningEaseType easeType = TweeningEaseType.Linear)
        {
            TweenTransform tweener = Create(target, fromValue, toValue, duration, loopType, easeType);
            if (tweener != null)
            {
                tweener.PlayForward();
            }
            return tweener;
        }
    }
}
