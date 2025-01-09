using System;
using UnityEngine;

namespace UnityEngine.UI
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class TweenAudioVolume : UITweener
    {
        [Range(0f, 1f)]
        public float FromValue = 0f;

        [Range(0f, 1f)]
        public float ToValue = 1f;

        [NonSerialized]
        private AudioSource _audioSource;
        private AudioSource audioSource
        {
            get
            {
                if (_audioSource == null)
                {
                    _audioSource = GetComponent<AudioSource>();
                }
                return _audioSource;
            }
        }

        private float GetValue()
        {
            return audioSource.volume;
        }

        private void SetValue(float value)
        {
            audioSource.volume = value;
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

        public static TweenAudioVolume Create(GameObject target, float fromValue, float toValue, float duration, TweeningLoopType loopType = TweeningLoopType.Once, TweeningEaseType easeType = TweeningEaseType.Linear)
        {
            if (target != null)
            {
                TweenAudioVolume tweener = target.GetComponent<TweenAudioVolume>();
                if (tweener == null)
                {
                    tweener = target.AddComponent<TweenAudioVolume>();
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

        public static TweenAudioVolume Play(GameObject target, float fromValue, float toValue, float duration, TweeningLoopType loopType = TweeningLoopType.Once, TweeningEaseType easeType = TweeningEaseType.Linear)
        {
            TweenAudioVolume tweener = Create(target, fromValue, toValue, duration, loopType, easeType);
            if (tweener != null)
            {
                tweener.PlayForward();
            }
            return tweener;
        }
    }
}
