using System;
using UnityEngine;

namespace UnityEngine.UI
{
    [DisallowMultipleComponent]
    public class DragToMove : UIDraggable
    {
        public bool FitInPixels = true;
        public CustomCursor MoveCursor;

        [NonSerialized]
        private bool needClipping;

        [NonSerialized]
        private Vector2 undoPosition;

        private Vector2 GetFittedPixelOffset(Vector2 value)
        {
            return new Vector2(Mathf.Round(value.x), Mathf.Round(value.y)) - value;
        }

        private Vector2 GetClippedAreaOffset(Vector2 delta)
        {
            Vector2 offset = Vector2.zero;
            RectTransform rectTransform = DragTarget;
            RectTransform clippingRectTransform = ClippingArea;
            if (rectTransform != null && clippingRectTransform != null)
            {
                Rect targetRect = GetScreenRect(rectTransform);
                Rect clippingRect = GetScreenRect(clippingRectTransform);
                targetRect.position += delta;
                if (clippingRect.width < targetRect.width)
                {
                    offset.x = clippingRect.center.x - targetRect.center.x;
                }
                else if (clippingRect.xMin > targetRect.xMin)
                {
                    offset.x = clippingRect.xMin - targetRect.xMin;
                }
                else if (clippingRect.xMax < targetRect.xMax)
                {
                    offset.x = clippingRect.xMax - targetRect.xMax;
                }
                if (clippingRect.height < targetRect.height)
                {
                    offset.y = clippingRect.center.y - targetRect.center.y;
                }
                else if (clippingRect.yMin > targetRect.yMin)
                {
                    offset.y = clippingRect.yMin - targetRect.yMin;
                }
                else if (clippingRect.yMax < targetRect.yMax)
                {
                    offset.y = clippingRect.yMax - targetRect.yMax;
                }
            }
            return offset;
        }

        private void ApplyDragDelta(Vector2 delta, bool fitting, bool clipping)
        {
            RectTransform rectTransform = DragTarget;
            if (rectTransform != null)
            {
                Vector2 offset = delta;
                if (clipping)
                {
                    offset += GetClippedAreaOffset(offset);
                }
                if (fitting)
                {
                    offset += GetFittedPixelOffset(rectTransform.anchoredPosition + offset);
                }
                rectTransform.anchoredPosition += offset;
            }
        }

        public void MoveTarget(Vector2 position)
        {
            ApplyDragDelta(position - DragTarget.anchoredPosition, FitInPixels, ClippingMode != DragClippingMode.DoNotClipping);
        }

        public void MoveTargetBy(Vector2 delta)
        {
            ApplyDragDelta(delta, FitInPixels, ClippingMode != DragClippingMode.DoNotClipping);
        }

        protected override void OnEnter()
        {
            if (!Input.GetMouseButton(0))
            {
                CustomCursor.Apply(this, MoveCursor);
            }
        }

        protected override void OnExit()
        {
            if (!Input.GetMouseButton(0))
            {
                CustomCursor.Reset(this);
            }
        }

        protected override void OnBeginDrag(Vector2 position)
        {
            undoPosition = DragTarget.anchoredPosition;
        }

        protected override void OnEndDrag(Vector2 position)
        {
            ApplyDragDelta(Vector2.zero, FitInPixels, ClippingMode == DragClippingMode.OnEndDrag);
        }

        protected override void OnDrag(Vector2 delta)
        {
            ApplyDragDelta(delta, false, ClippingMode == DragClippingMode.WhileDragging);
        }

        protected override void OnCancel()
        {
            DragTarget.anchoredPosition = undoPosition;
        }

        private void OnEnable()
        {
            needClipping = true;
        }

        private void OnDisable()
        {
            CancelDrag();
        }

        private void Update()
        {
            if (needClipping)
            {
                needClipping = false;
                ApplyDragDelta(Vector2.zero, FitInPixels, ClippingMode != DragClippingMode.DoNotClipping);
            }
            if (entered && !dragging && !Input.GetMouseButton(0))
            {
                CustomCursor.Apply(this, MoveCursor);
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CancelDrag();
            }
        }
    }
}
