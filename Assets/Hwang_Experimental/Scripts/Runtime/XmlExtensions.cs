using System;

namespace System.Xml
{
    public static class XmlExtensions
    {
        public static string ReadString(this XmlNode node, string xpath, string defaultValue = null)
        {
            XmlNode childNode = node.SelectSingleNode(xpath);
            if (childNode != null)
            {
                return childNode.InnerText;
            }
            return defaultValue;
        }

        public static bool ReadBool(this XmlNode node, string xpath, bool defaultValue = false)
        {
            XmlNode childNode = node.SelectSingleNode(xpath);
            if (childNode != null)
            {
                try
                {
                    return Convert.ToBoolean(childNode.InnerText);
                }
                catch (Exception)
                {
                }
            }
            return defaultValue;
        }

        public static int ReadInt(this XmlNode node, string xpath, int defaultValue = 0)
        {
            XmlNode childNode = node.SelectSingleNode(xpath);
            if (childNode != null)
            {
                try
                {
                    return Convert.ToInt32(childNode.InnerText);
                }
                catch (Exception)
                {
                }
            }
            return defaultValue;
        }

        public static float ReadFloat(this XmlNode node, string xpath, float defaultValue = 0f)
        {
            XmlNode childNode = node.SelectSingleNode(xpath);
            if (childNode != null)
            {
                try
                {
                    return Convert.ToSingle(childNode.InnerText);
                }
                catch (Exception)
                {
                }
            }
            return defaultValue;
        }

        public static void WriteString(this XmlNode node, string xpath, string value)
        {
            XmlNode childNode = node.SelectSingleNode(xpath);
            if (childNode == null)
            {
                childNode = node.AppendChild(node.OwnerDocument.CreateElement(xpath));
            }
            childNode.InnerText = value;
        }

        public static void WriteBool(this XmlNode node, string xpath, bool value)
        {
            XmlNode childNode = node.SelectSingleNode(xpath);
            if (childNode == null)
            {
                childNode = node.AppendChild(node.OwnerDocument.CreateElement(xpath));
            }
            childNode.InnerText = Convert.ToString(value);
        }

        public static void WriteInt(this XmlNode node, string xpath, int value)
        {
            XmlNode childNode = node.SelectSingleNode(xpath);
            if (childNode == null)
            {
                childNode = node.AppendChild(node.OwnerDocument.CreateElement(xpath));
            }
            childNode.InnerText = Convert.ToString(value);
        }

        public static void WriteFloat(this XmlNode node, string xpath, float value)
        {
            XmlNode childNode = node.SelectSingleNode(xpath);
            if (childNode == null)
            {
                childNode = node.AppendChild(node.OwnerDocument.CreateElement(xpath));
            }
            childNode.InnerText = Convert.ToString(value);
        }

        public static XmlAttribute FindAttribute(this XmlNode node, string name)
        {
            for (int i = 0; i < node.Attributes.Count; i++)
            {
                if (string.Compare(node.Attributes[i].Name, name) == 0)
                {
                    return node.Attributes[i];
                }
            }
            return null;
        }

        public static XmlAttribute FindAttribute(this XmlNode node, string name, string value)
        {
            for (int i = 0; i < node.Attributes.Count; i++)
            {
                if (string.Compare(node.Attributes[i].Name, name) == 0 && string.Compare(node.Attributes[i].Value, value) == 0)
                {
                    return node.Attributes[i];
                }
            }
            return null;
        }

        public static bool HasAttribute(this XmlNode node, string name)
        {
            return FindAttribute(node, name) != null;
        }

        public static bool HasAttribute(this XmlNode node, string name, string value)
        {
            return FindAttribute(node, name, value) != null;
        }

        public static XmlNode FindNodeByAttribute(this XmlNodeList nodes, string name, string value)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                if (HasAttribute(nodes[i], name, value))
                {
                    return nodes[i];
                }
            }
            return null;
        }
    }
}
