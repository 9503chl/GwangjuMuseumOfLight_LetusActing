using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using UnityEngine;

namespace UnityEngine.UI
{
    [DisallowMultipleComponent]
    public class TextAssetLoader : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Language name represents resource folder name.")]
        private SystemLanguage language = SystemLanguage.Unknown;
        public SystemLanguage Language
        {
            get
            {
                return language;
            }
            set
            {
                if (language != value)
                {
                    language = value;
                    LoadAndApply();
                }
            }
        }

        [NonSerialized]
        private SystemLanguage defaultLanguage = SystemLanguage.Unknown;

        [SerializeField]
        [Tooltip("Resource name without file extenstion.")]
        private string fileName;
        public string FileName
        {
            get
            {
                return fileName;
            }
            set
            {
                if (string.Compare(fileName, value, true) != 0)
                {
                    fileName = value;
                    LoadAndApply();
                }
            }
        }

        [SerializeField]
        [Tooltip("Column name or index for csv file, not for plain text file.")]
        private string columnName;
        public string ColumnName
        {
            get
            {
                return columnName;
            }
            set
            {
                if (string.Compare(columnName, value, true) != 0)
                {
                    columnName = value;
                    if (applied && enabled)
                    {
                        Apply();
                    }
                    else
                    {
                        LoadAndApply();
                    }
                }
            }
        }

        [SerializeField]
        [Tooltip("Key name for csv file, line number for plain text file.")]
        private string keyName;
        public string KeyName
        {
            get
            {
                return keyName;
            }
            set
            {
                if (string.Compare(keyName, value, true) != 0)
                {
                    keyName = value;
                    if (applied && enabled)
                    {
                        Apply();
                    }
                    else
                    {
                        LoadAndApply();
                    }
                }
            }
        }

        private const char CsvSeperator = ',';

        [NonSerialized]
        private string[] lines;

        [NonSerialized]
        private bool applied;

        [NonSerialized]
        private TextAsset textAsset;

#if UNITY_EDITOR
        public GUIContent Preview
        {
            get
            {
                if (textAsset != null)
                {
                    string text = GetText();
                    if (text != null)
                    {
                        int count = 0;
                        StringWriter writer = new StringWriter();
                        StringReader reader = new StringReader(text);
                        while (reader.Peek() > 0)
                        {
                            if (count > 0)
                            {
                                writer.WriteLine();
                            }
                            if (count > 9)
                            {
                                writer.Write("... more ...");
                                break;
                            }
                            writer.Write(reader.ReadLine());
                            count++;
                        }
                        return new GUIContent(writer.ToString());
                    }
                    else
                    {
                        return GUIContent.none;
                    }
                }
                return null;
            }
        }
#endif

        private void Awake()
        {
            if (defaultLanguage == SystemLanguage.Unknown)
            {
                defaultLanguage = Application.systemLanguage;
            }
        }

        private void OnEnable()
        {
            if (!applied)
            {
                LoadAndApply();
            }
        }

        private IEnumerator Loading()
        {
            ResourceRequest request;
            if (language == SystemLanguage.Unknown)
            {
                request = Resources.LoadAsync<TextAsset>(fileName);
            }
            else
            {
                request = Resources.LoadAsync<TextAsset>(string.Format("{0}/{1}", language, fileName));
            }
            yield return request;
            if (request.asset == null && defaultLanguage != language)
            {
                request = Resources.LoadAsync<TextAsset>(string.Format("{0}/{1}", defaultLanguage, fileName));
                yield return request;
            }
            textAsset = request.asset as TextAsset;
            if (textAsset != null)
            {
                List<string> strings = new List<string>();
                StringReader reader = new StringReader(textAsset.text);
                while (reader.Peek() > 0)
                {
                    strings.Add(reader.ReadLine());
                }
                lines = strings.ToArray();
                Apply();
            }
        }

        public bool Load()
        {
            Unload();
            if (language == SystemLanguage.Unknown)
            {
                textAsset = Resources.Load<TextAsset>(fileName);
            }
            else
            {
                textAsset = Resources.Load<TextAsset>(string.Format("{0}/{1}", language, fileName));
            }
            if (textAsset == null && defaultLanguage != language)
            {
                textAsset = Resources.Load<TextAsset>(string.Format("{0}/{1}", defaultLanguage, fileName));
            }
            if (textAsset != null)
            {
                List<string> strings = new List<string>();
                StringReader reader = new StringReader(textAsset.text);
                while (reader.Peek() > 0)
                {
                    strings.Add(reader.ReadLine());
                }
                lines = strings.ToArray();
            }
            return (textAsset != null);
        }

        public void Unload()
        {
            if (textAsset != null)
            {
                Resources.UnloadAsset(textAsset);
                textAsset = null;
            }
            applied = false;
        }

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
                else if (line[i] == CsvSeperator && !quoted)
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
                                return string.Empty;
                            }
                        }
                    }
                }
            }
            return null;
        }

        private string GetPlainText(string[] lines)
        {
            if (lines != null && lines.Length > 0)
            {
                int lineIndex;
                try
                {
                    lineIndex = int.Parse(keyName);
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
                if (lineIndex >= 0 && lineIndex < lines.Length)
                {
                    return lines[lineIndex];
                }
                return null;
            }
            return null;
        }

        private string GetText()
        {
            if (!string.IsNullOrEmpty(columnName))
            {
                return GetCsvText(lines);
            }
            else
            {
                return GetPlainText(lines);
            }
        }

        public void Apply()
        {
            Text text = GetComponent<Text>();
            if (text != null)
            {
                string s = GetText();
                if (s != null)
                {
                    text.text = s;
                }
            }
            InputField inputField = GetComponent<InputField>();
            if (inputField != null)
            {
                string s = GetText();
                if (s != null)
                {
                    inputField.text = s;
                }
            }
            applied = true;
        }

        public void LoadAndApply()
        {
            if (isActiveAndEnabled)
            {
                StartCoroutine(Loading());
            }
            else
            {
                applied = false;
            }
        }
    }
}
