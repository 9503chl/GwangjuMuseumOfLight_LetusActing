using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI
{
    [RequireComponent(typeof(Text))]
    public class SystemFontLoader : MonoBehaviour
    {
        [SerializeField]
        [TextArea(3, 3)]
        public string DesiredFontNames;

        [SerializeField]
        public int DesiredFontSize = 16;

        [NonSerialized]
        private Text textComponent;

        [NonSerialized]
        private Font originalFont;

        [NonSerialized]
        private static readonly List<string> installedFontNames = new List<string>();

        [NonSerialized]
        private static readonly Dictionary<string, Font> createdFonts = new Dictionary<string, Font>();

        [NonSerialized]
        private static readonly Dictionary<Font, int> referedCounts = new Dictionary<Font, int>();

        private void Awake()
        {
            if (textComponent == null)
            {
                textComponent = GetComponent<Text>();
            }
            Apply();
            originalFont = textComponent.font;
        }

        public bool ToggleDesiredFont(string fontName)
        {
            if (!string.IsNullOrEmpty(DesiredFontNames))
            {
                List<string> desiredFontNames = new List<string>();
                string[] fontNames = DesiredFontNames.Split(',', '\n');
                for (int i = 0; i < fontNames.Length; i++)
                {
                    fontNames[i] = fontNames[i].Trim();
                    if (fontNames[i].Length > 0)
                    {
                        desiredFontNames.Add(fontNames[i]);
                    }
                }
                if (desiredFontNames.Contains(fontName))
                {
                    desiredFontNames.Remove(fontName);
                    DesiredFontNames = string.Join(", ", desiredFontNames.ToArray());
                    return false;
                }
                else
                {
                    desiredFontNames.Add(fontName);
                    DesiredFontNames = string.Join(", ", desiredFontNames.ToArray());
                    return true;
                }
            }
            else
            {
                DesiredFontNames = fontName;
                return true;
            }
        }

        private void IncreaseRefCount(Font font)
        {
            if (font != null)
            {
                if (referedCounts.ContainsKey(font))
                {
                    referedCounts[font]++;
                }
                else
                {
                    referedCounts.Add(font, 1);
                }
            }
        }

        private void DecreaseRefCount(Font font)
        {
            if (font != null && referedCounts.ContainsKey(font))
            {
                referedCounts[font]--;
                if (referedCounts[font] == 0)
                {
                    referedCounts.Remove(font);
                    foreach (KeyValuePair<string, Font> pair in createdFonts)
                    {
                        if (pair.Value == font)
                        {
                            createdFonts.Remove(pair.Key);
                            break;
                        }
                    }
                    DestroyImmediate(font);
                }
            }
        }

        public void ApplyFont(string desiredFontNames, int desiredFontSize)
        {
            if (!string.IsNullOrEmpty(desiredFontNames))
            {
                List<string> availableFontNames = new List<string>(desiredFontNames.Split(',', '\n'));
                if (installedFontNames.Count == 0)
                {
                    installedFontNames.AddRange(Font.GetOSInstalledFontNames());
                }
                for (int i = availableFontNames.Count - 1; i >= 0; i--)
                {
                    availableFontNames[i] = availableFontNames[i].Trim();
                    if (!installedFontNames.Contains(availableFontNames[i]))
                    {
                        availableFontNames.RemoveAt(i);
                    }
                }
                if (availableFontNames.Count > 0)
                {
                    string fontKey = string.Format("{0} ({1})", string.Join("+", availableFontNames.ToArray()), desiredFontSize);
                    Font font;
                    if (!createdFonts.TryGetValue(fontKey, out font))
                    {
                        font = Font.CreateDynamicFontFromOSFont(availableFontNames.ToArray(), desiredFontSize);
                        font.name = string.Join("+", font.fontNames);
                        createdFonts.Add(fontKey, font);
                    }
                    if (textComponent == null)
                    {
                        textComponent = GetComponent<Text>();
                    }
                    Font oldFont = textComponent.font;
                    if (oldFont != null && !createdFonts.ContainsValue(oldFont))
                    {
                        originalFont = oldFont;
                    }
                    if (oldFont != font)
                    {
                        IncreaseRefCount(font);
                        textComponent.font = font;
                        if (oldFont != originalFont)
                        {
                            DecreaseRefCount(oldFont);
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("Desired fonts can't be found in current OS");
                }
            }
        }

        public void Apply()
        {
            ApplyFont(DesiredFontNames, DesiredFontSize);
        }

        public void Restore()
        {
            if (originalFont != null)
            {
                if (textComponent == null)
                {
                    textComponent = GetComponent<Text>();
                }
                Font oldFont = textComponent.font;
                if (oldFont != originalFont && createdFonts.ContainsValue(oldFont))
                {
                    textComponent.font = originalFont;
                    DecreaseRefCount(oldFont);
                }
            }
        }
    }
}
