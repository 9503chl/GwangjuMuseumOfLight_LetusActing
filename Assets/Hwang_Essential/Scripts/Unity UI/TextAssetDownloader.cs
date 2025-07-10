using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace UnityEngine.UI
{
    public class TextAssetDownloader : AssetDownloader
    {
        protected override Type assetType
        {
            get { return typeof(TextAsset); }
        }

        protected override Type[] componentTypes
        {
            get { return new Type[] { typeof(Text), typeof(InputField), typeof(TextMesh) }; }
        }

#if UNITY_2018_1_OR_NEWER
        public TextAsset textAsset
        {
            get { return asset as TextAsset; }
        }
#else
        public BufferAsset textAsset
        {
            get { return asset as BufferAsset; }
        }
#endif

        public string text
        {
            get { return GetText(); }
        }

        [SerializeField]
        [Tooltip("Column name or index for csv file")]
        private string columnName;
        public string ColumnName
        {
            get { return columnName; }
            set { if (columnName != value) { columnName = value; } }
        }

        [SerializeField]
        [Tooltip("Key name for csv file, Line index for plain text file")]
        private string keyName;
        public string KeyName
        {
            get { return keyName; }
            set { if (keyName != value) { keyName = value; } }
        }

#if UNITY_EDITOR
        public override GUIContent PreviewContent
        {
            get
            {
                if (textAsset != null)
                {
                    if (!string.IsNullOrEmpty(text))
                    {
                        int count = 0;
                        StringWriter writer = new StringWriter();
                        StringReader reader = new StringReader(text);
                        while (reader.Peek() > 0)
                        {
                            if (count > 0)
                            {
                                writer.WriteLine();
                                if (count > 3)
                                {
                                    writer.Write("......");
                                    break;
                                }
                            }
                            writer.Write(reader.ReadLine());
                            count++;
                        }
                        string previewText = writer.ToString();
                        if (previewText.Length > 100)
                        {
                            previewText = string.Format("{0}\n......", previewText.Substring(0, 100));
                        }
                        return new GUIContent(previewText);
                    }
                    return GUIContent.none;
                }
                return null;
            }
        }
#endif

        private string[] SplitCsvLine(string line)
        {
            List<string> sections = new List<string>();
            StringBuilder sb = new StringBuilder();
            bool escaped = false;
            bool quoted = false;
            char prevChar = '\0';
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == '\\')
                {
                    if (prevChar == '\\' && i < line.Length - 1)
                    {
                        sb.Append('\\');
                        prevChar = '\0';
                    }
                    else
                    {
                        prevChar = line[i];
                    }
                    escaped = true;
                }
                else if (line[i] == '\"' && !escaped)
                {
                    if (prevChar == '\"' && i < line.Length - 1)
                    {
                        sb.Append('\"');
                        prevChar = '\0';
                    }
                    else
                    {
                        prevChar = line[i];
                    }
                    quoted = !quoted;
                }
                else if (line[i] == ',' && !quoted)
                {
                    sections.Add(sb.ToString().Trim());
#if NET_2_0 || NET_2_0_SUBSET
                    sb.Length = 0;
#else
                    sb.Clear();
#endif
                    prevChar = '\0';
                }
                else
                {
                    if (prevChar == '\\')
                    {
                        try
                        {
                            sb.Append(Regex.Unescape(string.Format("\\{0}", line[i])));
                        }
                        catch (Exception)
                        {
                        }
                        prevChar = '\0';
                    }
                    else
                    {
                        sb.Append(line[i]);
                        prevChar = line[i];
                    }
                    escaped = false;
                }
            }
            string lastLine = sb.ToString();
            if (lastLine != string.Empty)
            {
                sections.Add(sb.ToString().Trim());
            }
            return sections.ToArray();
        }

        private string GetCsvText(string[] lines)
        {
            if (lines != null && lines.Length > 0)
            {
                string[] columns = SplitCsvLine(lines[0]);
                int columnIndex = -1;
                for (int i = 0; i < columns.Length; i++)
                {
                    if (string.Compare(columns[i], columnName, true) == 0)
                    {
                        columnIndex = i;
                    }
                }
                if (columnIndex < 0)
                {
                    try
                    {
                        columnIndex = int.Parse(columnName);
                    }
                    catch (Exception)
                    {
                    }
                }
                if (columnIndex >= 0)
                {
                    for (int i = 1; i < lines.Length; i++)
                    {
                        string[] values = SplitCsvLine(lines[i]);
                        if (values.Length > 0 && string.Compare(values[0], keyName, true) == 0)
                        {
                            if (columnIndex < values.Length)
                            {
                                return values[columnIndex];
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }
            return string.Empty;
        }

        private string GetPlainText(string[] lines)
        {
            if (lines != null && lines.Length > 0)
            {
                try
                {
                    int lineIndex = int.Parse(keyName);
                    if (lineIndex >= 0 && lineIndex < lines.Length)
                    {
                        return lines[lineIndex];
                    }
                }
                catch (Exception)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (string line in lines)
                    {
                        sb.AppendLine(line);
                    }
                    return sb.ToString();
                }
            }
            return string.Empty;
        }

        public string GetText()
        {
            if (textAsset != null)
            {
                List<string> strings = new List<string>();
                StringReader reader = new StringReader(textAsset.text);
                while (reader.Peek() > 0)
                {
                    strings.Add(reader.ReadLine());
                }
                if (!string.IsNullOrEmpty(columnName))
                {
                    return GetCsvText(strings.ToArray());
                }
                else
                {
                    return GetPlainText(strings.ToArray());
                }
            }
            return null;
        }

        protected override void ApplyAsset()
        {
            if (text != null)
            {
                Text textComponent = GetComponent<Text>();
                if (textComponent != null)
                {
                    textComponent.text = text;
                }
                else
                {
                    InputField inputField = GetComponent<InputField>();
                    if (inputField != null)
                    {
                        inputField.text = text;
                    }
                    else
                    {
                        TextMesh textMesh = GetComponent<TextMesh>();
                        if (textMesh != null)
                        {
                            textMesh.text = text;
                        }
                    }
                }
            }
        }
    }
}
