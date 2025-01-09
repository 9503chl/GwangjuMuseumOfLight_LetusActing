using System;
using System.Collections;
using UnityEngine;

public class DelayedDestroyObject : MonoBehaviour
{
    [Range(0f, 60f)]
    public float TimeToFadeIn = 0.25f;

    [Range(0f, 60f)]
    public float TimeToAlive = 1f;

    [Range(0f, 60f)]
    public float TimeToFadeOut = 0.75f;

    [NonSerialized]
    private CanvasGroup canvasGroup;

    [NonSerialized]
    private CanvasRenderer canvasRenderer;

    [NonSerialized]
    private SpriteRenderer spriteRenderer;

    [NonSerialized]
    private float originalAlpha;

    [NonSerialized]
    private Coroutine delayedRoutine;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasRenderer = GetComponent<CanvasRenderer>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalAlpha = GetAlpha();
    }

    private void Start()
    {
        StartCoroutine(FadeIn());
    }

    private void OnEnable()
    {
        delayedRoutine = StartCoroutine(DelayedDestroy());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        delayedRoutine = null;
        SetAlpha(originalAlpha);
    }

    private float GetAlpha()
    {
        if (canvasGroup != null)
        {
            return canvasGroup.alpha;
        }
        else if (canvasRenderer != null)
        {
            return canvasRenderer.GetAlpha();
        }
        else if (spriteRenderer != null)
        {
            return spriteRenderer.color.a;
        }
        return 0f;
    }

    private void SetAlpha(float a)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = a;
        }
        else if (canvasRenderer != null)
        {
            canvasRenderer.SetAlpha(a);
        }
        else if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = a;
            spriteRenderer.color = color;
        }
    }

    private IEnumerator FadeIn()
    {
        if (TimeToFadeIn > 0f)
        {
            SetAlpha(0f);
            float a = 0f;
            while (a < originalAlpha)
            {
                a += Time.deltaTime / TimeToFadeIn;
                if (a > originalAlpha) a = originalAlpha;
                SetAlpha(a);
                yield return null;
            }
        }
    }

    private IEnumerator FadeOut()
    {
        if (TimeToFadeOut > 0f)
        {
            float a = GetAlpha();
            while (a > 0f)
            {
                a -= Time.deltaTime / TimeToFadeOut;
                if (a < 0f) a = 0f;
                SetAlpha(a);
                yield return null;
            }
        }
    }

    private IEnumerator DelayedDestroy()
    {
        yield return new WaitForSeconds(TimeToAlive);
        yield return StartCoroutine(FadeOut());
        Destroy(gameObject);
        delayedRoutine = null;
    }

    public void Alive()
    {
        if (delayedRoutine != null)
        {
            StopCoroutine(delayedRoutine);
            delayedRoutine = null;
        }
        SetAlpha(originalAlpha);
        delayedRoutine = StartCoroutine(DelayedDestroy());
    }
}
