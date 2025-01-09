using System;
using UnityEngine;

namespace UnityEngine.UI
{
    public sealed class TweenPosition : UITweener
    {
        public Vector3 FromValue = Vector3.zero;

        public Vector3 ToValue = Vector3.zero;

        public bool FreezeX = false;
        public bool FreezeY = false;
        public bool FreezeZ = false;

        public bool UseRigidbody = false;

        [NonSerialized]
        private Rigidbody2D _rigidbody2D;
#pragma warning disable CS0108 
        private Rigidbody2D rigidbody2D
#pragma warning restore CS0108 
        {
            get
            {
                if (_rigidbody2D == null)
                {
                    _rigidbody2D = GetComponent<Rigidbody2D>();
                    if (_rigidbody2D == null)
                    {
                        _rigidbody2D = gameObject.AddComponent<Rigidbody2D>();
                        _rigidbody2D.isKinematic = true;
                    }
                    if (GetComponent<Collider2D>() == null)
                    {
                        gameObject.AddComponent<BoxCollider2D>();
                    }
                }
                return _rigidbody2D;
            }
        }

        [NonSerialized]
        private Rigidbody _rigidbody;
#pragma warning disable CS0108 
        private Rigidbody rigidbody
#pragma warning restore CS0108 
        {
            get
            {
                if (_rigidbody == null)
                {
                    _rigidbody = GetComponent<Rigidbody>();
                    if (_rigidbody == null)
                    {
                        _rigidbody = gameObject.AddComponent<Rigidbody>();
                        _rigidbody.isKinematic = true;
                    }
                    if (GetComponent<Collider>() == null)
                    {
                        gameObject.AddComponent<BoxCollider>();
                    }
                }
                return _rigidbody;
            }
        }

        private Vector3 GetValue()
        {
            if (UseRigidbody)
            {
                if (rectTransform != null)
                {
                    return rigidbody2D.position;
                }
                else
                {
                    return rigidbody.position;
                }
            }
            else
            {
                if (rectTransform != null)
                {
                    return rectTransform.anchoredPosition3D;
                }
                else
                {
                    return transform.localPosition;
                }
            }
        }

        private void SetValue(Vector3 value)
        {
            if (UseRigidbody)
            {
                if (rectTransform != null)
                {
                    if (FreezeX) value.x = rigidbody2D.position.x;
                    if (FreezeY) value.y = rigidbody2D.position.y;
                    rigidbody2D.position = value;
                }
                else
                {
                    if (FreezeX) value.x = rigidbody.position.x;
                    if (FreezeY) value.y = rigidbody.position.y;
                    if (FreezeZ) value.z = rigidbody.position.z;
                    rigidbody.position = value;
                }
            }
            else
            {
                if (rectTransform != null)
                {
                    if (FreezeX) value.x = rectTransform.anchoredPosition3D.x;
                    if (FreezeY) value.y = rectTransform.anchoredPosition3D.y;
                    if (FreezeZ) value.z = rectTransform.anchoredPosition3D.z;
                    rectTransform.anchoredPosition3D = value;
                }
                else
                {
                    if (FreezeX) value.x = transform.localPosition.x;
                    if (FreezeY) value.y = transform.localPosition.y;
                    if (FreezeZ) value.z = transform.localPosition.z;
                    transform.localPosition = value;
                }
            }
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

        public static TweenPosition Create(GameObject target, Vector3 fromValue, Vector3 toValue, float duration, TweeningLoopType loopType = TweeningLoopType.Once, TweeningEaseType easeType = TweeningEaseType.Linear)
        {
            if (target != null)
            {
                TweenPosition tweener = target.GetComponent<TweenPosition>();
                if (tweener == null)
                {
                    tweener = target.AddComponent<TweenPosition>();
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

        public static TweenPosition Play(GameObject target, Vector3 fromValue, Vector3 toValue, float duration, TweeningLoopType loopType = TweeningLoopType.Once, TweeningEaseType easeType = TweeningEaseType.Linear)
        {
            TweenPosition tweener = Create(target, fromValue, toValue, duration, loopType, easeType);
            if (tweener != null)
            {
                tweener.PlayForward();
            }
            return tweener;
        }
    }
}
