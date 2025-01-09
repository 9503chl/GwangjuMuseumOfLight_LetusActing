using System;

namespace UnityEngine
{
    public static class ComponentExtensions
    {
        public static Component EnsureComponent(this Component component, Type type)
        {
            Component result = component.GetComponent(type);
            if (result == null)
            {
                result = component.gameObject.AddComponent(type);
            }
            return result;
        }

        public static T EnsureComponent<T>(this Component component) where T : Component
        {
            T result = component.GetComponent<T>();
            if (result == null)
            {
                result = component.gameObject.AddComponent<T>();
            }
            return result;
        }

        public static void RemoveComponents(this Component component, Type type)
        {
            Component[] results = component.GetComponents<Component>();
            foreach (Component result in results)
            {
                Object.Destroy(result);
            }
        }

        public static void RemoveComponents<T>(this Component component) where T : Component
        {
            T[] results = component.GetComponents<T>();
            foreach (Component result in results)
            {
                Object.Destroy(result);
            }
        }

        public static void RemoveComponentsInChildren(this Component component, Type type)
        {
            Component[] results = component.GetComponentsInChildren<Component>();
            foreach (Component result in results)
            {
                Object.Destroy(result);
            }
        }

        public static void RemoveComponentsInChildren<T>(this Component component) where T : Component
        {
            T[] results = component.GetComponentsInChildren<T>();
            foreach (Component result in results)
            {
                Object.Destroy(result);
            }
        }
    }
}
