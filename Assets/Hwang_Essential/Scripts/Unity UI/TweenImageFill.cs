using System;
using UnityEngine;

namespace UnityEngine.UI
{
    [RequireComponent(typeof(Image))]
    public sealed class TweenImageFill : UITweener
    {
        [Range(0f, 1f)]
        public float FromValue = 0f;

        [Range(0f, 1f)]
        public float ToValue = 1f;

        [NonSerialized]
        private Image _image;
        private Image image
        {
            get
            {
                if (_image == null)
                {
                    _image = GetComponent<Image>();
                }
                return _image;
            }
        }

        private float GetValue()
        {
            return image.fillAmount;
        }

        private void SetValue(float value)
        {
            if (image.type == Image.Type.Filled)
            {
                image.fillAmount = value;
            }
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

        public static TweenImageFill Create(GameObject target, float fromValue, float toValue, float duration, TweeningLoopType loopType = TweeningLoopType.Once, TweeningEaseType easeType = TweeningEaseType.Linear)
        {
            if (target != null)
            {
                TweenImageFill tweener = target.GetComponent<TweenImageFill>();
                if (tweener == null)
                {
                    tweener = target.AddComponent<TweenImageFill>();
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

        public static TweenImageFill Play(GameObject target, float fromValue, float toValue, float duration, TweeningLoopType loopType = TweeningLoopType.Once, TweeningEaseType easeType = TweeningEaseType.Linear)
        {
            TweenImageFill tweener = Create(target, fromValue, toValue, duration, loopType, easeType);
            if (tweener != null)
            {
                tweener.PlayForward();
            }
            return tweener;
        }
    }
}
