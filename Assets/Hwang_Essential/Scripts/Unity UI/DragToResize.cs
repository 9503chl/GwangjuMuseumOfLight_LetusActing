using System;
using UnityEngine;

namespace UnityEngine.UI
{
    [DisallowMultipleComponent]
    public class DragToResize : UIDraggable
    {
        public enum DragBorderType
        {
            None = 0,
            Left = 1,
            Right = 2,
            Top = 4,
            Bottom = 8,
            TopLeft = Top | Left,
            TopRight = Top | Right,
            BottomLeft = Bottom | Left,
            BottomRight = Bottom | Right,
            InsideOfBorders = 16
        }

        public enum DragBorderMode
        {
            None = 0,
            Left = 1,
            Right = 2,
            Top = 4,
            Bottom = 8,
            TopLeft = 16,
            TopRight = 32,
            BottomLeft = 64,
            BottomRight = 128,
            Horizontal = Left | Right,
            Vertical = Top | Bottom,
            MainDiagonal = TopLeft | BottomRight,
            AntiDiagonal = TopRight | BottomLeft,
            FourSides = Horizontal | Vertical,
            FourEdges = MainDiagonal | AntiDiagonal,
            AllBorders = FourSides | FourEdges
        }

        public bool FitInPixels = true;
        public DragBorderMode BorderMode = DragBorderMode.AllBorders;
        [Range(0f, 10f)]
        public float BorderSize = 5f;
        public Vector2 MinimumSize = new Vector2(100f, 100f);
        public Vector2 MaximumSize = Vector2.zero;
        public Vector2 AspectRatio = Vector2.one;
        public bool KeepAspectRatio = false;
        //public bool ResizeByCenter = false;
        public CustomCursor HorizontalCursor;
        public CustomCursor VerticalCursor;
        public CustomCursor MainDiagonalCursor;
        public CustomCursor AntiDiagonalCursor;

        [NonSerialized]
        private bool needClipping;

        [NonSerialized]
        private Vector2 undoPosition;

        [NonSerialized]
        private Vector2 undoSize;

        [NonSerialized]
        private DragBorderType dragBorderType;

        private bool IsBorderDraggable(DragBorderType borderType)
        {
            switch (BorderMode)
            {
                case DragBorderMode.Left:
                    return borderType == DragBorderType.Left;
                case DragBorderMode.Right:
                    return borderType == DragBorderType.Right;
                case DragBorderMode.Top:
                    return borderType == DragBorderType.Top;
                case DragBorderMode.Bottom:
                    return borderType == DragBorderType.Bottom;
                case DragBorderMode.TopLeft:
                    return borderType == DragBorderType.TopLeft;
                case DragBorderMode.TopRight:
                    return borderType == DragBorderType.TopRight;
                case DragBorderMode.BottomLeft:
                    return borderType == DragBorderType.BottomLeft;
                case DragBorderMode.BottomRight:
                    return borderType == DragBorderType.BottomRight;
                case DragBorderMode.Horizontal:
                    return borderType == DragBorderType.Left || borderType == DragBorderType.Right;
                case DragBorderMode.Vertical:
                    return borderType == DragBorderType.Top || borderType == DragBorderType.Bottom;
                case DragBorderMode.MainDiagonal:
                    return borderType == DragBorderType.TopLeft || borderType == DragBorderType.BottomRight;
                case DragBorderMode.AntiDiagonal:
                    return borderType == DragBorderType.TopRight || borderType == DragBorderType.BottomLeft;
                case DragBorderMode.FourSides:
                    return borderType == DragBorderType.Left || borderType == DragBorderType.Right || borderType == DragBorderType.Top || borderType == DragBorderType.Bottom;
                case DragBorderMode.FourEdges:
                    return borderType == DragBorderType.TopLeft || borderType == DragBorderType.BottomRight || borderType == DragBorderType.TopRight || borderType == DragBorderType.BottomLeft;
                case DragBorderMode.AllBorders:
                    return borderType != DragBorderType.None;
            }
            return false;
        }

        private DragBorderType GetBorderType(Vector2 screenPoint)
        {
            DragBorderType borderType = DragBorderType.None;
            RectTransform rectTransform = DragTarget;
            if (rectTransform != null)
            {
                Rect targetRect = GetScreenRect(rectTransform);
                Vector2 borderSize = new Vector2(BorderSize, BorderSize);
                if (borderSize.x > targetRect.width)
                {
                    borderSize.x = targetRect.width;
                }
                if (borderSize.y > targetRect.height)
                {
                    borderSize.y = targetRect.height;
                }
                Vector2 cornerSize = new Vector2(BorderSize, BorderSize) * 2f;
                if (cornerSize.x > targetRect.width)
                {
                    cornerSize.x = targetRect.width;
                }
                if (cornerSize.y > targetRect.height)
                {
                    cornerSize.y = targetRect.height;
                }
                if (screenPoint.x <= targetRect.xMax && screenPoint.x > targetRect.xMax - borderSize.x)
                {
                    if (screenPoint.y >= targetRect.yMin && screenPoint.y < targetRect.yMin + cornerSize.y)
                    {
                        borderType = DragBorderType.BottomRight;
                    }
                    else if (screenPoint.y <= targetRect.yMax && screenPoint.y > targetRect.yMax - cornerSize.y)
                    {
                        borderType = DragBorderType.TopRight;
                    }
                    else
                    {
                        borderType |= DragBorderType.Right;
                    }
                }
                else if (screenPoint.x >= targetRect.xMin && screenPoint.x < targetRect.xMin + borderSize.x)
                {
                    if (screenPoint.y >= targetRect.yMin && screenPoint.y < targetRect.yMin + cornerSize.y)
                    {
                        borderType = DragBorderType.BottomLeft;
                    }
                    else if (screenPoint.y <= targetRect.yMax && screenPoint.y > targetRect.yMax - cornerSize.y)
                    {
                        borderType = DragBorderType.TopLeft;
                    }
                    else
                    {
                        borderType |= DragBorderType.Left;
                    }
                }
                if (screenPoint.y >= targetRect.yMin && screenPoint.y < targetRect.yMin + borderSize.y)
                {
                    if (screenPoint.x <= targetRect.xMax && screenPoint.x > targetRect.xMax - cornerSize.x)
                    {
                        borderType = DragBorderType.BottomRight;
                    }
                    else if (screenPoint.x >= targetRect.xMin && screenPoint.x < targetRect.xMin + cornerSize.x)
                    {
                        borderType = DragBorderType.BottomLeft;
                    }
                    else
                    {
                        borderType |= DragBorderType.Bottom;
                    }
                }
                else if (screenPoint.y <= targetRect.yMax && screenPoint.y > targetRect.yMax - borderSize.y)
                {
                    if (screenPoint.x <= targetRect.xMax && screenPoint.x > targetRect.xMax - cornerSize.x)
                    {
                        borderType = DragBorderType.TopRight;
                    }
                    else if (screenPoint.x >= targetRect.xMin && screenPoint.x < targetRect.xMin + cornerSize.x)
                    {
                        borderType = DragBorderType.TopLeft;
                    }
                    else
                    {
                        borderType |= DragBorderType.Top;
                    }
                }
                if (targetRect.Contains(screenPoint))
                {
                    return IsBorderDraggable(borderType) ? borderType : DragBorderType.InsideOfBorders;
                }
            }
            return DragBorderType.None;
        }

        private Rect GetDraggedRectOffset(DragBorderType borderType, Vector2 delta)
        {
            Rect offset = Rect.zero;
            RectTransform rectTransform = DragTarget;
            if (rectTransform != null)
            {
                Vector2 pivot = rectTransform.pivot;
                switch (borderType)
                {
                    case DragBorderType.None:
                        offset.position = Vector2.Scale(delta, new Vector2(0.5f - pivot.x, 0.5f - pivot.y));
                        offset.size = delta;
                        break;
                    case DragBorderType.Left:
                        offset.position = Vector2.Scale(delta, new Vector2(1f - pivot.x, 0f));
                        offset.size = Vector2.Scale(delta, new Vector2(-1f, 0f));
                        break;
                    case DragBorderType.Right:
                        offset.position = Vector2.Scale(delta, new Vector2(pivot.x, 0f));
                        offset.size = Vector2.Scale(delta, new Vector2(1f, 0f));
                        break;
                    case DragBorderType.Top:
                        offset.position = Vector2.Scale(delta, new Vector2(0f, pivot.y));
                        offset.size = Vector2.Scale(delta, new Vector2(0f, 1f));
                        break;
                    case DragBorderType.Bottom:
                        offset.position = Vector2.Scale(delta, new Vector2(0f, 1f - pivot.y));
                        offset.size = Vector2.Scale(delta, new Vector2(0f, -1f));
                        break;
                    case DragBorderType.TopLeft:
                        offset.position = Vector2.Scale(delta, new Vector2(1f - pivot.x, pivot.y));
                        offset.size = Vector2.Scale(delta, new Vector2(-1f, 1f));
                        break;
                    case DragBorderType.TopRight:
                        offset.position = Vector2.Scale(delta, pivot);
                        offset.size = delta;
                        break;
                    case DragBorderType.BottomLeft:
                        offset.position = Vector2.Scale(delta, new Vector2(1f - pivot.x, 1f - pivot.y));
                        offset.size = Vector2.Scale(delta, new Vector2(-1f, -1f));
                        break;
                    case DragBorderType.BottomRight:
                        offset.position = Vector2.Scale(delta, new Vector2(pivot.x, 1f - pivot.y));
                        offset.size = Vector2.Scale(delta, new Vector2(1f, -1f));
                        break;
                }
            }
            return offset;
        }

        private Rect GetLimitedSizeOffset(DragBorderType borderType, Vector2 sizeOffset)
        {
            Rect offset = Rect.zero;
            RectTransform rectTransform = DragTarget;
            if (rectTransform != null)
            {
                Rect targetRect = GetScreenRect(rectTransform);
                targetRect.size += sizeOffset;
                Vector2 pivot = rectTransform.pivot;
                Vector2 delta = Vector2.zero;
                if (MinimumSize.x > targetRect.width)
                {
                    delta.x = MinimumSize.x - targetRect.width;
                }
                else if (MaximumSize.x > MinimumSize.x && MaximumSize.x < targetRect.width)
                {
                    delta.x = MaximumSize.x - targetRect.width;
                }
                if (MinimumSize.y > targetRect.height)
                {
                    delta.y = MinimumSize.y - targetRect.height;
                }
                else if (MaximumSize.y > MinimumSize.y && MaximumSize.y < targetRect.height)
                {
                    delta.y = MaximumSize.y - targetRect.height;
                }
                switch (borderType)
                {
                    case DragBorderType.None:
                        offset.position = Vector2.Scale(delta, new Vector2(pivot.x - 0.5f, pivot.y - 0.5f));
                        offset.size = delta;
                        break;
                    case DragBorderType.Left:
                        offset.position = Vector2.Scale(delta, new Vector2(pivot.x - 1f, 0f));
                        offset.size = Vector2.Scale(delta, new Vector2(1f, 0f));
                        break;
                    case DragBorderType.Right:
                        offset.position = Vector2.Scale(delta, new Vector2(pivot.x, 0f));
                        offset.size = Vector2.Scale(delta, new Vector2(1f, 0f));
                        break;
                    case DragBorderType.Top:
                        offset.position = Vector2.Scale(delta, new Vector2(0f, pivot.y));
                        offset.size = Vector2.Scale(delta, new Vector2(0f, 1f));
                        break;
                    case DragBorderType.Bottom:
                        offset.position = Vector2.Scale(delta, new Vector2(0f, pivot.y - 1f));
                        offset.size = Vector2.Scale(delta, new Vector2(0f, 1f));
                        break;
                    case DragBorderType.TopLeft:
                        offset.position = Vector2.Scale(delta, new Vector2(pivot.x - 1f, pivot.y));
                        offset.size = delta;
                        break;
                    case DragBorderType.TopRight:
                        offset.position = Vector2.Scale(delta, pivot);
                        offset.size = delta;
                        break;
                    case DragBorderType.BottomLeft:
                        offset.position = Vector2.Scale(delta, new Vector2(pivot.x - 1f, pivot.y - 1f));
                        offset.size = delta;
                        break;
                    case DragBorderType.BottomRight:
                        offset.position = Vector2.Scale(delta, new Vector2(pivot.x, pivot.y - 1f));
                        offset.size = delta;
                        break;
                }
            }
            return offset;
        }

        private Rect GetAspectedRectOffset(DragBorderType borderType)
        {
            Rect offset = Rect.zero;
            RectTransform rectTransform = DragTarget;
            if (rectTransform != null)
            {
                Vector2 pivot = rectTransform.pivot;
                Vector2 size = rectTransform.sizeDelta;
                Vector2 delta = Vector2.zero;
                float aspect = 1f;
                if (AspectRatio.x > 0f && AspectRatio.y > 0f)
                {
                    aspect = AspectRatio.x / AspectRatio.y;
                }
                switch (borderType)
                {
                    case DragBorderType.None:
                        if (aspect < 1f)
                        {
                            delta.x = size.y * aspect - size.x;
                        }
                        else
                        {
                            delta.y = size.x / aspect - size.y;
                        }
                        break;
                    case DragBorderType.Left:
                    case DragBorderType.Right:
                        delta.y = size.x / aspect - size.y;
                        break;
                    case DragBorderType.Top:
                    case DragBorderType.Bottom:
                        delta.x = size.y * aspect - size.x;
                        break;
                    case DragBorderType.TopLeft:
                    case DragBorderType.TopRight:
                    case DragBorderType.BottomLeft:
                    case DragBorderType.BottomRight:
                        if (aspect < 1f)
                        {
                            delta.x = size.y * aspect - size.x;
                        }
                        else
                        {
                            delta.y = size.x / aspect - size.y;
                        }
                        break;
                }
                offset = GetDraggedRectOffset(DragBorderType.None, delta);
            }
            return offset;
        }

        private Vector2 GetFittedPixelOffset(Vector2 value)
        {
            return new Vector2(Mathf.Round(value.x), Mathf.Round(value.y)) - value;
        }

        private Rect GetClippedAreaOffset(DragBorderType borderType, Rect areaOffset)
        {
            Rect offset = Rect.zero;
            RectTransform rectTransform = DragTarget;
            RectTransform clippingRectTransform = ClippingArea;
            if (rectTransform != null && clippingRectTransform != null)
            {
                Rect targetRect = GetScreenRect(rectTransform);
                Rect clippingRect = GetScreenRect(clippingRectTransform);
                targetRect.position += areaOffset.position;
                targetRect.size += areaOffset.size;
                Vector2 pivot = rectTransform.pivot;
                Vector2 delta = Vector2.zero;
                if (clippingRect.xMin > targetRect.xMin)
                {
                    if (borderType == DragBorderType.None || (borderType & DragBorderType.Left) == DragBorderType.Left)
                    {
                        delta.x = clippingRect.xMin - targetRect.xMin;
                    }
                }
                if (clippingRect.xMax < targetRect.xMax)
                {
                    if (borderType == DragBorderType.None || (borderType & DragBorderType.Right) == DragBorderType.Right)
                    {
                        delta.x = clippingRect.xMax - targetRect.xMax;
                    }
                }
                if (clippingRect.yMin > targetRect.yMin)
                {
                    if (borderType == DragBorderType.None || (borderType & DragBorderType.Bottom) == DragBorderType.Bottom)
                    {
                        delta.y = clippingRect.yMin - targetRect.yMin;
                    }
                }
                if (clippingRect.yMax < targetRect.yMax)
                {
                    if (borderType == DragBorderType.None || (borderType & DragBorderType.Top) == DragBorderType.Top)
                    {
                        delta.y = clippingRect.yMax - targetRect.yMax;
                    }
                }
                switch (borderType)
                {
                    case DragBorderType.None:
                        offset.position = Vector2.Scale(delta, new Vector2(1f - pivot.x, 1f - pivot.y));
                        offset.size = delta;
                        break;
                    case DragBorderType.Left:
                        offset.position = Vector2.Scale(delta, new Vector2(1f - pivot.x, 0f));
                        offset.size = Vector2.Scale(delta, new Vector2(-1f, 0f));
                        break;
                    case DragBorderType.Right:
                        offset.position = Vector2.Scale(delta, new Vector2(pivot.x, 0f));
                        offset.size = Vector2.Scale(delta, new Vector2(1f, 0f));
                        break;
                    case DragBorderType.Top:
                        offset.position = Vector2.Scale(delta, new Vector2(0f, pivot.y));
                        offset.size = Vector2.Scale(delta, new Vector2(0f, 1f));
                        break;
                    case DragBorderType.Bottom:
                        offset.position = Vector2.Scale(delta, new Vector2(0f, 1f - pivot.y));
                        offset.size = Vector2.Scale(delta, new Vector2(0f, -1f));
                        break;
                    case DragBorderType.TopLeft:
                        offset.position = Vector2.Scale(delta, new Vector2(1f - pivot.x, pivot.y));
                        offset.size = Vector2.Scale(delta, new Vector2(-1f, 1f));
                        break;
                    case DragBorderType.TopRight:
                        offset.position = Vector2.Scale(delta, pivot);
                        offset.size = delta;
                        break;
                    case DragBorderType.BottomLeft:
                        offset.position = Vector2.Scale(delta, new Vector2(1f - pivot.x, 1f - pivot.y));
                        offset.size = Vector2.Scale(delta, new Vector2(-1f, -1f));
                        break;
                    case DragBorderType.BottomRight:
                        offset.position = Vector2.Scale(delta, new Vector2(pivot.x, 1f - pivot.y));
                        offset.size = Vector2.Scale(delta, new Vector2(1f, -1f));
                        break;
                }
                offset.size = new Vector2(-Mathf.Abs(delta.x), -Mathf.Abs(delta.y));
            }
            return offset;
        }

        private void ApplyCustomCursor(DragBorderType borderType)
        {
            if (dragBorderType != borderType)
            {
                switch (borderType)
                {
                    case DragBorderType.Left:
                    case DragBorderType.Right:
                        CustomCursor.Apply(this, HorizontalCursor);
                        break;
                    case DragBorderType.Top:
                    case DragBorderType.Bottom:
                        CustomCursor.Apply(this, VerticalCursor);
                        break;
                    case DragBorderType.TopLeft:
                    case DragBorderType.BottomRight:
                        CustomCursor.Apply(this, MainDiagonalCursor);
                        break;
                    case DragBorderType.TopRight:
                    case DragBorderType.BottomLeft:
                        CustomCursor.Apply(this, AntiDiagonalCursor);
                        break;
                    default:
                        CustomCursor.Reset(this);
                        break;
                }
                dragBorderType = borderType;
            }
        }

        private void ApplyDragDelta(DragBorderType borderType, Vector2 delta, bool fitting, bool clipping)
        {
            RectTransform rectTransform = DragTarget;
            if (rectTransform != null)
            {
                Rect offset = GetDraggedRectOffset(borderType, delta);
                Rect sizeOffset = GetLimitedSizeOffset(borderType, offset.size);
                offset.position += sizeOffset.position;
                offset.size += sizeOffset.size;
                if (clipping)
                {
                    Rect clipOffset = GetClippedAreaOffset(borderType, offset);
                    offset.position += clipOffset.position;
                    offset.size += clipOffset.size;
                }
                if (fitting)
                {
                    offset.position += GetFittedPixelOffset(rectTransform.anchoredPosition + offset.position);
                    offset.size += GetFittedPixelOffset(rectTransform.sizeDelta + offset.size);
                }
                rectTransform.anchoredPosition += offset.position;
                rectTransform.sizeDelta += offset.size;
            }
        }

        private void ApplyAspectRatio(DragBorderType borderType, bool fitting)
        {
            RectTransform rectTransform = DragTarget;
            if (rectTransform != null)
            {
                Rect offset = GetAspectedRectOffset(borderType);
                if (fitting)
                {
                    offset.position += GetFittedPixelOffset(rectTransform.anchoredPosition + offset.position);
                    offset.size += GetFittedPixelOffset(rectTransform.sizeDelta + offset.size);
                }
                rectTransform.anchoredPosition += offset.position;
                rectTransform.sizeDelta += offset.size;
            }
        }

        public void ResizeTarget(Vector2 size, DragBorderType borderType = DragBorderType.None)
        {
            ApplyDragDelta(borderType, size - DragTarget.sizeDelta, FitInPixels, ClippingMode != DragClippingMode.DoNotClipping);
        }

        public void ResizeTargetBy(Vector2 delta, DragBorderType borderType = DragBorderType.None)
        {
            ApplyDragDelta(borderType, delta, FitInPixels, ClippingMode != DragClippingMode.DoNotClipping);
        }

        protected override void OnEnter()
        {
            if (!Input.GetMouseButton(0))
            {
                ApplyCustomCursor(GetBorderType(Input.mousePosition / ScaleFactor));
            }
        }

        protected override void OnExit()
        {
            dragBorderType = DragBorderType.None;
            if (!Input.GetMouseButton(0))
            {
                CustomCursor.Reset(this);
            }
        }

        protected override void OnBeginDrag(Vector2 position)
        {
            dragBorderType = GetBorderType(position / ScaleFactor);
            undoPosition = DragTarget.anchoredPosition;
            undoSize = DragTarget.sizeDelta;
            if (dragBorderType == DragBorderType.None)
            {
                CancelDrag();
            }
        }

        protected override void OnEndDrag(Vector2 position)
        {
            ApplyDragDelta(dragBorderType, Vector2.zero, FitInPixels, ClippingMode == DragClippingMode.OnEndDrag);
            if (KeepAspectRatio)
            {
                ApplyAspectRatio(dragBorderType, FitInPixels);
            }
        }

        protected override void OnDrag(Vector2 delta)
        {
            ApplyDragDelta(dragBorderType, delta, false, ClippingMode == DragClippingMode.WhileDragging);
        }

        protected override void OnCancel()
        {
            DragTarget.anchoredPosition = undoPosition;
            DragTarget.sizeDelta = undoSize;
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
                ApplyDragDelta(DragBorderType.None, Vector2.zero, FitInPixels, ClippingMode != DragClippingMode.DoNotClipping);
            }
            if (entered && !dragging && !Input.GetMouseButton(0))
            {
                DragBorderType borderType = GetBorderType(Input.mousePosition / ScaleFactor);
                if (dragBorderType != borderType)
                {
                    ApplyCustomCursor(borderType);
                }
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CancelDrag();
            }
        }
    }
}
