using System;
using UnityEngine;

namespace UnityEngine.UI
{
    public class ComponentTag : MonoBehaviour
    {
        [TextArea(3, 12)]
        public string Description;

        [NonSerialized]
        public object Value;
    }

    public static class ComponentTagExtensions
    {
        public static string GetDescription(this Component component)
        {
            if (component != null)
            {
                ComponentTag tag = component.GetComponent<ComponentTag>();
                if (tag != null)
                {
                    return tag.Description;
                }
            }
            return null;
        }

        public static void SetDescription(this Component component, string description)
        {
            if (component != null)
            {
                ComponentTag tag = component.GetComponent<ComponentTag>();
                if (tag == null)
                {
                    tag = component.gameObject.AddComponent<ComponentTag>();
                }
                tag.Description = description;
            }
        }

        public static string GetDescription(this GameObject go)
        {
            if (go != null)
            {
                ComponentTag tag = go.GetComponent<ComponentTag>();
                if (tag != null)
                {
                    return tag.Description;
                }
            }
            return null;
        }

        public static void SetDescription(this GameObject go, string description)
        {
            if (go != null)
            {
                ComponentTag tag = go.GetComponent<ComponentTag>();
                if (tag == null)
                {
                    tag = go.AddComponent<ComponentTag>();
                }
                tag.Description = description;
            }
        }

        public static object GetComponentTag(this Component component)
        {
            if (component != null)
            {
                ComponentTag tag = component.GetComponent<ComponentTag>();
                if (tag != null)
                {
                    return tag.Value;
                }
            }
            return null;
        }

        public static void SetComponentTag(this Component component, object value)
        {
            if (component != null)
            {
                ComponentTag tag = component.GetComponent<ComponentTag>();
                if (tag == null)
                {
                    tag = component.gameObject.AddComponent<ComponentTag>();
                }
                tag.Value = value;
            }
        }

        public static object GetComponentTag(this GameObject go)
        {
            if (go != null)
            {
                ComponentTag tag = go.GetComponent<ComponentTag>();
                if (tag != null)
                {
                    return tag.Value;
                }
            }
            return null;
        }

        public static void SetComponentTag(this GameObject go, object value)
        {
            if (go != null)
            {
                ComponentTag tag = go.GetComponent<ComponentTag>();
                if (tag == null)
                {
                    tag = go.AddComponent<ComponentTag>();
                }
                tag.Value = value;
            }
        }
    }
}
