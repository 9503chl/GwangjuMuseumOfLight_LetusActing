using System;

namespace System.Xml
{
    public static class XmlExtensions
    {
        public static XmlDeclaration AppendXmlDeclaration(this XmlDocument document, string version = "1.0", string encoding = "utf-8", string standalone = "yes")
        {
            return document.AppendChild(document.CreateXmlDeclaration(version, encoding, standalone)) as XmlDeclaration;
        }

        public static XmlElement AppendElement(this XmlNode node, string name)
        {
            if (node is XmlDocument)
            {
                return node.AppendChild((node as XmlDocument).CreateElement(name)) as XmlElement;
            }
            else if (node.OwnerDocument != null)
            {
                return node.AppendChild(node.OwnerDocument.CreateElement(name)) as XmlElement;
            }
            return null;
        }

        public static XmlComment AppendComment(this XmlNode node, string data)
        {
            if (node is XmlDocument)
            {
                return node.AppendChild((node as XmlDocument).CreateComment(string.Format(" {0} ", data))) as XmlComment;
            }
            else if (node.OwnerDocument != null)
            {
                return node.AppendChild(node.OwnerDocument.CreateComment(string.Format(" {0} ", data))) as XmlComment;
            }
            return null;
        }

        public static bool HasChild(this XmlNode node, string xpath)
        {
            return node.SelectSingleNode(xpath) != null;
        }

        public static XmlNode RemoveChild(this XmlNode node, string xpath)
        {
            XmlNode childNode = node.SelectSingleNode(xpath);
            if (childNode != null)
            {
                return node.RemoveChild(childNode);
            }
            return null;
        }

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
                if (node is XmlDocument)
                {
                    childNode = node.AppendChild((node as XmlDocument).CreateElement(xpath));
                }
                else if (node.OwnerDocument != null)
                {
                    childNode = node.AppendChild(node.OwnerDocument.CreateElement(xpath));
                }
            }
            if (!string.IsNullOrEmpty(value))
            {
                childNode.InnerText = value;
            }
        }

        public static void WriteBool(this XmlNode node, string xpath, bool value)
        {
            XmlNode childNode = node.SelectSingleNode(xpath);
            if (childNode == null)
            {
                if (node is XmlDocument)
                {
                    childNode = node.AppendChild((node as XmlDocument).CreateElement(xpath));
                }
                else if (node.OwnerDocument != null)
                {
                    childNode = node.AppendChild(node.OwnerDocument.CreateElement(xpath));
                }
            }
            childNode.InnerText = Convert.ToString(value);
        }

        public static void WriteInt(this XmlNode node, string xpath, int value)
        {
            XmlNode childNode = node.SelectSingleNode(xpath);
            if (childNode == null)
            {
                if (node is XmlDocument)
                {
                    childNode = node.AppendChild((node as XmlDocument).CreateElement(xpath));
                }
                else if (node.OwnerDocument != null)
                {
                    childNode = node.AppendChild(node.OwnerDocument.CreateElement(xpath));
                }
            }
            childNode.InnerText = Convert.ToString(value);
        }

        public static void WriteFloat(this XmlNode node, string xpath, float value)
        {
            XmlNode childNode = node.SelectSingleNode(xpath);
            if (childNode == null)
            {
                if (node is XmlDocument)
                {
                    childNode = node.AppendChild((node as XmlDocument).CreateElement(xpath));
                }
                else if (node.OwnerDocument != null)
                {
                    childNode = node.AppendChild(node.OwnerDocument.CreateElement(xpath));
                }
            }
            childNode.InnerText = Convert.ToString(value);
        }

        public static XmlAttribute FindAttribute(this XmlNode node, string name)
        {
            if (node.Attributes != null)
            {
                for (int i = 0; i < node.Attributes.Count; i++)
                {
                    if (string.Compare(node.Attributes[i].Name, name) == 0)
                    {
                        return node.Attributes[i];
                    }
                }
            }
            return null;
        }

        public static XmlAttribute FindAttribute(this XmlNode node, string name, string value)
        {
            if (node.Attributes != null)
            {
                for (int i = 0; i < node.Attributes.Count; i++)
                {
                    if (string.Compare(node.Attributes[i].Name, name) == 0 && string.Compare(node.Attributes[i].Value, value) == 0)
                    {
                        return node.Attributes[i];
                    }
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

        public static XmlAttribute AppendAttribute(this XmlNode node, string name)
        {
            if (node.Attributes != null)
            {
                XmlAttribute attribute = node.Attributes[name];
                if (attribute == null && node.OwnerDocument != null)
                {
                    attribute = node.Attributes.Append(node.OwnerDocument.CreateAttribute(name));
                }
                return attribute;
            }
            return null;
        }

        public static XmlAttribute RemoveAttribute(this XmlNode node, string name)
        {
            if (node.Attributes != null)
            {
                XmlAttribute attribute = node.Attributes[name];
                if (attribute != null)
                {
                    attribute = node.Attributes.Remove(attribute);
                }
                return attribute;
            }
            return null;
        }

        public static string ReadAttributeString(this XmlNode node, string name, string defaultValue = null)
        {
            if (node.Attributes != null)
            {
                XmlAttribute attribute = node.Attributes[name];
                if (attribute != null)
                {
                    return attribute.Value;
                }
            }
            return defaultValue;
        }

        public static bool ReadAttributeBool(this XmlNode node, string name, bool defaultValue = false)
        {
            if (node.Attributes != null)
            {
                XmlAttribute attribute = node.Attributes[name];
                if (attribute != null)
                {
                    try
                    {
                        return Convert.ToBoolean(attribute.Value);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            return defaultValue;
        }

        public static int ReadAttributeInt(this XmlNode node, string name, int defaultValue = 0)
        {
            if (node.Attributes != null)
            {
                XmlAttribute attribute = node.Attributes[name];
                if (attribute != null)
                {
                    try
                    {
                        return Convert.ToInt32(attribute.Value);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            return defaultValue;
        }

        public static float ReadAttributeFloat(this XmlNode node, string name, float defaultValue = 0f)
        {
            if (node.Attributes != null)
            {
                XmlAttribute attribute = node.Attributes[name];
                if (attribute != null)
                {
                    try
                    {
                        return Convert.ToSingle(attribute.Value);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            return defaultValue;
        }

        public static void WriteAttributeString(this XmlNode node, string name, string value)
        {
            if (node.Attributes != null)
            {
                XmlAttribute attribute = node.Attributes[name];
                if (attribute == null)
                {
                    if (node is XmlDocument)
                    {
                        attribute = node.Attributes.Append((node as XmlDocument).CreateAttribute(name));
                    }
                    else if (node.OwnerDocument != null)
                    {
                        attribute = node.Attributes.Append(node.OwnerDocument.CreateAttribute(name));
                    }
                }
                attribute.Value = value;
            }
        }

        public static void WriteAttributeBool(this XmlNode node, string name, bool value)
        {
            if (node.Attributes != null)
            {
                XmlAttribute attribute = node.Attributes[name];
                if (attribute == null)
                {
                    if (node is XmlDocument)
                    {
                        attribute = node.Attributes.Append((node as XmlDocument).CreateAttribute(name));
                    }
                    else if (node.OwnerDocument != null)
                    {
                        attribute = node.Attributes.Append(node.OwnerDocument.CreateAttribute(name));
                    }
                }
                attribute.Value = Convert.ToString(value);
            }
        }

        public static void WriteAttributeInt(this XmlNode node, string name, int value)
        {
            if (node.Attributes != null)
            {
                XmlAttribute attribute = node.Attributes[name];
                if (attribute == null)
                {
                    if (node is XmlDocument)
                    {
                        attribute = node.Attributes.Append((node as XmlDocument).CreateAttribute(name));
                    }
                    else if (node.OwnerDocument != null)
                    {
                        attribute = node.Attributes.Append(node.OwnerDocument.CreateAttribute(name));
                    }
                }
                attribute.Value = Convert.ToString(value);
            }
        }

        public static void WriteAttributeFloat(this XmlNode node, string name, float value)
        {
            if (node.Attributes != null)
            {
                XmlAttribute attribute = node.Attributes[name];
                if (attribute == null)
                {
                    if (node is XmlDocument)
                    {
                        attribute = node.Attributes.Append((node as XmlDocument).CreateAttribute(name));
                    }
                    else if (node.OwnerDocument != null)
                    {
                        attribute = node.Attributes.Append(node.OwnerDocument.CreateAttribute(name));
                    }
                }
                attribute.Value = Convert.ToString(value);
            }
        }
    }
}
