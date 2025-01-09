using System;
using UnityEngine;

namespace UnityEngine.UI
{
    public sealed class TweenColor : UITweener
    {
        public Color FromValue = Color.white;

        public Color ToValue = Color.black;

        public bool KeepAlpha = false;

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

        private Color GetValue()
        {
            if (graphic != null)
            {
                return graphic.color;
            }
            else if (canvasRenderer != null)
            {
                return canvasRenderer.GetColor();
            }
            else if (spriteRenderer != null)
            {
                return spriteRenderer.color;
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
                    return material.color;
                }
            }
            return Color.black;
        }

        private void SetValue(Color value)
        {
            if (graphic != null)
            {
                graphic.color = value;
            }
            else if (canvasRenderer != null)
            {
                canvasRenderer.SetColor(value);
            }
            else if (spriteRenderer != null)
            {
                spriteRenderer.color = value;
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
                    material.color = value;
                }
            }
        }

        protected override void UpdateProgress(float progress)
        {
            SetValue(Color.Lerp(FromValue, ToValue, progress));
        }

#if UNITY_EDITOR
        [ContextMenu("Swap 'From' And 'To'")]
        private void SwapValues()
        {
            Color temp = FromValue;
            FromValue = ToValue;
            ToValue = temp;
        }

        private void Reset()
        {
            FromValue = GetValue();
            ToValue = GetValue();
        }
#endif

        public static TweenColor Create(GameObject target, Color fromValue, Color toValue, float duration, TweeningLoopType loopType = TweeningLoopType.Once, TweeningEaseType easeType = TweeningEaseType.Linear)
        {
            if (target != null)
            {
                TweenColor tweener = target.GetComponent<TweenColor>();
                if (tweener == null)
                {
                    tweener = target.AddComponent<TweenColor>();
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

        public static TweenColor Play(GameObject target, Color fromValue, Color toValue, float duration, TweeningLoopType loopType = TweeningLoopType.Once, TweeningEaseType easeType = TweeningEaseType.Linear)
        {
            TweenColor tweener = Create(target, fromValue, toValue, duration, loopType, easeType);
            if (tweener != null)
            {
                tweener.PlayForward();
            }
            return tweener;
        }
    }
}
