using System;
using UnityEngine;

namespace UnityEngine.UI
{
    public sealed class TweenSpin : UITweener
    {
        public float FromValue = 0f;

        public float ToValue = 0f;

        private float GetValue()
        {
            return transform.localEulerAngles.z;
        }

        private void SetValue(float value)
        {
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, value);
        }

        protected override void UpdateProgress(float progress)
        {
            SetValue(Mathf.Lerp(FromValue, ToValue, progress));
        }

#if UNITY_EDITOR
        [ContextMenu("Swap 'From' And 'To'")]
        private void SwapValues()
        {
            float temp = FromValue;
            FromValue = ToValue;
            ToValue = temp;
        }

        private void Reset()
        {
            FromValue = GetValue();
            ToValue = GetValue();
        }
#endif

        public static TweenSpin Create(GameObject target, float fromValue, float toValue, float duration, TweeningLoopType loopType = TweeningLoopType.Once, TweeningEaseType easeType = TweeningEaseType.Linear)
        {
            if (target != null)
            {
                TweenSpin tweener = target.GetComponent<TweenSpin>();
                if (tweener == null)
                {
                    tweener = target.AddComponent<TweenSpin>();
                    tweener.enabled = false;
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

        public static TweenSpin Play(GameObject target, float fromValue, float toValue, float duration, TweeningLoopType loopType = TweeningLoopType.Once, TweeningEaseType easeType = TweeningEaseType.Linear)
        {
            TweenSpin tweener = Create(target, fromValue, toValue, duration, loopType, easeType);
            if (tweener != null)
            {
                tweener.PlayForward();
            }
            return tweener;
        }
    }
}
