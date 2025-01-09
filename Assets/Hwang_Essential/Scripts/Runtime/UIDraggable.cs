using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    [Serializable]
    public abstract class UIDraggable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IDragHandler, IDropHandler
    {
        public enum DragClippingMode
        {
            WhileDragging,
            OnEndDrag,
            DoNotClipping
        }

        [SerializeField]
        protected RectTransform dragTarget;
        public RectTransform DragTarget
        {
            get
            {
                if (dragTarget == null)
                {
                    return GetComponent<RectTransform>();
                }
                return dragTarget;
            }
            set
            {
                dragTarget = value;
            }
        }

        [SerializeField]
        protected DragClippingMode clippingMode = DragClippingMode.WhileDragging;
        public DragClippingMode ClippingMode
        {
            get { return clippingMode; }
            set { clippingMode = value; }
        }

        [SerializeField]
        protected RectTransform clippingArea;
        public RectTransform ClippingArea
        {
            get
            {
                if (clippingArea == null)
                {
                    if (dragTarget != null)
                    {
                        Canvas canvas = dragTarget.GetComponentInParent<Canvas>();
                        if (canvas != null)
                        {
                            return canvas.GetComponent<RectTransform>();
                        }
                    }
                    else
                    {
                        Canvas canvas = GetComponentInParent<Canvas>();
                        if (canvas != null)
                        {
                            return canvas.GetComponent<RectTransform>();
                        }
                    }
                }
                return clippingArea;
            }
            set
            {
                clippingArea = value;
            }
        }

        [SerializeField]
        protected bool bringToFront = true;
        public bool BringToFrontOnDrag
        {
            get { return bringToFront; }
            set { bringToFront = value; }
        }

        protected float ScaleFactor
        {
            get
            {
                if (dragTarget != null)
                {
                    Canvas canvas = dragTarget.GetComponentInParent<Canvas>();
                    if (canvas != null)
                    {
                        return canvas.scaleFactor;
                    }
                }
                else
                {
                    Canvas canvas = GetComponentInParent<Canvas>();
                    if (canvas != null)
                    {
                        return canvas.scaleFactor;
                    }
                }
                return 1f;
            }
        }

        public event UnityAction<UIDraggable> OnBeforeDrag;
        public event UnityAction<UIDraggable> OnAfterDrag;
        public event UnityAction<UIDraggable> OnCancelDrag;

        [NonSerialized]
        protected bool entered;

        [NonSerialized]
        protected bool dragging;

        [NonSerialized]
        protected int pointerId;

        [NonSerialized]
        protected Vector2 dragPosition;

        protected abstract void OnEnter();
        protected abstract void OnExit();
        protected abstract void OnBeginDrag(Vector2 position);
        protected abstract void OnEndDrag(Vector2 position);
        protected abstract void OnDrag(Vector2 delta);
        protected abstract void OnCancel();

        protected Rect GetScreenRect(RectTransform rectTransform)
        {
            if (rectTransform != null)
            {
                float scaleFactor = 1f;
                Canvas canvas = GetComponentInParent<Canvas>();
                if (canvas != null)
                {
                    scaleFactor = canvas.scaleFactor;
                }
                return new Rect(rectTransform.rect.position + (Vector2)rectTransform.position / scaleFactor, rectTransform.rect.size);
            }
            return Rect.zero;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (isActiveAndEnabled && !dragging)
            {
                entered = true;
                OnEnter();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (isActiveAndEnabled && !dragging)
            {
                entered = false;
                OnExit();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (isActiveAndEnabled && DragTarget != null)
            {
                GameObject currentObject = eventData.pointerCurrentRaycast.gameObject;
                if (currentObject != null && currentObject.transform.IsChildOf(transform))
                {
                    if (!dragging && eventData.pointerId >= -1)
                    {
                        dragging = true;
                        pointerId = eventData.pointerId;
                        dragPosition = eventData.position;
                        if (bringToFront)
                        {
                            DragTarget.SetAsLastSibling();
                        }
                        OnBeginDrag(eventData.position);
                        if (dragging)
                        {
                            if (OnBeforeDrag != null)
                            {
                                OnBeforeDrag.Invoke(this);
                            }
                        }
                    }
                    else if (dragging && eventData.pointerId == -2)
                    {
                        CancelDrag();
                    }
                }
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (isActiveAndEnabled && DragTarget != null && eventData.pointerId == pointerId)
            {
                if (dragging)
                {
                    dragging = false;
                    OnEndDrag(eventData.position);
                    if (OnAfterDrag != null)
                    {
                        OnAfterDrag.Invoke(this);
                    }
                    LayoutRebuilder.ForceRebuildLayoutImmediate(DragTarget);
                }

                GameObject currentObject = eventData.pointerCurrentRaycast.gameObject;
                if (currentObject != null && currentObject.transform.IsChildOf(transform))
                {
                    entered = true;
                    OnEnter();
                }
                else
                {
                    entered = false;
                    OnExit();
                }
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (isActiveAndEnabled && DragTarget != null && eventData.pointerId == pointerId)
            {
                if (dragging)
                {
                    Vector2 delta = eventData.position - dragPosition;
                    dragPosition = eventData.position;
                    OnDrag(delta / ScaleFactor);
                }
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (isActiveAndEnabled && DragTarget != null && eventData.pointerId == pointerId)
            {
                entered = true;
                OnEnter();
            }
        }

        public void CancelDrag()
        {
            if (dragging)
            {
                dragging = false;
                pointerId = 0;
                OnCancel();
                if (OnCancelDrag != null)
                {
                    OnCancelDrag.Invoke(this);
                }
                OnExit();
            }
        }
    }
}
