using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    [DisallowMultipleComponent]
    public class View : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        public Selectable FocusControl;

        [Range(0f, 10f)]
        public float FadeDuration = 0.5f;

        public RectTransform DragSource;
        public RectTransform DragTarget;
        public bool Draggable = false;

        public event UnityAction OnBeforeShow;
        public event UnityAction OnAfterShow;
        public event UnityAction OnBeforeHide;
        public event UnityAction OnAfterHide;
        public event UnityAction OnBeforeDrag;
        public event UnityAction OnAfterDrag;

        [NonSerialized]
        private float sourceAlpha = 1f;

        [NonSerialized]
        private Vector2 savedOffsetMin;

        [NonSerialized]
        private Vector2 savedOffsetMax;

        [NonSerialized]
        private Vector2 dragPosition;

        [NonSerialized]
        private bool dragging = false;

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

        private void SetInteractable(bool interactable)
        {
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.interactable = interactable;
            }
        }

        public Selectable GetFocus()
        {
            if (EventSystem.current != null)
            {
                GameObject selected = EventSystem.current.currentSelectedGameObject;
                if (selected != null)
                {
                    return selected.GetComponent<Selectable>();
                }
            }
            return null;
        }

        private Selectable GetNextFocus(Selectable selectable, bool reverse)
        {
            Selectable[] selectables = GetComponentsInChildren<Selectable>();
            int selectedIndex = -1;
            if (selectable != null)
            {
                for (int i = 0; i < selectables.Length; i++)
                {
                    if (selectables[i] == selectable)
                    {
                        selectedIndex = i;
                        break;
                    }
                }
            }
            if (reverse)
            {
                if (selectedIndex > 0)
                {
                    return selectables[selectedIndex - 1];
                }
                else if (selectables.Length > 0)
                {
                    return selectables[selectables.Length - 1];
                }
            }
            else
            {
                if (selectedIndex < selectables.Length - 1)
                {
                    return selectables[selectedIndex + 1];
                }
                else if (selectables.Length > 0)
                {
                    return selectables[0];
                }
            }
            return null;
        }

        public void SetFocus(Selectable selectable)
        {
            if (selectable == null)
            {
                if (EventSystem.current != null)
                {
                    EventSystem.current.SetSelectedGameObject(null);
                }
            }
            else
            {
                InputField inputField = selectable.GetComponent<InputField>();
                if (inputField != null)
                {
                    inputField.ActivateInputField();
                }
                selectable.Select();
            }
        }

        public void SetNextFocus(bool reverse)
        {
            Selectable selectable = GetFocus();
            Selectable nextSelectable = GetNextFocus(selectable, reverse);
            if (nextSelectable != null)
            {
                while (!nextSelectable.isActiveAndEnabled || !nextSelectable.interactable)
                {
                    nextSelectable = GetNextFocus(nextSelectable, reverse);
                }
            }
            SetFocus(nextSelectable);
        }

        public void KillFocus()
        {
            SetFocus(null);
        }

        public void SetActive(bool value)
        {
            gameObject.SetActive(value);
        }

        public bool IsFading()
        {
            return Fade.IsFading(gameObject);
        }

        public void Show()
        {
            KillFocus();
            if (!Fade.IsFading(gameObject))
            {
                if (gameObject.activeSelf) return;

                sourceAlpha = Fade.GetAlpha(gameObject);
                Fade.SetAlpha(gameObject, 0f);
                SetInteractable(false);
                gameObject.SetActive(true);
            }
            if (OnBeforeShow != null)
            {
                OnBeforeShow.Invoke();
            }
            if (!gameObject.activeSelf)
            {
                Fade.SetAlpha(gameObject, sourceAlpha);
                return;
            }
            Fade.To(gameObject, FadeDuration, sourceAlpha, delegate (bool completed)
            {
                SetInteractable(true);
                if (completed)
                {
                    SetFocus(FocusControl);
                    if (OnAfterShow != null)
                    {
                        OnAfterShow.Invoke();
                    }
                }
                Fade.SetAlpha(gameObject, sourceAlpha);
            });
        }

        public void Hide()
        {
            if (!Fade.IsFading(gameObject))
            {
                if (!gameObject.activeSelf) return;

                sourceAlpha = Fade.GetAlpha(gameObject);
                SetInteractable(false);
            }
            if (OnBeforeHide != null)
            {
                OnBeforeHide.Invoke();
            }
            Fade.To(gameObject, FadeDuration, 0f, delegate (bool completed)
            {
                SetInteractable(true);
                if (completed)
                {
                    gameObject.SetActive(false);
                    if (OnAfterHide != null)
                    {
                        OnAfterHide.Invoke();
                    }
                }
                Fade.SetAlpha(gameObject, sourceAlpha);
            });
        }

        public void SavePosition()
        {
            if (DragTarget != null)
            {
                savedOffsetMin = DragTarget.offsetMin;
                savedOffsetMax = DragTarget.offsetMax;
            }
            else
            {
                savedOffsetMin = rectTransform.offsetMin;
                savedOffsetMax = rectTransform.offsetMax;
            }
        }

        public void RestorePosition()
        {
            if (DragTarget != null)
            {
                DragTarget.offsetMin = savedOffsetMin;
                DragTarget.offsetMax = savedOffsetMax;
            }
            else
            {
                rectTransform.offsetMin = savedOffsetMin;
                rectTransform.offsetMax = savedOffsetMax;
            }
        }

        private bool IsInsideOfScreen(Vector2 screenPoint)
        {
            Rect screenRect = new Rect(0f, 0f, Screen.width, Screen.height);
            return screenRect.Contains(screenPoint);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (isActiveAndEnabled && dragging && IsInsideOfScreen(eventData.position))
            {
                if (Draggable)
                {
                    Vector2 delta = eventData.position - dragPosition;
                    Canvas canvas = GetComponentInParent<Canvas>();
                    if (canvas != null)
                    {
                        delta /= canvas.scaleFactor;
                    }
                    dragPosition = eventData.position;
                    if (DragTarget != null)
                    {
                        DragTarget.anchoredPosition += delta;
                    }
                    else
                    {
                        rectTransform.anchoredPosition += delta;
                    }
                }
                else
                {
                    dragging = false;
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (isActiveAndEnabled && !dragging && eventData.button == PointerEventData.InputButton.Left)
            {
                if (Draggable)
                {
                    GameObject currentObject = eventData.pointerCurrentRaycast.gameObject;
                    if (DragSource != null && !currentObject.transform.IsChildOf(DragSource))
                    {
                        return;
                    }
                    dragPosition = eventData.position;
                    dragging = true;
                    if (OnBeforeDrag != null)
                    {
                        OnBeforeDrag.Invoke();
                    }
                }
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (isActiveAndEnabled && dragging && eventData.button == PointerEventData.InputButton.Left)
            {
                if (Draggable)
                {
                    dragging = false;
                    if (DragTarget != null)
                    {
                        DragTarget.anchoredPosition = new Vector2(Mathf.Ceil(DragTarget.anchoredPosition.x), Mathf.Floor(DragTarget.anchoredPosition.y));
                    }
                    else
                    {
                        rectTransform.anchoredPosition = new Vector2(Mathf.Ceil(rectTransform.anchoredPosition.x), Mathf.Floor(rectTransform.anchoredPosition.y));
                    }
                    if (OnAfterDrag != null)
                    {
                        OnAfterDrag.Invoke();
                    }
                }
            }
        }
    }
}
