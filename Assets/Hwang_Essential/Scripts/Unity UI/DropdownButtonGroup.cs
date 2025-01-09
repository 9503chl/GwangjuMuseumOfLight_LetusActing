using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public sealed class DropdownButtonGroup : MonoBehaviour
    {
        public enum ButtonOrder
        {
            None,
            MoveToFirst,
            MoveToLast
        }

        [SerializeField]
        private int selectedIndex = -1;

        public int SelectedIndex
        {
            get
            {
                for (int i = 0; i < buttons.Count; i++)
                {
                    if (buttons[i] == selectedButton)
                    {
                        return i;
                    }
                }
                return -1;
            }
            set
            {
                Button deselectedButton = selectedButton;
                selectedIndex = value;
                if (value >= 0 && value < buttons.Count)
                {
                    selectedButton = buttons[value];
                }
                else
                {
                    selectedButton = null;
                }
                if (selectedButton != deselectedButton)
                {
                    SetButtonSibling(deselectedButton, false);
                    SetButtonSibling(selectedButton, true);
                }
            }
        }

        [NonSerialized]
        private Button selectedButton;

        public Button SelectedButton
        {
            get
            {
                return selectedButton;
            }
            set
            {
                Button deselectedButton = selectedButton;
                if (value != null)
                {
                    selectedIndex = buttons.IndexOf(value);
                }
                else
                {
                    selectedIndex = -1;
                }
                selectedButton = value;
                if (selectedButton != deselectedButton)
                {
                    SetButtonSibling(deselectedButton, false);
                    SetButtonSibling(selectedButton, true);
                }
            }
        }

        public Image BackgroundImage;
        public Image CollapsedImage;
        public Image ExpandedImage;

        [NonSerialized]
        private Button dropdownButton;

        public ButtonOrder ChangeButtonOrder;

        [SerializeField]
        private List<Button> buttons = new List<Button>();

        public Button this[int index]
        {
            get
            {
                return buttons[index];
            }
        }

        public int Count
        {
            get
            {
                return buttons.Count;
            }
        }

        public IEnumerator GetEnumerator()
        {
            return buttons.GetEnumerator();
        }

        public UnityEvent onClick;
        public UnityEvent onCancel;

        [NonSerialized]
        private bool isDroppedDown = false;

        public bool IsDroppedDown
        {
            get
            {
                return isDroppedDown;
            }
            set
            {
                SetDropdownState(value);
            }
        }


#if UNITY_EDITOR
        [NonSerialized]
        private int buttonCount = 0;

        private void FixedUpdate()
        {
            if (SelectedIndex != selectedIndex)
            {
                SelectedIndex = selectedIndex;
                SetDropdownState(isDroppedDown);
            }
            else if (buttonCount != buttons.Count)
            {
                buttonCount = buttons.Count;
                SetDropdownState(IsDroppedDown);
            }
        }
#endif

        private void Awake()
        {
            dropdownButton = GetComponent<Button>();
            if (dropdownButton != null)
            {
                dropdownButton.onClick.AddListener(delegate { SetDropdownState(!isDroppedDown); });
            }
            foreach (Button button in buttons)
            {
                if (button != null)
                {
                    button.onClick.AddListener(delegate { ButtonClick(button); });
                }
            }
        }

        private void Start()
        {
            if (selectedButton == null)
            {
                if (selectedIndex >= 0 && selectedIndex < buttons.Count)
                {
                    selectedButton = buttons[selectedIndex];
                    SetButtonSibling(selectedButton, true);
                }
            }
            SetDropdownState(isDroppedDown);
        }

        private void OnDisable()
        {
            if (isDroppedDown)
            {
                SetDropdownState(false);
                if (onCancel != null)
                {
                    onCancel.Invoke();
                }
            }
        }

        private IEnumerator Checkup()
        {
            EventSystem.current.SetSelectedGameObject(dropdownButton.gameObject);
            yield return new WaitForEndOfFrame();

            while (isDroppedDown)
            {
                GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
                bool cancel = true;
                if (currentSelected == dropdownButton.gameObject)
                {
                    cancel = false;
                }
                else
                {
                    foreach (Button button in buttons)
                    {
                        if (button != null && currentSelected == button.gameObject)
                        {
                            cancel = false;
                            break;
                        }
                    }
                }
                if (cancel)
                {
                    SetDropdownState(false);
                    if (onCancel != null)
                    {
                        onCancel.Invoke();
                    }
                }
                yield return null;
            }
        }

        private void SetDropdownState(bool expanded)
        {
            if (expanded)
            {
                for (int i = 0; i < buttons.Count; i++)
                {
                    if (buttons[i] != null)
                    {
                        buttons[i].gameObject.SetActive(true);
                    }
                }
                if (BackgroundImage != null)
                {
                    BackgroundImage.gameObject.SetActive(true);
                }
                if (CollapsedImage != null)
                {
                    CollapsedImage.gameObject.SetActive(false);
                }
                if (ExpandedImage != null)
                {
                    ExpandedImage.gameObject.SetActive(true);
                }
                isDroppedDown = true;
                if (isActiveAndEnabled)
                {
                    StartCoroutine(Checkup());
                }
            }
            else
            {
                for (int i = 0; i < buttons.Count; i++)
                {
                    if (buttons[i] != null && buttons[i] != selectedButton)
                    {
                        buttons[i].gameObject.SetActive(false);
                    }
                }
                if (BackgroundImage != null)
                {
                    BackgroundImage.gameObject.SetActive(false);
                }
                if (CollapsedImage != null)
                {
                    CollapsedImage.gameObject.SetActive(true);
                }
                if (ExpandedImage != null)
                {
                    ExpandedImage.gameObject.SetActive(false);
                }
                isDroppedDown = false;
            }
        }

        private void SetButtonSibling(Button button, bool selected)
        {
            if (button != null && ChangeButtonOrder != ButtonOrder.None)
            {
                button.gameObject.SetActive(selected);
                if (selected)
                {
                    if (ChangeButtonOrder == ButtonOrder.MoveToFirst)
                    {
                        button.transform.SetAsFirstSibling();
                    }
                    else
                    {
                        button.transform.SetAsLastSibling();
                    }
                }
                else
                {
                    for (int i = 0; i < buttons.Count; i++)
                    {
                        if (buttons[i] == button)
                        {
                            buttons[i].transform.SetSiblingIndex(i);
                            break;
                        }
                    }
                }
            }
        }

        private void ButtonClick(Button button)
        {
            if (isActiveAndEnabled)
            {
                if (isDroppedDown)
                {
                    SelectedButton = button;
                    SetDropdownState(false);
                    if (onClick != null)
                    {
                        onClick.Invoke();
                    }
                }
                else
                {
                    SetDropdownState(true);
                }
            }
        }

        public void SetActive(bool value)
        {
            gameObject.SetActive(value);
        }


        public void SetInteractable(bool value)
        {
            dropdownButton.interactable = value;
            foreach (Button button in buttons)
            {
                if (button != null)
                {
                    button.interactable = value;
                }
            }
        }

        public void Add(Button button)
        {
            if (button != null)
            {
                buttons.Add(button);
                button.onClick.AddListener(delegate { ButtonClick(button); });
            }
        }

        public void Remove(Button button)
        {
            if (button != null)
            {
                if (button == selectedButton)
                {
                    selectedIndex = -1;
                    selectedButton = null;
                }
                buttons.Remove(button);
                button.onClick.RemoveAllListeners();
            }
        }

        public void Clear()
        {
            selectedIndex = -1;
            selectedButton = null;
            foreach (Button button in buttons)
            {
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                }
            }
            buttons.Clear();
        }
    }
}
