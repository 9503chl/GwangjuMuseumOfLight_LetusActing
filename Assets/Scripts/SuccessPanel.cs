using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SuccessPanel : View
{
    private Text text;

    public void TextInit(string @str)
    {
        text.text = string.Format("<Color=orange>{0}</Color> ดิ", @str);
    }
}
