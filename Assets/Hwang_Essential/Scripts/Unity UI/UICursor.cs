using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UIBehaviour))]
    public class UICursor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        public CustomCursor HighlightedCursor;
        public CustomCursor PressedCursor;
        public CustomCursor DisabledCursor;

        [NonSerialized]
        private Selectable selectable;

        [NonSerialized]
        private bool entered;

        [NonSerialized]
        private bool pressed;

        [NonSerialized]
        private bool dragging;

        public bool HasSelectable
        {
            get { return GetComponent<Selectable>() != null; }
        }

        private void OnEnable()
        {
            selectable = GetComponent<Selectable>();
        }

        private void OnDisable()
        {
            entered = false;
            pressed = false;
            dragging = false;
            ResetCursor();
        }

        private void ApplyCursor()
        {
            if (selectable == null || selectable.enabled)
            {
                if (selectable != null && !selectable.interactable)
                {
                    CustomCursor.Apply(this, DisabledCursor);
                }
                else if (dragging)
                {
                    CustomCursor.Apply(this, PressedCursor);
                }
                else
                {
                    CustomCursor.Apply(this, HighlightedCursor);
                }
            }
            else
            {
                CustomCursor.Reset(this);
            }
        }

        private void ResetCursor()
        {
            CustomCursor.Reset(this);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            UICursor uiCursor = transform.parent.GetComponentInParent<UICursor>();
            entered = true;
            if (uiCursor != null)
            {
                uiCursor.entered = false;
            }
            if (!Input.GetMouseButton(0))
            {
                ApplyCursor();
            }
            else
            {
                pressed = true;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            UICursor uiCursor = transform.parent.GetComponentInParent<UICursor>();
            entered = false;
            if (uiCursor != null)
            {
                uiCursor.entered = true;
            }
            if (!Input.GetMouseButton(0))
            {
                ResetCursor();
                if (uiCursor != null)
                {
                    uiCursor.ApplyCursor();
                }
            }
            else
            {
                if (uiCursor != null)
                {
                    uiCursor.pressed = true;
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                dragging = true;
                ApplyCursor();
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                dragging = false;
                if (entered)
                {
                    pressed = true;
                }
                else
                {
                    ResetCursor();
                }
            }
        }

        private void Update()
        {
            if (entered && pressed && !dragging && !Input.GetMouseButton(0))
            {
                pressed = false;
                ApplyCursor();
            }
        }
    }
}
