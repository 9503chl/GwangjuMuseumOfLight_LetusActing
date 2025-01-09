using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Draggable : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    private RectTransform maskRect;

    private RectTransform DragTarget;

    [NonSerialized]
    private RectTransform _rectTransform;

    public RectTransform rectTransform
    {
        get
        {
            if (_rectTransform == null)
            {
                _rectTransform = GetComponent<RectTransform>();
            }
            return _rectTransform;
        }
    }


    [NonSerialized]
    private Vector2 dragPosition;
    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.pointerId < -1 || eventData.pointerId > 0) return;
        if (isActiveAndEnabled)
        {

            Vector2 delta = eventData.position - dragPosition;
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                delta /= canvas.transform.localScale.x;
            }
            dragPosition = eventData.position;
            if (DragTarget != null)
            {
                float x = Mathf.Abs((maskRect.sizeDelta.x / 2 - DragTarget.sizeDelta.x / 2 * DragTarget.localScale.x));
                float y = Mathf.Abs((maskRect.sizeDelta.y / 2 - (DragTarget.sizeDelta.y) / 2 * DragTarget.localScale.x));

                DragTarget.anchoredPosition += delta  / DragTarget.localScale.x;
                DragTarget.anchoredPosition = new Vector2(Mathf.Clamp(DragTarget.anchoredPosition.x, -x, x), Mathf.Clamp(DragTarget.anchoredPosition.y, -y, y));
            }
            else
            {
                rectTransform.anchoredPosition += delta  / DragTarget.localScale.x;
            }
        }
    }
    //public void ResizeDragObject()
    //{
    //    Vector2 delta = (Vector2)DragTarget.localPosition - dragPosition;
    //    Canvas canvas = GetComponentInParent<Canvas>();
    //    if (canvas != null)
    //    {
    //        delta /= canvas.transform.localScale.x;
    //    }
    //    dragPosition = gameObject.transform.localPosition;
    //    if (DragTarget != null)
    //    {
    //        float x = Mathf.Abs((maskRect.sizeDelta.x / 2 - DragTarget.sizeDelta.x / 2 * DragTarget.localScale.x));
    //        float y = Mathf.Abs((maskRect.sizeDelta.y / 2 - DragTarget.sizeDelta.y / 2 * DragTarget.localScale.x));

    //        DragTarget.anchoredPosition += delta / 5;
    //        DragTarget.anchoredPosition = new Vector2(Mathf.Clamp(DragTarget.anchoredPosition.x, -x, x), Mathf.Clamp(DragTarget.anchoredPosition.y, -y, y));
    //    }
    //    else
    //    {
    //        rectTransform.anchoredPosition += delta / 5;
    //    }
    //}

    // Start is called before the first frame update
    void Awake()
    {
        maskRect = transform.parent.GetComponent<RectTransform>();

        DragTarget = transform.GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isActiveAndEnabled && eventData.button == PointerEventData.InputButton.Left)
        {
            dragPosition = eventData.position;
        }
    }
}
