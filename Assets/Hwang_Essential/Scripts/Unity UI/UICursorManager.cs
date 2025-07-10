using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    [DisallowMultipleComponent]
    public class UICursorManager : MonoBehaviour
    {
        public CustomCursor DefaultCursor;

        [Header("UI cursors")]
        public CustomCursor ButtonCursor;
        public CustomCursor InputFieldCursor;
        public CustomCursor DropdownCursor;
        public CustomCursor ToggleCursor;
        public CustomCursor HorizontalSliderCursor;
        public CustomCursor VerticalSliderCursor;

        [NonSerialized]
        private GameObject overObject;

        public GameObject GetPointerOverUIObject()
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = Input.mousePosition;
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            GameObject topMostObject = null;
            foreach (RaycastResult result in results)
            {
                if (result.isValid)
                {
                    topMostObject = result.gameObject;
                    break;
                }
            }
            return topMostObject;
        }

        private void OnDisable()
        {
            CustomCursor.Reset(this);
        }

        private void FixedUpdate()
        {
            if (!Input.GetMouseButton(0))
            {
                CustomCursor cursor = null;
                overObject = GetPointerOverUIObject();
                if (overObject != null)
                {
                    Canvas canvas = overObject.GetComponent<Canvas>();
                    UICursor uiCursor = overObject.GetComponent<UICursor>();
                    Selectable selectable = overObject.GetComponentInParent<Selectable>();
                    if (canvas == null && uiCursor == null && selectable != null && selectable.enabled && selectable.interactable)
                    {
                        if (selectable is Button)
                        {
                            cursor = ButtonCursor;
                        }
                        else if (selectable is InputField)
                        {
                            cursor = InputFieldCursor;
                        }
                        else if (selectable is Dropdown)
                        {
                            cursor = DropdownCursor;
                        }
                        else if (selectable is Toggle)
                        {
                            cursor = ToggleCursor;
                        }
                        else if (selectable is Slider)
                        {
                            Slider slider = selectable as Slider;
                            if (slider.direction == Slider.Direction.LeftToRight || slider.direction == Slider.Direction.RightToLeft)
                            {
                                cursor = HorizontalSliderCursor;
                            }
                            else
                            {
                                cursor = VerticalSliderCursor;
                            }
                        }
                    }
                }
                if (cursor != null)
                {
                    CustomCursor.Apply(this, cursor);
                }
                else
                {
                    CustomCursor.ApplyOrReset(this, DefaultCursor);
                }
            }
        }
    }
}
