using System;
using UnityEngine;

namespace UnityEngine.UI
{
    [Serializable]
    public class SimpleFontData
    {
        public Font font;
        public FontStyle fontStyle;
        public int fontSize;
        public Color color;

        public SimpleFontData()
        {
            DefaultFontData();
        }

        public SimpleFontData(Text textComponent)
        {
            if (textComponent != null)
            {
                CopyFromComponent(textComponent);
            }
            else
            {
                DefaultFontData();
            }
        }

        public void DefaultFontData()
        {
            font = FontData.defaultFontData.font;
            fontStyle = FontData.defaultFontData.fontStyle;
            fontSize = FontData.defaultFontData.fontSize;
            color = Color.gray;
        }

        public void CopyFromComponent(Text textComponent)
        {
            if (textComponent != null)
            {
                font = textComponent.font;
                fontStyle = textComponent.fontStyle;
                fontSize = textComponent.fontSize;
                color = textComponent.color;
            }
        }

        public void PasteIntoComponent(Text textComponent)
        {
            if (textComponent != null)
            {
                textComponent.font = font;
                textComponent.fontStyle = fontStyle;
                textComponent.fontSize = fontSize;
                textComponent.color = color;
            }
        }
    }
}
