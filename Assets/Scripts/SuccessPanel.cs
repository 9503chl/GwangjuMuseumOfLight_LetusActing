using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SuccessPanel : View
{
    private Text text;

    private void Awake()
    {
        text = GetComponentInChildren<Text>(true);
    }

    public void TextInit(string @str)
    {
        text.text = @str;
    }
}
