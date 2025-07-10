using System;
using System.Collections.Generic;
using System.Text;

namespace UnityEngine
{
    public class WebUri
    {
        private string baseUrl;
        public string BaseUrl
        {
            get { return baseUrl; }
        }

        private readonly Dictionary<string, string> fields = new Dictionary<string, string>();

        private const char QuerySeperator = '?';
        private const char ValueSeperator = '=';
        private const char FieldSeperator = '&';

        public WebUri()
        {
        }

        public WebUri(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                int p = url.IndexOf(QuerySeperator);
                if (p == -1)
                {
                    baseUrl = url;
                }
                else
                {
                    baseUrl = url.Substring(0, p);
                    string queries = url.Substring(p + 1, url.Length - p - 1);
                    string[] parts = queries.Split(FieldSeperator);
                    foreach (string part in parts)
                    {
                        if (!string.IsNullOrEmpty(part))
                        {
                            p = part.IndexOf(ValueSeperator);
                            if (p == -1)
                            {
                                fields.Add(part, string.Empty);
                            }
                            else
                            {
                                fields.Add(part.Substring(0, p), Uri.UnescapeDataString(part.Substring(p + 1, part.Length - p - 1).Replace('+', ' ')));
                            }
                        }
                    }
                }
            }
            else
            {
                baseUrl = string.Empty;
            }
        }

        public WebUri(Uri uri) : this(uri.OriginalString)
        {
        }

        public string GetFieldValue(string fieldName)
        {
            if (fields.ContainsKey(fieldName))
            {
                return fields[fieldName];
            }
            return null;
        }

        public void AddField(string fieldName, string value)
        {
            fields.Add(fieldName, value);
        }

        public void AddField(string fieldName, int value)
        {
            fields.Add(fieldName, Convert.ToString(value));
        }

        public void AddField(string fieldName, float value)
        {
            fields.Add(fieldName, Convert.ToString(value));
        }

        public void RemoveField(string fieldName)
        {
            fields.Remove(fieldName);
        }

        public void ClearAllFields()
        {
            fields.Clear();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrEmpty(baseUrl))
            {
                sb.Append(baseUrl);
                if (fields.Count > 0)
                {
                    sb.Append(QuerySeperator);
                }
            }
            int count = 0;
            foreach (KeyValuePair<string, string> field in fields)
            {
                sb.Append(field.Key);
                sb.Append(ValueSeperator);
                if (!string.IsNullOrEmpty(field.Value))
                {
                    sb.Append(Uri.EscapeUriString(field.Value));
                }
                if (fields.Count > ++count)
                {
                    sb.Append(FieldSeperator);
                }
            }
            return sb.ToString();
        }

        public static explicit operator string(WebUri webUri)
        {
            return webUri.ToString();
        }

        public static implicit operator Uri(WebUri webUri)
        {
            return new Uri(webUri.ToString());
        }
    }
}
