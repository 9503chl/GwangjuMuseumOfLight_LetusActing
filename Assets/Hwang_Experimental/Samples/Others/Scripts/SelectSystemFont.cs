using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class SelectSystemFont : MonoBehaviour
{
    public Dropdown FontListDropdown;
    public Text[] TextComponents;
    public Button SaveListButton;
    public int FontSize = 16;

    private void Start()
    {
        FontListDropdown.options.Clear();
        FontListDropdown.options.Add(new Dropdown.OptionData("Select system font..."));
        string[] fontNames = Font.GetOSInstalledFontNames();
        foreach (string fontName in fontNames)
        {
            FontListDropdown.options.Add(new Dropdown.OptionData(fontName));
        }
        FontListDropdown.value = 0;
        FontListDropdown.onValueChanged.AddListener(FontListDropdown_ValueChanged);
        SaveListButton.onClick.AddListener(SaveListButton_Click);
    }

    private void FontListDropdown_ValueChanged(int value)
    {
        if (value > 0 && value < FontListDropdown.options.Count)
        {
            string fontName = FontListDropdown.options[value].text;
            foreach (Text textComponent in TextComponents)
            {
                if (textComponent != null)
                {
                    SystemFontLoader loader = textComponent.gameObject.EnsureComponent<SystemFontLoader>();
                    loader.ApplyFont(fontName, FontSize);
                }
            }
        }
    }

    private void SaveListButton_Click()
    {
        StringBuilder sb = new StringBuilder();
        string[] fontNames = Font.GetOSInstalledFontNames();
        foreach (string fontName in fontNames)
        {
            sb.AppendLine(fontName);
        }
        string path = string.Format("{0}/fontlist.txt", Application.persistentDataPath);
        File.WriteAllText(path, sb.ToString());
        SaveListButton.interactable = false;
        Text textComponent = SaveListButton.GetComponentInChildren<Text>();
        if (textComponent != null)
        {
            textComponent.text = "Saved to persistent data path";
        }
    }
}
