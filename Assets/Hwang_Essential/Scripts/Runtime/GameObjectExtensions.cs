using System;
using UnityEngine.UI;

namespace UnityEngine
{
    public static class GameObjectExtensions
    {
        public static Component EnsureComponent(this GameObject go, Type type)
        {
            Component result = go.GetComponent(type);
            if (result == null)
            {
                result = go.AddComponent(type);
            }
            return result;
        }

        public static T EnsureComponent<T>(this GameObject go) where T : Component
        {
            T result = go.GetComponent<T>();
            if (result == null)
            {
                result = go.AddComponent<T>();
            }
            return result;
        }

        public static void RemoveComponents(this GameObject go, Type type)
        {
            Component[] results = go.GetComponents(type);
            foreach (Component result in results)
            {
                Object.Destroy(result);
            }
        }

        public static void RemoveComponents<T>(this GameObject go) where T : Component
        {
            T[] results = go.GetComponents<T>();
            foreach (Component result in results)
            {
                Object.Destroy(result);
            }
        }

        public static void RemoveComponentsInChildren(this GameObject go, Type type)
        {
            Component[] results = go.GetComponentsInChildren(type);
            foreach (Component result in results)
            {
                Object.Destroy(result);
            }
        }

        public static void RemoveComponentsInChildren<T>(this GameObject go) where T : Component
        {
            T[] results = go.GetComponentsInChildren<T>();
            foreach (Component result in results)
            {
                Object.Destroy(result);
            }
        }

        private static void SetActiveRecursively(GameObject go)
        {
            if (go != null)
            {
                go.SetActive(true);
                Transform parent = go.transform.parent;
                if (parent != null && !parent.gameObject.activeInHierarchy)
                {
                    SetActiveRecursively(parent.gameObject);
                }
            }
        }

        public static void SetActiveInHierarchy(this GameObject go)
        {
            if (!go.activeInHierarchy)
            {
                SetActiveRecursively(go);
            }
        }
    }
}
