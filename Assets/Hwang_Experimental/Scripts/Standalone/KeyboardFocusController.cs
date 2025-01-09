using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class KeyboardFocusController : MonoBehaviour
{
    [Tooltip("Press Enter key on InputField to submit. (Ctrl+Enter key for multi-line InputField)")]
    public bool submitByEnterKey = true;
    [Tooltip("Press Tab or Shift+Tab key to move focus. (Ctrl+Tab or Ctrl+Shift+Tab key for multi-line InputField)")]
    public bool moveFocusByTabKey = true;
    [Tooltip("Press F4, Alt+Down or Alt+Up key to show/hide Dropdown.")]
    public bool dropdownByF4Key = true;
    [Tooltip("Does not lose focus when press Esc key.")]
    public bool keepFocusOnEscKey = true;
    [Tooltip("Always shift focus when lose focus.")]
    public bool alwaysShiftFocus = true;

    public UnityEvent onSubmit = new UnityEvent();

    [NonSerialized]
    private InputField[] inputFields;

    [NonSerialized]
    private Selectable lastSelectable;

    [NonSerialized]
    private Dictionary<InputField, Vector2Int> inputFieldSelections = new Dictionary<InputField, Vector2Int>();

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

    public Selectable GetNextFocus(bool reverse)
    {
        Selectable selectable = GetNextFocus(GetFocus(), reverse);
        while (selectable != null)
        {
            if (selectable.isActiveAndEnabled && selectable.interactable)
            {
                //if (selectable.GetComponentInParent<InputField>() == null && selectable.GetComponentInParent<Dropdown>() == null && selectable.GetComponentInParent<ComboBox>() == null)
                if ((selectable.GetComponent<Toggle>() == null) ||
                    (selectable.GetComponent<Toggle>() != null && selectable.GetComponentInParent<Dropdown>() == null && selectable.GetComponentInParent<ComboBox>() == null))
                {
                    break;
                }
            }
            selectable = GetNextFocus(selectable, reverse);
        }
        return selectable;
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
        SetFocus(GetNextFocus(reverse));
    }

    public void KillFocus()
    {
        SetFocus(null);
    }

    private void SaveInputFieldSelection(InputField inputField)
    {
        if (inputField != null && inputField.isFocused)
        {
            if (inputFieldSelections.ContainsKey(inputField))
            {
                inputFieldSelections[inputField] = new Vector2Int(inputField.selectionAnchorPosition, inputField.selectionFocusPosition);
            }
            else
            {
                inputFieldSelections.Add(inputField, new Vector2Int(inputField.selectionAnchorPosition, inputField.selectionFocusPosition));
            }
        }
    }

    private IEnumerator AppendInputFieldText(InputField inputField, string text = null)
    {
        Vector2Int selection = Vector2Int.zero;
        if (inputFieldSelections.ContainsKey(inputField))
        {
            selection = inputFieldSelections[inputField];
        }
        inputField.ActivateInputField();
        yield return new WaitForEndOfFrame();
        inputField.MoveTextStart(false);
        if (!string.IsNullOrEmpty(text))
        {
            int caretPosition = Mathf.Min(selection.x, selection.y);
            int selectionLegnth = Mathf.Abs(selection.y - selection.x);
            inputField.text = inputField.text.Remove(caretPosition, selectionLegnth).Insert(caretPosition, text);
            inputField.selectionFocusPosition = inputField.selectionAnchorPosition = inputField.caretPosition = caretPosition + text.Length;
        }
        else
        {
            inputField.selectionAnchorPosition = inputField.caretPosition = selection.x;
            inputField.selectionFocusPosition = selection.y;
        }
        inputField.ForceLabelUpdate();
    }

    private IEnumerator DelayedSetNextFocus(bool reverse)
    {
        yield return new WaitForFixedUpdate();
        SetNextFocus(reverse);
    }

    private void Start()
    {
        inputFields = gameObject.GetComponentsInChildren<InputField>();
    }

    private void Update()
    {
        foreach (InputField inputField in inputFields)
        {
            SaveInputFieldSelection(inputField);
        }
        if (submitByEnterKey && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            Selectable selectable = GetFocus();
            bool isShift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            bool isControl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            if (selectable != null)
            {
                InputField inputField = selectable.GetComponent<InputField>();
                Dropdown dropdown = selectable.GetComponentInParent<Dropdown>();
                ComboBox comboBox = selectable.GetComponentInParent<ComboBox>();
                if (inputField == null || dropdown != null || comboBox != null)
                {
                    // ignore
                }
                else if (inputField.lineType == InputField.LineType.SingleLine)
                {
                    if (isShift && isControl)
                    {
                        StartCoroutine(AppendInputFieldText(inputField));
                    }
                    else
                    {
                        onSubmit.Invoke();
                    }
                }
                else if (inputField.lineType == InputField.LineType.MultiLineSubmit)
                {
                    if (isShift && isControl)
                    {
                        StartCoroutine(AppendInputFieldText(inputField));
                    }
                    else if (isShift && !isControl)
                    {
                        StartCoroutine(AppendInputFieldText(inputField, "\n"));
                    }
                    else if (!isShift)
                    {
                        onSubmit.Invoke();
                    }
                }
                else if (inputField.lineType == InputField.LineType.MultiLineNewline)
                {
                    if (!isShift && isControl)
                    {
                        if (inputFieldSelections.ContainsKey(inputField))
                        {
                            Vector2Int selections = inputFieldSelections[inputField];
                            if (selections.x == selections.y)
                            {
#if UNITY_2019_1_OR_NEWER
                                inputField.SetTextWithoutNotify(inputField.text.Remove(selections.x - 1, 1));
#else
                                InputField.OnChangeEvent changeEvent = inputField.onValueChanged;
                                inputField.onValueChanged = new InputField.OnChangeEvent();
                                inputField.text = inputField.text.Remove(selections.x - 1, 1);
                                inputField.onValueChanged = changeEvent;
#endif
                            }
                        }
                        inputField.MoveTextStart(false);
                        onSubmit.Invoke();
                    }
                }
            }
        }
        if (moveFocusByTabKey && Input.GetKeyDown(KeyCode.Tab))
        {
            Selectable selectable = GetFocus();
            bool isShift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            bool isControl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            if (selectable == null)
            {
                SetNextFocus(isShift);
            }
            else
            {
                InputField inputField = selectable.GetComponent<InputField>();
                if (inputField == null)
                {
                    Dropdown dropdown = selectable.GetComponentInParent<Dropdown>();
                    ComboBox comboBox = selectable.GetComponentInParent<ComboBox>();
                    if (dropdown != null)
                    {
                        dropdown.Hide();
                        StartCoroutine(DelayedSetNextFocus(isShift));
                    }
                    else if (comboBox != null)
                    {
                        comboBox.Hide();
                        StartCoroutine(DelayedSetNextFocus(isShift));
                    }
                    else
                    {
                        SetNextFocus(isShift);
                    }
                }
                else if (inputField.lineType == InputField.LineType.SingleLine)
                {
                    SetNextFocus(isShift);
                }
                else if (isControl)
                {
                    SetNextFocus(isShift);
                }
            }
        }
        if (dropdownByF4Key)
        {
            bool isAlt = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
            if ((Input.GetKeyDown(KeyCode.F4) && !isAlt) || (Input.GetKeyDown(KeyCode.UpArrow) && isAlt) || (Input.GetKeyDown(KeyCode.DownArrow) && isAlt))
            {
                // not implemented yet!
            }
        }
        if (keepFocusOnEscKey && Input.GetKeyDown(KeyCode.Escape))
        {
            SetFocus(GetFocus());
        }
        if (alwaysShiftFocus)
        {
            Selectable selectable = GetFocus();
            if (selectable == null)
            {
                if (lastSelectable != null && lastSelectable.isActiveAndEnabled && lastSelectable.interactable)
                {
                    SetFocus(lastSelectable);
                }
                else
                {
                    Selectable nextSelectable = GetNextFocus(lastSelectable, false);
                    if (nextSelectable != null)
                    {
                        lastSelectable = nextSelectable;
                    }
                }
            }
            else
            {
                lastSelectable = selectable;
                if (!selectable.isActiveAndEnabled || !selectable.interactable)
                {
                    KillFocus();
                }
            }
        }
    }
}
