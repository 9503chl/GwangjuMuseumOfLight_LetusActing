using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    [DisallowMultipleComponent]
    public class EnhanceScrollItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        [NonSerialized]
        private EnhanceScrollView scrollView;

        [SerializeField]
        private Color selectedColor = Color.white;

        [SerializeField]
        private Color deselectedColor = Color.white;

        [NonSerialized]
        private bool isCentered = false;

        [NonSerialized]
        private int offsetIndex = 0;
        public int OffsetIndex
        {
            get { return offsetIndex; }
            set { offsetIndex = value; }
        }

        [NonSerialized]
        private int realIndex = 0;
        public int RealIndex
        {
            get { return realIndex; }
            set { realIndex = value; }
        }

        [NonSerialized]
        private float centerOffset = 0f;
        public float CenterOffset
        {
            get { return centerOffset; }
            set { centerOffset = value; }
        }

        public void SetScrollView(EnhanceScrollView view)
        {
            scrollView = view;
        }

        public void SelectScrollItem(bool centered)
        {
            Graphic graphic = GetComponent<Graphic>();
            if (graphic != null)
            {
                if (centered)
                {
                    graphic.color = selectedColor;
                }
                else
                {
                    graphic.color = deselectedColor;
                }
            }
            isCentered = centered;
        }

        public void UpdateScrollItem(Vector3 position, Vector3 scale, float itemDepth, int itemCount)
        {
            transform.localPosition = position;
            transform.SetSiblingIndex((int)(itemDepth * itemCount));
            transform.localScale = scale;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (scrollView != null)
            {
                scrollView.OnBeginDragScrollItem(this);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (scrollView != null)
            {
                scrollView.OnDragScrollItem(this, eventData.delta);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (scrollView != null)
            {
                scrollView.OnEndDragScrollItem(this);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (scrollView != null)
            {
                if (eventData.button == PointerEventData.InputButton.Left && !eventData.dragging)
                {
                    scrollView.OnPointerClickScrollItem(this);
                }
            }
        }

#if UNITY_EDITOR
        [NonSerialized]
        private Color oldSelectedColor = Color.white;

        [NonSerialized]
        private Color oldDeselectedColor = Color.white;

        private void OnValidate()
        {
            if (oldSelectedColor != selectedColor || oldDeselectedColor != deselectedColor)
            {
                oldSelectedColor = selectedColor;
                oldDeselectedColor = deselectedColor;
                SelectScrollItem(isCentered);
            }
        }
#endif
    }
}
