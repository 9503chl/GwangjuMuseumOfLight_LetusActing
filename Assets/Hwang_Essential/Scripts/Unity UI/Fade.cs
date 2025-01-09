using System;
using UnityEngine;
using UnityEngine.Events;

namespace UnityEngine.UI
{
    public class Fade : MonoBehaviour
    {
        [SerializeField]
        [Range(0f, 1f)]
        private float alpha = 0f;
        [SerializeField]
        [Range(0f, 1f)]
        private float targetAlpha = 1f;
        [SerializeField]
        [Range(0f, 60f)]
        private float duration = 1f;

        public event UnityAction<bool> OnFinish;

        private void OnDisable()
        {
            if (!gameObject.activeInHierarchy)
            {
                Destroy(this);
                if (OnFinish != null)
                {
                    OnFinish.Invoke(false);
                }
            }
        }

        private void Update()
        {
            if (duration > 0f && alpha != targetAlpha)
            {
                float value = Time.deltaTime / duration;
                if (alpha > targetAlpha)
                {
                    alpha -= value;
                    if (alpha < targetAlpha)
                    {
                        alpha = targetAlpha;
                    }
                }
                else
                {
                    alpha += value;
                    if (alpha > targetAlpha)
                    {
                        alpha = targetAlpha;
                    }
                }
                SetAlpha(gameObject, alpha);
            }
            else
            {
                SetAlpha(gameObject, targetAlpha);
                Destroy(this);
                if (OnFinish != null)
                {
                    OnFinish.Invoke(true);
                }
            }
        }

        public static float GetAlpha(GameObject obj)
        {
            Renderer render = obj.GetComponent<Renderer>();
            if (render != null)
            {
                return render.material.color.a;
            }
            else
            {
                CanvasRenderer cr = obj.GetComponent<CanvasRenderer>();
                if (cr != null)
                {
                    return cr.GetAlpha();
                }
                else
                {
                    CanvasGroup cg = obj.GetComponent<CanvasGroup>();
                    if (cg != null)
                    {
                        return cg.alpha;
                    }
                }
            }
            return 0f;
        }

        public static void SetAlpha(GameObject obj, float alpha)
        {
            Renderer render = obj.GetComponent<Renderer>();
            if (render != null)
            {
                Color c = render.material.color;
                c.a = alpha;
                render.material.color = c;
            }
            else
            {
                CanvasRenderer cr = obj.GetComponent<CanvasRenderer>();
                if (cr != null)
                {
                    cr.SetAlpha(alpha);
                }
                else
                {
                    CanvasGroup cg = obj.GetComponent<CanvasGroup>();
                    if (cg != null)
                    {
                        cg.alpha = alpha;
                    }
                }
            }
        }

        public static Fade To(GameObject obj, float duration = 1f, float targetAlpha = 1f, UnityAction<bool> onFinish = null)
        {
            Fade effect = obj.GetComponent<Fade>();
            if (effect == null)
            {
                effect = obj.AddComponent<Fade>();
            }
            effect.duration = duration;
            effect.alpha = GetAlpha(obj);
            effect.targetAlpha = targetAlpha;
            effect.OnFinish = onFinish;
            effect.enabled = true;
            return effect;
        }

        public static Fade In(GameObject obj, float duration = 1f, UnityAction<bool> onFinish = null)
        {
            SetAlpha(obj, 0f);
            return To(obj, duration, 1f, onFinish);
        }

        public static Fade Out(GameObject obj, float duration = 1f, UnityAction<bool> onFinish = null)
        {
            SetAlpha(obj, 1f);
            return To(obj, duration, 0f, onFinish);
        }

        public static bool IsFading(GameObject obj)
        {
            Fade effect = obj.GetComponent<Fade>();
            return (effect != null && effect.isActiveAndEnabled);
        }
    }
}
