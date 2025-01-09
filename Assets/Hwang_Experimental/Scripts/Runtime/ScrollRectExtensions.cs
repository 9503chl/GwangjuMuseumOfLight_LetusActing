using UnityEngine;

namespace UnityEngine.UI
{
    public static class ScrollRectExtensions
    {
        public static void RecalulateContentSize(this ScrollRect scrollRect)
        {
            scrollRect.StopMovement();
            ContentSizeFitter contentSizeFitter = scrollRect.content.GetComponentInParent<ContentSizeFitter>();
            if (contentSizeFitter != null)
            {
                ContentSizeFitter.FitMode horizontalFit = contentSizeFitter.horizontalFit;
                if (horizontalFit != ContentSizeFitter.FitMode.Unconstrained)
                {
                    contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                    contentSizeFitter.horizontalFit = horizontalFit;
                }
                ContentSizeFitter.FitMode verticalFit = contentSizeFitter.verticalFit;
                if (verticalFit != ContentSizeFitter.FitMode.Unconstrained)
                {
                    contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
                    contentSizeFitter.verticalFit = verticalFit;
                }
            }
            Canvas.ForceUpdateCanvases();
        }

        public static void EnsureVisible(this ScrollRect scrollRect, RectTransform childTransform, Vector2 pivot)
        {
            if (childTransform == null || !childTransform.IsChildOf(scrollRect.transform))
            {
                return;
            }
            RecalulateContentSize(scrollRect);
            Vector2 viewportSize = scrollRect.viewport.rect.size;
            Vector2 scrollRange = scrollRect.content.rect.size - viewportSize;
            Vector2 scrollPosition = scrollRect.content.anchoredPosition;
            Vector2 elementSize = childTransform.sizeDelta;
            Vector2 elementOffset = childTransform.anchoredPosition - Vector2.Scale(elementSize, pivot) + viewportSize + scrollPosition;
            if (scrollRect.horizontal && scrollRect.horizontalScrollbar != null && scrollRange.x > 0f)
            {
                if (elementOffset.x < elementSize.x)
                {
                    scrollPosition.x += elementSize.x - elementOffset.x;
                }
                else if (elementOffset.x > viewportSize.x)
                {
                    scrollPosition.x -= elementOffset.x - viewportSize.x;
                    if (scrollPosition.x < viewportSize.x / 3f)
                    {
                        scrollPosition.x = 0f;
                    }
                }
            }
            if (scrollRect.vertical && scrollRect.verticalScrollbar != null && scrollRange.y > 0f)
            {
                if (elementOffset.y < elementSize.y)
                {
                    scrollPosition.y += elementSize.y - elementOffset.y;
                }
                else if (elementOffset.y > viewportSize.y)
                {
                    scrollPosition.y -= elementOffset.y - viewportSize.y;
                    if (scrollPosition.y < viewportSize.y / 3f)
                    {
                        scrollPosition.y = 0f;
                    }
                }
            }
            scrollRect.content.anchoredPosition = scrollPosition;
        }

        public static void EnsureVisible(this ScrollRect scrollRect, RectTransform childTransform)
        {
            EnsureVisible(scrollRect, childTransform, Vector2.zero);
        }
    }
}
