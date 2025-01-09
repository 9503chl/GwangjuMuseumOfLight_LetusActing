using System;
using UnityEngine;

namespace UnityEngine.UI
{
    public sealed class TweenScale : UITweener
    {
        public Vector3 FromValue = Vector3.one;

        public Vector3 ToValue = Vector3.one;

        public bool FreezeX = false;
        public bool FreezeY = false;
        public bool FreezeZ = false;

        private Vector3 GetValue()
        {
            return transform.localScale;
        }

        private void SetValue(Vector3 value)
        {
            if (FreezeX) value.x = transform.localScale.x;
            if (FreezeY) value.y = transform.localScale.y;
            if (FreezeZ) value.z = transform.localScale.z;
            transform.localScale = value;
        }

        protected override void UpdateProgress(float progress)
        {
            SetValue(Vector3.Lerp(FromValue, ToValue, progress));
        }

#if UNITY_EDITOR
        [ContextMenu("Swap 'From' And 'To'")]
        private void SwapValues()
        {
            Vector3 temp = FromValue;
            FromValue = ToValue;
            ToValue = temp;
        }

        private void Reset()
        {
            FromValue = GetValue();
            ToValue = GetValue();
        }
#endif

        public static TweenScale Create(GameObject target, Vector3 fromValue, Vector3 toValue, float duration, TweeningLoopType loopType = TweeningLoopType.Once, TweeningEaseType easeType = TweeningEaseType.Linear)
        {
            if (target != null)
            {
                TweenScale tweener = target.GetComponent<TweenScale>();
                if (tweener == null)
                {
                    tweener = target.AddComponent<TweenScale>();
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

        public static TweenScale Play(GameObject target, Vector3 fromValue, Vector3 toValue, float duration, TweeningLoopType loopType = TweeningLoopType.Once, TweeningEaseType easeType = TweeningEaseType.Linear)
        {
            TweenScale tweener = Create(target, fromValue, toValue, duration, loopType, easeType);
            if (tweener != null)
            {
                tweener.PlayForward();
            }
            return tweener;
        }
    }
}
