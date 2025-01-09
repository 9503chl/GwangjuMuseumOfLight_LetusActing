using System;
using UnityEngine;

namespace UnityEngine.UI
{
    public sealed class TweenAlpha : UITweener
    {
        [Range(0f, 1f)]
        public float FromValue = 1f;

        [Range(0f, 1f)]
        public float ToValue = 0f;

        [NonSerialized]
        private CanvasGroup _canvasGroup;
        private CanvasGroup canvasGroup
        {
            get
            {
                if (_canvasGroup == null)
                {
                    _canvasGroup = GetComponent<CanvasGroup>();
                }
                return _canvasGroup;
            }
        }

        [NonSerialized]
        private Graphic _graphic;
        private Graphic graphic
        {
            get
            {
                if (_graphic == null)
                {
                    _graphic = GetComponent<Graphic>();
                }
                return _graphic;
            }
        }

        [NonSerialized]
        private CanvasRenderer _canvasRenderer;
        private CanvasRenderer canvasRenderer
        {
            get
            {
                if (_canvasRenderer == null)
                {
                    _canvasRenderer = GetComponent<CanvasRenderer>();
                }
                return _canvasRenderer;
            }
        }

        [NonSerialized]
        private SpriteRenderer _spriteRenderer;
        private SpriteRenderer spriteRenderer
        {
            get
            {
                if (_spriteRenderer == null)
                {
                    _spriteRenderer = GetComponent<SpriteRenderer>();
                }
                return _spriteRenderer;
            }
        }

        [NonSerialized]
        private Renderer _renderer;
#pragma warning disable CS0108 
        private Renderer renderer
#pragma warning restore CS0108 
        {
            get
            {
                if (_renderer == null)
                {
                    _renderer = GetComponent<Renderer>();
                }
                return _renderer;
            }
        }

        private float GetValue()
        {
            if (canvasGroup != null)
            {
                return canvasGroup.alpha;
            }
            else if (graphic != null)
            {
                return graphic.color.a;
            }
            else if (canvasRenderer != null)
            {
                return canvasRenderer.GetAlpha();
            }
            else if (spriteRenderer != null)
            {
                return spriteRenderer.color.a; 
            }
            else if (renderer != null)
            {
                Material material;
                if (Application.isPlaying)
                {
                    material = renderer.material;
                }
                else
                {
                    material = renderer.sharedMaterial;
                }
                if (material != null)
                {
                    return material.color.a;
                }
            }
            return 0f;
        }

        private void SetValue(float value)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = value;
            }
            else if (graphic != null)
            {
                Color color = graphic.color;
                color.a = value;
                graphic.color = color;
            }
            else if (canvasRenderer != null)
            {
                canvasRenderer.SetAlpha(value);
            }
            else if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = value;
                spriteRenderer.color = color;
            }
            else if (renderer != null)
            {
                Material material;
                if (Application.isPlaying)
                {
                    material = renderer.material;
                }
                else
                {
                    material = renderer.sharedMaterial;
                }
                if (material != null)
                {
                    Color color = renderer.material.color;
                    color.a = value;
                    material.color = color;
                }
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
            float tempValue = FromValue;
            FromValue = ToValue;
            ToValue = tempValue;
        }

        private void Reset()
        {
            FromValue = GetValue();
            ToValue = GetValue();
        }
#endif

        public static TweenAlpha Create(GameObject target, float fromValue, float toValue, float duration, TweeningLoopType loopType = TweeningLoopType.Once, TweeningEaseType easeType = TweeningEaseType.Linear)
        {
            if (target != null)
            {
                TweenAlpha tweener = target.GetComponent<TweenAlpha>();
                if (tweener == null)
                {
                    tweener = target.AddComponent<TweenAlpha>();
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

        public static TweenAlpha Play(GameObject target, float fromValue, float toValue, float duration, TweeningLoopType loopType = TweeningLoopType.Once, TweeningEaseType easeType = TweeningEaseType.Linear)
        {
            TweenAlpha tweener = Create(target, fromValue, toValue, duration, loopType, easeType);
            if (tweener != null)
            {
                tweener.PlayForward();
            }
            return tweener;
        }
    }
}
