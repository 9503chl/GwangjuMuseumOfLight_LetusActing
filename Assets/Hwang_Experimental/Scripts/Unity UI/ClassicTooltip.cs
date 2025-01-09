using System;
using System.Collections;
using UnityEngine;

namespace UnityEngine.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class ClassicTooltip : MonoBehaviour
    {
        public GameObject TooltipObject;
        public Text TitleText;
        public Text ContentText;

        public TextAnchor TooltipAnchor = TextAnchor.MiddleCenter;
        public Vector2 TooltipOffset = new Vector2(0f, -80f);
        public Vector2 ExpandSize = new Vector2(5f, 0f);
        public RectOffset ScreenPadding = new RectOffset();
        public float ShowDelay = 0.5f;
        public float HideDelay = 5.5f;

        public bool AutoPopup = true;
        public bool PassThrough = true;
        public string TooltipTitle;
        public string TooltipContent;

        [NonSerialized]
        private Selectable selectable;

        [NonSerialized]
        private bool isRollOver = false;

        [NonSerialized]
        private Coroutine showRoutine;

        [NonSerialized]
        private Coroutine hideRoutine;

        private void Start()
        {
            if (TooltipObject == null)
            {
                Debug.LogError("Please assign tooltip object!");
                enabled = false;
            }
            else if (TooltipObject == gameObject)
            {
                Debug.LogError("Do not attach this script to tooltip object!");
                enabled = false;
            }
            else
            {
                TooltipObject.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (AutoPopup)
            {
                selectable = GetComponent<Selectable>();
            }
            if (TooltipObject != null)
            {
                HideTooltip();
            }
        }

        private void OnDisable()
        {
            HideTooltip();
        }

        private void Update()
        {
            if (AutoPopup)
            {
                if (selectable != null && !selectable.interactable)
                {
                    return;
                }

                if (!Input.anyKey && EventSystemRaycaster.Instance.IsPointerOverGameObject(gameObject, PassThrough))
                {
                    if (!isRollOver)
                    {
                        isRollOver = true;
                        ShowTooltip(GetComponent<RectTransform>(), TooltipContent, TooltipTitle);
                    }
                }
                else if (isRollOver)
                {
                    HideTooltip();
                }
            }
        }

        public IEnumerator DelayedShow(RectTransform rectTransform)
        {
            yield return new WaitForSeconds(ShowDelay);
            if (MoveTooltip(rectTransform))
            {
                TooltipObject.transform.localScale = Vector3.zero;
                TooltipObject.SetActive(true);
                TooltipObject.transform.localScale = Vector3.one;
                hideRoutine = StartCoroutine(DelayedHide());
            }
            showRoutine = null;
        }

        public IEnumerator DelayedShow()
        {
            yield return new WaitForSeconds(ShowDelay);
            TooltipObject.SetActive(true);
            hideRoutine = StartCoroutine(DelayedHide());
            showRoutine = null;
        }

        public IEnumerator DelayedHide()
        {
            yield return new WaitForSeconds(HideDelay);
            TooltipObject.SetActive(false);
            hideRoutine = null;
        }

        private void SetTooltip(string content, string title = null)
        {
            RectTransform objectRectTransform = TooltipObject.GetComponent<RectTransform>();
            if (ContentText != null && TitleText != null)
            {
                RectTransform contentRectTransform = ContentText.GetComponent<RectTransform>();
                RectTransform titleRectTransform = TitleText.GetComponent<RectTransform>();
                ContentText.SetTextAndFitWidth(content, ContentText.fontSize);
                TitleText.SetTextAndFitWidth(title, TitleText.fontSize);
                objectRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Max(contentRectTransform.rect.width, titleRectTransform.rect.width) + ExpandSize.x);
                objectRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentRectTransform.rect.height + titleRectTransform.rect.height + contentRectTransform.rect.y - titleRectTransform.rect.y + ExpandSize.y);
            }
            else if (ContentText != null)
            {
                RectTransform contentRectTransform = ContentText.GetComponent<RectTransform>();
                ContentText.SetTextAndFitWidth(content, ContentText.fontSize);
                objectRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, contentRectTransform.rect.width + ExpandSize.x);
                objectRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentRectTransform.rect.height + ExpandSize.y);
            }
            else if (TitleText != null)
            {
                RectTransform titleRectTransform = TitleText.GetComponent<RectTransform>();
                TitleText.SetTextAndFitWidth(title, TitleText.fontSize);
                objectRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, titleRectTransform.rect.width + ExpandSize.x);
                objectRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, titleRectTransform.rect.height + ExpandSize.y);
            }
        }

        private float ScaleFactor
        {
            get
            {
                Canvas canvas = GetComponentInParent<Canvas>();
                if (canvas != null)
                {
                    return canvas.scaleFactor;
                }
                return 1f;
            }
        }

        private bool MoveTooltip(RectTransform rectTransform)
        {
            Rect screenRect = new Rect(0, 0, Screen.width / ScaleFactor, Screen.height / ScaleFactor);
            screenRect.position += new Vector2(ScreenPadding.left, ScreenPadding.bottom);
            screenRect.size -= new Vector2(ScreenPadding.left + ScreenPadding.right, ScreenPadding.top + ScreenPadding.bottom);
            Vector2 screenPoint = (Vector2)rectTransform.position / ScaleFactor - Vector2.Scale(rectTransform.rect.size, rectTransform.pivot - new Vector2(0.5f, 0.5f));
            Rect contentRect = new Rect(rectTransform.position.x / ScaleFactor, (rectTransform.position.y - rectTransform.rect.height) / ScaleFactor, rectTransform.rect.width / ScaleFactor, rectTransform.rect.height / ScaleFactor);
            if (screenRect.Overlaps(contentRect))
            {
                RectTransform tooltipRectTransform = TooltipObject.GetComponent<RectTransform>();
                Vector2 tooltipSize = tooltipRectTransform.rect.size;
                Vector2 contentSize = rectTransform.rect.size + ExpandSize;
                if (ContentText != null)
                {
                    contentSize += new Vector2(ContentText.fontSize, 0f);
                }
                Vector2 tooltipPosition = screenPoint;
                switch (TooltipAnchor)
                {
                    case TextAnchor.UpperLeft:
                    case TextAnchor.MiddleLeft:
                    case TextAnchor.LowerLeft:
                        tooltipPosition.x = screenPoint.x + TooltipOffset.x - contentSize.x / 2f;
                        break;
                    case TextAnchor.UpperCenter:
                    case TextAnchor.MiddleCenter:
                    case TextAnchor.LowerCenter:
                        tooltipPosition.x = screenPoint.x + TooltipOffset.x - tooltipSize.x / 2f;
                        break;
                    case TextAnchor.UpperRight:
                    case TextAnchor.MiddleRight:
                    case TextAnchor.LowerRight:
                        tooltipPosition.x = screenPoint.x + TooltipOffset.x + contentSize.x / 2f - tooltipSize.x;
                        break;
                }
                switch (TooltipAnchor)
                {
                    case TextAnchor.LowerLeft:
                    case TextAnchor.LowerCenter:
                    case TextAnchor.LowerRight:
                        tooltipPosition.y = screenPoint.y + TooltipOffset.y - contentSize.y / 2f;
                        break;
                    case TextAnchor.MiddleLeft:
                    case TextAnchor.MiddleCenter:
                    case TextAnchor.MiddleRight:
                        tooltipPosition.y = screenPoint.y + TooltipOffset.y - tooltipSize.y / 2f;
                        break;
                    case TextAnchor.UpperLeft:
                    case TextAnchor.UpperCenter:
                    case TextAnchor.UpperRight:
                        tooltipPosition.y = screenPoint.y + TooltipOffset.y + contentSize.y / 2f - tooltipSize.y;
                        break;
                }

                if (tooltipPosition.x < screenRect.x)
                {
                    tooltipPosition.x = screenRect.x;
                }
                else if (tooltipPosition.x > screenRect.x + screenRect.width - tooltipSize.x)
                {
                    tooltipPosition.x = screenRect.x + screenRect.width - tooltipSize.x;
                }
                if (tooltipPosition.y < screenRect.y)
                {
                    tooltipPosition.y = screenRect.y;
                }
                else if (tooltipPosition.y > screenRect.y + screenRect.height - tooltipSize.y)
                {
                    tooltipPosition.y = screenRect.y + screenRect.height - tooltipSize.y;
                }

                if (ContentText != null)
                {
                    Vector2 textSize = ContentText.rectTransform.rect.size;
                    Vector2 textOffset = new Vector2(tooltipPosition.x - (int)tooltipPosition.x, tooltipPosition.y - (int)tooltipPosition.y);
                    Vector2 correctTextSize = new Vector2((int)textSize.x - ((int)textSize.x) % 2 + textOffset.x * 2f, (int)textSize.y - ((int)textSize.y) % 2 + textOffset.y * 2f);
                    ContentText.rectTransform.anchoredPosition = new Vector2((textSize.x - correctTextSize.x) / 2f, ContentText.rectTransform.anchoredPosition.y);
                }
                if (TitleText != null)
                {
                    Vector2 textSize = TitleText.rectTransform.rect.size;
                    Vector2 textOffset = new Vector2(tooltipPosition.x - (int)tooltipPosition.x, tooltipPosition.y - (int)tooltipPosition.y);
                    Vector2 correctTextSize = new Vector2((int)textSize.x - ((int)textSize.x) % 2 + textOffset.x * 2f, (int)textSize.y - ((int)textSize.y) % 2 + textOffset.y * 2f);
                    TitleText.rectTransform.anchoredPosition = new Vector2((textSize.x - correctTextSize.x) / 2f, TitleText.rectTransform.anchoredPosition.y);
                }
                tooltipRectTransform.anchoredPosition = tooltipPosition;
                return true;
            }
            return false;
        }

        private bool MoveTooltip(Vector2 screenPoint)
        {
            Rect screenRect = new Rect(0, 0, Screen.width / ScaleFactor, Screen.height / ScaleFactor);
            screenRect.position += new Vector2(ScreenPadding.left, ScreenPadding.bottom);
            screenRect.size -= new Vector2(ScreenPadding.left + ScreenPadding.right, ScreenPadding.top + ScreenPadding.bottom);
            screenPoint = new Vector2(Mathf.Ceil(screenPoint.x / ScaleFactor), Mathf.Floor(screenPoint.y / ScaleFactor));
            if (screenRect.Contains(screenPoint))
            {
                RectTransform tooltipRectTransform = TooltipObject.GetComponent<RectTransform>();
                Vector2 tooltipSize = tooltipRectTransform.rect.size;
                Vector2 tooltipPosition = screenPoint;
                switch (TooltipAnchor)
                {
                    case TextAnchor.UpperLeft:
                    case TextAnchor.MiddleLeft:
                    case TextAnchor.LowerLeft:
                        tooltipPosition.x = screenPoint.x + TooltipOffset.x - tooltipSize.x;
                        break;
                    case TextAnchor.UpperCenter:
                    case TextAnchor.MiddleCenter:
                    case TextAnchor.LowerCenter:
                        tooltipPosition.x = screenPoint.x + TooltipOffset.x - tooltipSize.x / 2f;
                        break;
                    case TextAnchor.UpperRight:
                    case TextAnchor.MiddleRight:
                    case TextAnchor.LowerRight:
                        tooltipPosition.x = screenPoint.x + TooltipOffset.x;
                        break;
                }
                switch (TooltipAnchor)
                {
                    case TextAnchor.LowerLeft:
                    case TextAnchor.LowerCenter:
                    case TextAnchor.LowerRight:
                        tooltipPosition.y = screenPoint.y + TooltipOffset.y - tooltipSize.y;
                        break;
                    case TextAnchor.MiddleLeft:
                    case TextAnchor.MiddleCenter:
                    case TextAnchor.MiddleRight:
                        tooltipPosition.y = screenPoint.y + TooltipOffset.y - tooltipSize.y / 2f;
                        break;
                    case TextAnchor.UpperLeft:
                    case TextAnchor.UpperCenter:
                    case TextAnchor.UpperRight:
                        tooltipPosition.y = screenPoint.y + TooltipOffset.y;
                        break;
                }

                if (tooltipPosition.x < screenRect.x)
                {
                    switch (TooltipAnchor)
                    {
                        case TextAnchor.UpperLeft:
                        case TextAnchor.MiddleLeft:
                        case TextAnchor.LowerLeft:
                            tooltipPosition.x = screenPoint.x + Mathf.Abs(TooltipOffset.x);
                            break;
                        default:
                            tooltipPosition.x = screenRect.x;
                            break;
                    }
                }
                else if (tooltipPosition.x > screenRect.x + screenRect.width - tooltipSize.x)
                {
                    switch (TooltipAnchor)
                    {
                        case TextAnchor.UpperRight:
                        case TextAnchor.MiddleRight:
                        case TextAnchor.LowerRight:
                            tooltipPosition.x = screenRect.x + screenRect.width - Mathf.Abs(TooltipOffset.x) - tooltipSize.x;
                            break;
                        default:
                            tooltipPosition.x = screenRect.x + screenRect.width - tooltipSize.x;
                            break;
                    }
                }
                if (tooltipPosition.y < screenRect.y)
                {
                    switch (TooltipAnchor)
                    {
                        case TextAnchor.LowerLeft:
                        case TextAnchor.LowerCenter:
                        case TextAnchor.LowerRight:
                        case TextAnchor.MiddleCenter:
                            tooltipPosition.y = screenPoint.y + Mathf.Abs(TooltipOffset.y);
                            break;
                        default:
                            tooltipPosition.y = screenRect.y;
                            break;
                    }
                }
                else if (tooltipPosition.y > screenRect.y + screenRect.height - tooltipSize.y)
                {
                    switch (TooltipAnchor)
                    {
                        case TextAnchor.UpperLeft:
                        case TextAnchor.UpperCenter:
                        case TextAnchor.UpperRight:
                            tooltipPosition.y = screenPoint.y - Mathf.Abs(TooltipOffset.y) - tooltipSize.y;
                            break;
                        case TextAnchor.MiddleCenter:
                            tooltipPosition.y = screenPoint.y - Mathf.Abs(TooltipOffset.y) - tooltipSize.y / 2f;
                            break;
                        default:
                            tooltipPosition.y = screenRect.y + screenRect.height - tooltipSize.y;
                            break;
                    }
                }

                if (ContentText != null)
                {
                    ContentText.rectTransform.anchoredPosition = new Vector2(0f, ContentText.rectTransform.anchoredPosition.y);
                }
                if (TitleText != null)
                {
                    TitleText.rectTransform.anchoredPosition = new Vector2(0f, TitleText.rectTransform.anchoredPosition.y);
                }
                tooltipRectTransform.anchoredPosition = tooltipPosition;
                return true;
            }
            return false;
        }

        public void ShowTooltip(RectTransform rectTransform, string content, string title = null)
        {
            if (isActiveAndEnabled)
            {
                if (hideRoutine != null)
                {
                    StopCoroutine(hideRoutine);
                    hideRoutine = null;
                }
                SetTooltip(content, title);
                if (MoveTooltip(rectTransform))
                {
                    if (showRoutine == null)
                    {
                        showRoutine = StartCoroutine(DelayedShow(rectTransform));
                    }
                }
                else
                {
                    TooltipObject.SetActive(false);
                }
            }
        }

        public void ShowTooltip(RectTransform rectTransform)
        {
            if (isActiveAndEnabled)
            {
                if (hideRoutine != null)
                {
                    StopCoroutine(hideRoutine);
                    hideRoutine = null;
                }
                if (MoveTooltip(rectTransform))
                {
                    if (showRoutine == null)
                    {
                        showRoutine = StartCoroutine(DelayedShow(rectTransform));
                    }
                }
                else
                {
                    TooltipObject.SetActive(false);
                }
            }
        }

        public void ShowTooltip(Vector2 screenPoint, string content, string title = null)
        {
            if (isActiveAndEnabled)
            {
                if (hideRoutine != null)
                {
                    StopCoroutine(hideRoutine);
                    hideRoutine = null;
                }
                SetTooltip(content, title);
                if (MoveTooltip(screenPoint))
                {
                    if (showRoutine == null)
                    {
                        showRoutine = StartCoroutine(DelayedShow());
                    }
                }
                else
                {
                    TooltipObject.SetActive(false);
                }
            }
        }

        public void ShowTooltip(Vector2 screenPoint)
        {
            if (isActiveAndEnabled)
            {
                if (hideRoutine != null)
                {
                    StopCoroutine(hideRoutine);
                    hideRoutine = null;
                }
                if (MoveTooltip(screenPoint))
                {
                    if (showRoutine == null)
                    {
                        showRoutine = StartCoroutine(DelayedShow());
                    }
                }
                else
                {
                    TooltipObject.SetActive(false);
                }
            }
        }

        public void ShowTooltip(string content, string title = null)
        {
            if (isActiveAndEnabled)
            {
                if (hideRoutine != null)
                {
                    StopCoroutine(hideRoutine);
                    hideRoutine = null;
                }
                SetTooltip(content, title);
                if (showRoutine == null)
                {
                    showRoutine = StartCoroutine(DelayedShow());
                }
            }
        }

        public void ShowTooltip()
        {
            if (isActiveAndEnabled)
            {
                if (hideRoutine != null)
                {
                    StopCoroutine(hideRoutine);
                    hideRoutine = null;
                }
                if (showRoutine == null)
                {
                    showRoutine = StartCoroutine(DelayedShow());
                }
            }
        }

        public void HideTooltip()
        {
            if (isActiveAndEnabled)
            {
                if (showRoutine != null)
                {
                    StopCoroutine(showRoutine);
                    showRoutine = null;
                }
                if (hideRoutine != null)
                {
                    StopCoroutine(hideRoutine);
                    hideRoutine = null;
                }
                TooltipObject.SetActive(false);
            }
            isRollOver = false;
        }
    }
}
