using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    [DisallowMultipleComponent]
    public class TouchToClick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [Range(0f, 10f)]
        public float TouchInterval = 0.5f;
        public bool LButtonToTouch = true;
        public bool PressToRepeat = false;
        [Range(0f, 10f)]
        public float LongPressDelay = 0.5f;
        [Range(0f, 10f)]
        public float RepeatInterval = 0.2f;

        [NonSerialized]
        private Button button;

        [NonSerialized]
        private bool touched;

        [NonSerialized]
        private float lastTime;

        [NonSerialized]
        private Button.ButtonClickedEvent emptyEvent;

        [NonSerialized]
        private Button.ButtonClickedEvent clickedEvent;

        [NonSerialized]
        private Coroutine repeatRoutine;

        private void Awake()
        {
            emptyEvent = new Button.ButtonClickedEvent();
        }

        private void OnEnable()
        {
            button = transform.GetComponent<Button>();
        }

        private void OnDisable()
        {
            if (repeatRoutine != null)
            {
                StopCoroutine(repeatRoutine);
                repeatRoutine = null;
            }
            if (touched)
            {
                if (button != null && clickedEvent != null)
                {
                    button.onClick = clickedEvent;
                    clickedEvent = null;
                }
                touched = false;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (isActiveAndEnabled && eventData.pointerId >= (LButtonToTouch ? -1 : 0))
            {
                float currentTime = Time.realtimeSinceStartup;
                if (button != null && button.isActiveAndEnabled && button.interactable)
                {
                    touched = true;
                    if (clickedEvent == null && button.onClick != null)
                    {
                        clickedEvent = button.onClick;
                        button.onClick = emptyEvent;
                    }
                    if (currentTime - lastTime >= TouchInterval)
                    {
                        lastTime = currentTime;
                        if (clickedEvent != null)
                        {
                            clickedEvent.Invoke();
                            if (PressToRepeat)
                            {
                                repeatRoutine = StartCoroutine(Repeating());
                            }
                        }
                    }
                }
                else
                {
                    Selectable selectable = transform.GetComponent<Selectable>();
                    if (selectable != null)
                    {
                        selectable.Select();
                    }
                }
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (isActiveAndEnabled && eventData.pointerId >= (LButtonToTouch ? -1 : 0))
            {
                StartCoroutine(Untouching());
            }
        }

        private IEnumerator Untouching()
        {
            yield return null;
            if (repeatRoutine != null)
            {
                StopCoroutine(repeatRoutine);
                repeatRoutine = null;
            }
            if (touched)
            {
                if (button != null && clickedEvent != null)
                {
                    button.onClick = clickedEvent;
                    clickedEvent = null;
                }
                touched = false;
            }
        }

        private IEnumerator Repeating()
        {
            float currentTime = Time.realtimeSinceStartup;
            if (button != null && button.isActiveAndEnabled && button.interactable)
            {
                yield return new WaitForSeconds(LongPressDelay);
                while (touched)
                {
                    currentTime += Time.unscaledDeltaTime;
                    if (currentTime - lastTime >= RepeatInterval)
                    {
                        lastTime = currentTime;
                        if (clickedEvent != null)
                        {
                            clickedEvent.Invoke();
                        }
                    }
                    yield return null;
                }
            }
            repeatRoutine = null;
        }
    }
}
