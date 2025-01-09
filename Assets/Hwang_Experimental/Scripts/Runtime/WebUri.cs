using System;
using System.Collections.Generic;
using System.Text;

namespace UnityEngine
{
    public class WebUri
    {
        private Dictionary<string, string> fields = new Dictionary<string, string>();
        private string baseUrl;

        private const string QuerySeperator = "?";
        private const string ValueSeperator = "=";
        private const string FieldSeperator = "&";

        public WebUri()
        {
        }

        public WebUri(string url)
        {
            baseUrl = url;
        }

        public WebUri(Uri uri)
        {
            baseUrl = uri.OriginalString;
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
                sb.Append(field.Value);
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
