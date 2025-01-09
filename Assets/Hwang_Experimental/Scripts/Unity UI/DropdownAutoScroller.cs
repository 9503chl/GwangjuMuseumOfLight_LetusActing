using System;
using UnityEngine;

namespace UnityEngine.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Dropdown))]
    public class DropdownAutoScroller : MonoBehaviour
    {
        [NonSerialized]
        private Dropdown dropdown;

        [NonSerialized]
        private ScrollRect scrollRect;

        [NonSerialized]
        private bool wasDroppedDown = false;

        private void Awake()
        {
            dropdown = GetComponent<Dropdown>();
        }

        private void Update()
        {
            if (dropdown == null) return;
            scrollRect = dropdown.GetComponentInChildren<ScrollRect>(false);
            if (wasDroppedDown && scrollRect == null)
            {
                wasDroppedDown = false;
            }
            else if (!wasDroppedDown && scrollRect != null)
            {
                wasDroppedDown = true;
                float viewportHeight;
                try
                {
                    if (scrollRect.viewport != null)
                    {
                        viewportHeight = scrollRect.viewport.rect.height;
                    }
                    else
                    {
                        viewportHeight = scrollRect.GetComponent<RectTransform>().rect.height;
                    }
                    float contentHeight = scrollRect.content.rect.height;
                    float scrollHeight = 0f;
                    if (contentHeight > viewportHeight)
                    {
                        int index = 0;
                        for (int i = 0; i < scrollRect.content.childCount; i++)
                        {
                            RectTransform child = scrollRect.content.GetChild(i).GetComponent<RectTransform>();
                            if (child != null && child.gameObject.activeInHierarchy)
                            {
                                scrollHeight += child.rect.height;
                                if (dropdown.value == index)
                                {
                                    if (scrollHeight > viewportHeight)
                                    {
                                        scrollRect.content.anchoredPosition = new Vector2(0f, scrollHeight - viewportHeight);
                                    }
                                    break;
                                }
                                index++;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
