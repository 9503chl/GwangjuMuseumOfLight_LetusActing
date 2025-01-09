using System;
using UnityEngine;
using UnityEngine.Events;

namespace UnityEngine.UI
{
    public enum TweeningEaseType
    {
        Custom,
        Linear,
        BackEaseIn,
        BackEaseOut,
        BackEaseInOut,
        BounceEaseIn,
        BounceEaseOut,
        BounceEaseInOut,
        CircleEaseIn,
        CircleEaseOut,
        CircleEaseInOut,
        CubicEaseIn,
        CubicEaseOut,
        CubicEaseInOut,
        ElasticEaseIn,
        ElasticEaseOut,
        ElasticEaseInOut,
        ExpoEaseIn,
        ExpoEaseOut,
        ExpoEaseInOut,
        QuadEaseIn,
        QuadEaseOut,
        QuadEaseInOut,
        QuartEaseIn,
        QuartEaseOut,
        QuartEaseInOut,
        QuintEaseIn,
        QuintEaseOut,
        QuintEaseInOut,
        SineEaseIn,
        SineEaseOut,
        SineEaseInOut,
    }

    public enum TweeningLoopType
    {
        Once,
        Loop,
        PingPong
    }

    public enum TweeningDirection
    {
        Forward = 1,
        Reverse = -1
    }

    [Serializable]
    public abstract class UITweener : MonoBehaviour
    {
        public TweeningLoopType LoopType = TweeningLoopType.Once;
        public TweeningEaseType EaseType = TweeningEaseType.Linear;

        [HideInInspector]
        public AnimationCurve CustomAnimationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        public float Delay = 0f;
        public float Duration = 3f;
        public bool InitOnPlay = true;
        public bool PlayOnEnable = true;
        public int StopAfterLoop = 0;
        public bool IgnoreTimeScale = true;

        public UnityEvent onFinish = new UnityEvent();

        public event UnityAction<UITweener> OnStartLoop;
        public event UnityAction<UITweener> OnEndLoop;

        [HideInInspector]
        private TweeningDirection direction = TweeningDirection.Forward;
        public TweeningDirection Direction
        {
            get { return direction; }
        }

        [HideInInspector]
        private TweeningDirection initialDirection = TweeningDirection.Forward;

        [NonSerialized]
        private float delayed = 0f;

        [NonSerialized]
        private float current = 0f;

        [NonSerialized]
        protected bool isFirst = true;

        [NonSerialized]
        protected int loopCount = 0;

        [NonSerialized]
        private bool isPlaying = false;
        public bool IsPlaying
        {
            get { return isPlaying; }
        }

        [NonSerialized]
        private RectTransform _rectTransform;
        public RectTransform rectTransform
        {
            get
            {
                if (_rectTransform == null)
                {
                    _rectTransform = GetComponent<RectTransform>();
                }
                return _rectTransform;
            }
        }

#if UNITY_EDITOR
        [HideInInspector]
        [SerializeField]
        private bool validated = false;

        private void OnValidate()
        {
            if (!validated)
            {
                validated = true;
            }
        }
#endif

        protected virtual void OnEnable()
        {
#if UNITY_EDITOR
            if (!validated)
            {
                validated = true;
                SendMessage("Reset", SendMessageOptions.DontRequireReceiver);
            }
#endif
            if (PlayOnEnable && !isPlaying)
            {
                PlayForward();
            }
        }

        protected virtual void OnDisable()
        {
            if (PlayOnEnable && isPlaying)
            {
                Finish();
            }
        }

        protected virtual void Update()
        {
            if (isPlaying)
            {
                if (isFirst)
                {
                    if (Delay > 0f && delayed < Delay)
                    {
                        delayed += (IgnoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime);
                    }
                    else
                    {
                        isFirst = false;
                        FireOnStartLoop();
                    }
                    return;
                }

                if (Duration > 0f)
                {
                    UpdateProgress(GetSample(current, Duration));
                }

                switch (LoopType)
                {
                    case TweeningLoopType.Once:
                        if ((current == 0f && Direction == TweeningDirection.Reverse) || (current == Duration && Direction == TweeningDirection.Forward))
                        {
                            isPlaying = false;
                            if (FireOnEndLoop())
                            {
                                return;
                            }
                            Stop(true);
                        }
                        break;
                    case TweeningLoopType.Loop:
                        if (current >= Duration && Direction == TweeningDirection.Forward)
                        {
                            isPlaying = false;
                            if (FireOnEndLoop())
                            {
                                return;
                            }
                            isPlaying = true;
                            current = 0f;
                            direction = TweeningDirection.Forward;
                            FireOnStartLoop();
                        }
                        else if (current <= 0f && Direction == TweeningDirection.Reverse)
                        {
                            isPlaying = false;
                            if (FireOnEndLoop())
                            {
                                return;
                            }
                            isPlaying = true;
                            current = Duration;
                            direction = TweeningDirection.Reverse;
                            FireOnStartLoop();
                        }
                        break;
                    case TweeningLoopType.PingPong:
                        if (current >= Duration && Direction == TweeningDirection.Forward)
                        {
                            if (initialDirection == TweeningDirection.Reverse)
                            {
                                isPlaying = false;
                                if (FireOnEndLoop())
                                {
                                    return;
                                }
                                isPlaying = true;
                            }
                            current = Duration;
                            direction = TweeningDirection.Reverse;
                            FireOnStartLoop();
                        }
                        else if (current <= 0f && Direction == TweeningDirection.Reverse)
                        {
                            if (initialDirection == TweeningDirection.Forward)
                            {
                                isPlaying = false;
                                if (FireOnEndLoop())
                                {
                                    return;
                                }
                                isPlaying = true;
                            }
                            current = 0f;
                            direction = TweeningDirection.Forward;
                            FireOnStartLoop();
                        }
                        break;
                }

                current += (IgnoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime) * (int)Direction;
                if (current < 0f)
                {
                    current = 0f;
                }
                else if (current > Duration)
                {
                    current = Duration;
                }
            }
        }

        protected abstract void UpdateProgress(float progress);

        private float GetSample(float current, float duration)
        {
            switch (EaseType)
            {
                case TweeningEaseType.Linear:
                    return EasingUtility.Linear(current, 0f, 1f, duration);
                case TweeningEaseType.BackEaseIn:
                    return EasingUtility.BackEaseIn(current, 0f, 1f, duration);
                case TweeningEaseType.BackEaseOut:
                    return EasingUtility.BackEaseOut(current, 0f, 1f, duration);
                case TweeningEaseType.BackEaseInOut:
                    return EasingUtility.BackEaseInOut(current, 0f, 1f, duration);
                case TweeningEaseType.BounceEaseIn:
                    return EasingUtility.BounceEaseIn(current, 0f, 1f, duration);
                case TweeningEaseType.BounceEaseOut:
                    return EasingUtility.BounceEaseOut(current, 0f, 1f, duration);
                case TweeningEaseType.BounceEaseInOut:
                    return EasingUtility.BounceEaseInOut(current, 0f, 1f, duration);
                case TweeningEaseType.CircleEaseIn:
                    return EasingUtility.CircleEaseIn(current, 0f, 1f, duration);
                case TweeningEaseType.CircleEaseOut:
                    return EasingUtility.CircleEaseOut(current, 0f, 1f, duration);
                case TweeningEaseType.CircleEaseInOut:
                    return EasingUtility.CircleEaseInOut(current, 0f, 1f, duration);
                case TweeningEaseType.CubicEaseIn:
                    return EasingUtility.CubicEaseIn(current, 0f, 1f, duration);
                case TweeningEaseType.CubicEaseOut:
                    return EasingUtility.CubicEaseOut(current, 0f, 1f, duration);
                case TweeningEaseType.CubicEaseInOut:
                    return EasingUtility.CubicEaseInOut(current, 0f, 1f, duration);
                case TweeningEaseType.ElasticEaseIn:
                    return EasingUtility.ElasticEaseIn(current, 0f, 1f, duration);
                case TweeningEaseType.ElasticEaseOut:
                    return EasingUtility.ElasticEaseOut(current, 0f, 1f, duration);
                case TweeningEaseType.ElasticEaseInOut:
                    return EasingUtility.ElasticEaseInOut(current, 0f, 1f, duration);
                case TweeningEaseType.ExpoEaseIn:
                    return EasingUtility.ExpoEaseIn(current, 0f, 1f, duration);
                case TweeningEaseType.ExpoEaseOut:
                    return EasingUtility.ExpoEaseOut(current, 0f, 1f, duration);
                case TweeningEaseType.ExpoEaseInOut:
                    return EasingUtility.ExpoEaseInOut(current, 0f, 1f, duration);
                case TweeningEaseType.QuadEaseIn:
                    return EasingUtility.QuadEaseIn(current, 0f, 1f, duration);
                case TweeningEaseType.QuadEaseOut:
                    return EasingUtility.QuadEaseOut(current, 0f, 1f, duration);
                case TweeningEaseType.QuadEaseInOut:
                    return EasingUtility.QuadEaseInOut(current, 0f, 1f, duration);
                case TweeningEaseType.QuartEaseIn:
                    return EasingUtility.QuartEaseIn(current, 0f, 1f, duration);
                case TweeningEaseType.QuartEaseOut:
                    return EasingUtility.QuartEaseOut(current, 0f, 1f, duration);
                case TweeningEaseType.QuartEaseInOut:
                    return EasingUtility.QuartEaseInOut(current, 0f, 1f, duration);
                case TweeningEaseType.QuintEaseIn:
                    return EasingUtility.QuintEaseIn(current, 0f, 1f, duration);
                case TweeningEaseType.QuintEaseOut:
                    return EasingUtility.QuintEaseOut(current, 0f, 1f, duration);
                case TweeningEaseType.QuintEaseInOut:
                    return EasingUtility.QuintEaseInOut(current, 0f, 1f, duration);
                case TweeningEaseType.SineEaseIn:
                    return EasingUtility.SineEaseIn(current, 0f, 1f, duration);
                case TweeningEaseType.SineEaseOut:
                    return EasingUtility.SineEaseOut(current, 0f, 1f, duration);
                case TweeningEaseType.SineEaseInOut:
                    return EasingUtility.SineEaseInOut(current, 0f, 1f, duration);
            }
            return EasingUtility.AnimationCurve(current, CustomAnimationCurve, duration);
        }

        private void Play(bool forward)
        {
            isPlaying = false;
            if (forward)
            {
                if (isFirst)
                {
                    current = 0f;
                    if (InitOnPlay)
                    {
                        UpdateProgress(0f);
                    }
                }
                direction = TweeningDirection.Forward;
            }
            else
            {
                if (isFirst)
                {
                    current = Duration;
                    if (InitOnPlay)
                    {
                        UpdateProgress(1f);
                    }
                }
                direction = TweeningDirection.Reverse;
            }
            initialDirection = direction;
            delayed = 0f;
            loopCount = 0;
            isPlaying = true;
        }

        private void Stop(bool finish)
        {
            isPlaying = false;
            isFirst = true;
            if (finish)
            {
                if ((LoopType == TweeningLoopType.PingPong && initialDirection == TweeningDirection.Reverse) ||
                    (LoopType != TweeningLoopType.PingPong && initialDirection == TweeningDirection.Forward))
                {
                    UpdateProgress(1f);
                }
                else
                {
                    UpdateProgress(0f);
                }
                FireOnFinish();
            }
        }

        public void PlayForward()
        {
            Play(true);
        }

        public void PlayReverse()
        {
            Play(false);
        }

        public void Finish()
        {
            Stop(true);
        }

        public void Stop()
        {
            Stop(false);
        }

        private void FireOnFinish()
        {
            if (onFinish != null)
            {
                onFinish.Invoke();
            }
        }

        private void FireOnStartLoop()
        {
            if (OnStartLoop != null)
            {
                OnStartLoop.Invoke(this);
            }
        }

        private bool FireOnEndLoop()
        {
            if (StopAfterLoop > 0)
            {
                loopCount++;
            }
            if (OnEndLoop != null)
            {
                OnEndLoop.Invoke(this);
            }
            if (StopAfterLoop > 0 && loopCount >= StopAfterLoop)
            {
                Finish();
                return true;
            }
            return isPlaying;
        }
    }
}
