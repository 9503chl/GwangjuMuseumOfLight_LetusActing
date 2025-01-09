using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace UnityEngine.UI
{
    [DisallowMultipleComponent]
    public sealed class ButtonGroup : MonoBehaviour, IEnumerable
    {
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
                selectedButton = null;
                if (value >= 0 && value < buttons.Count)
                {
                    selectedButton = buttons[value];
                }
                if (selectedButton != deselectedButton && !hideSelection)
                {
                    SetButtonSelected(deselectedButton, false);
                    SetButtonSelected(selectedButton, true);
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
                if (selectedButton != deselectedButton && !hideSelection)
                {
                    SetButtonSelected(deselectedButton, false);
                    SetButtonSelected(selectedButton, true);
                }
            }
        }

#if UNITY_2019_1_OR_NEWER
        [Tooltip("Use pressed state instead of selected state for transition.")]
        [SerializeField]
        private bool usePressedState = true;
#endif

        [SerializeField]
        private bool hideSelection = false;

        public bool HideSelection
        {
            get
            {
                return hideSelection;
            }
            set
            {
                hideSelection = value;
                SetButtonSelected(selectedButton, !hideSelection);
            }
        }

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

        public event UnityAction<Button, bool> OnSelect;

        [NonSerialized]
        private readonly Dictionary<Button, Sprite> buttonSprites = new Dictionary<Button, Sprite>();

#if UNITY_EDITOR
        [NonSerialized]
        private bool showSelection = true;

        public void FixedUpdate()
        {
            if (Application.isPlaying)
            {
                if (SelectedIndex != selectedIndex)
                {
                    SelectedIndex = selectedIndex;
                }
                if (showSelection == hideSelection)
                {
                    showSelection = !hideSelection;
                    SetButtonSelected(selectedButton, !hideSelection);
                }
            }
        }
#endif

        private void Awake()
        {
            foreach (Button button in buttons)
            {
                if (button != null)
                {
                    if (!buttonSprites.ContainsKey(button) && button.image != null)
                    {
                        buttonSprites.Add(button, button.image.sprite);
                    }
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
                    if (!hideSelection)
                    {
                        SetButtonSelected(selectedButton, true);
                    }
                }
            }
        }

        private void SetButtonSelected(Button button, bool selected)
        {
            if (button != null)
            {
                switch (button.transition)
                {
                    case Selectable.Transition.ColorTint:
                        if (button.image != null)
                        {
#if UNITY_2019_1_OR_NEWER
                            button.image.color = selected ? (usePressedState ? button.colors.pressedColor : button.colors.selectedColor) : button.colors.normalColor;
#else
                            button.image.color = selected ? button.colors.pressedColor : button.colors.normalColor;
#endif
                        }
                        break;
                    case Selectable.Transition.SpriteSwap:
                        if (button.image != null)
                        {
                            if (!buttonSprites.ContainsKey(button))
                            {
                                buttonSprites.Add(button, button.image.sprite);
                            }
#if UNITY_2019_1_OR_NEWER
                            button.image.sprite = selected ? (usePressedState ? button.spriteState.pressedSprite : button.spriteState.selectedSprite) : buttonSprites[button];
#else
                            button.image.sprite = selected ? button.spriteState.pressedSprite : buttonSprites[button];
#endif
                        }
                        break;
                    case Selectable.Transition.Animation:
                        if (button.animator != null)
                        {
#if UNITY_2019_1_OR_NEWER
                            button.animator.SetTrigger(selected ? (usePressedState ? button.animationTriggers.pressedTrigger : button.animationTriggers.selectedTrigger) : button.animationTriggers.normalTrigger);
#else
                            button.animator.SetTrigger(selected ? button.animationTriggers.pressedTrigger : button.animationTriggers.normalTrigger);
#endif
                        }
                        break;
                }
                if (OnSelect != null)
                {
                    OnSelect.Invoke(button, selected);
                }
            }
        }

        private void ButtonClick(Button button)
        {
            if (isActiveAndEnabled)
            {
                SelectedButton = button;
                if (onClick != null)
                {
                    onClick.Invoke();
                }
            }
        }

        public void SetActive(bool value)
        {
            gameObject.SetActive(value);
        }

        public void SetInteractable(bool value)
        {
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
                if (!buttonSprites.ContainsKey(button) && button.image != null)
                {
                    buttonSprites.Add(button, button.image.sprite);
                }
                button.onClick.AddListener(delegate { ButtonClick(button); });
            }
        }

        public void Remove(Button button)
        {
            if (button != null)
            {
                if (button == selectedButton)
                {
                    if (!hideSelection)
                    {
                        SetButtonSelected(button, false);
                    }
                    selectedIndex = -1;
                    selectedButton = null;
                }
                buttons.Remove(button);
                if (buttonSprites.ContainsKey(button))
                {
                    buttonSprites.Remove(button);
                }
                button.onClick.RemoveAllListeners();
            }
        }

        public void Clear()
        {
            foreach (Button button in buttons)
            {
                if (button != null)
                {
                    if (buttonSprites.ContainsKey(button))
                    {
                        buttonSprites.Remove(button);
                    }
                    button.onClick.RemoveAllListeners();
                }
            }
            buttons.Clear();
            buttonSprites.Clear();
            selectedIndex = -1;
            selectedButton = null;
        }
    }
}
