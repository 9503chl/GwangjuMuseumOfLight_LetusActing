using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EventSystem))]
    public class EventSystemRaycaster : MonoBehaviour
    {
        [NonSerialized]
        private RaycastResult[] raycastResults = new RaycastResult[0];

        private static EventSystemRaycaster instance;
        public static EventSystemRaycaster Instance
        {
            get
            {
                if (instance == null)
                {
#if UNITY_2022_2_OR_NEWER || UNITY_2021_3
                    EventSystemRaycaster[] templates = FindObjectsByType<EventSystemRaycaster>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#elif UNITY_2020_1_OR_NEWER
                    EventSystemRaycaster[] templates = FindObjectsOfType<EventSystemRaycaster>(true);
#else
                    EventSystemRaycaster[] templates = FindObjectsOfType<EventSystemRaycaster>();
#endif
                    if (templates.Length > 0)
                    {
                        instance = templates[0];
                        instance.enabled = true;
                        instance.gameObject.SetActive(true);
                    }
                    else
                    {
#if UNITY_2022_2_OR_NEWER || UNITY_2021_3
                        EventSystem eventSystem = FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include);
#elif UNITY_2020_1_OR_NEWER
                        EventSystem eventSystem = FindObjectOfType<EventSystem>(true);
#else
                        EventSystem eventSystem = FindObjectOfType<EventSystem>();
#endif
                        instance = eventSystem.gameObject.AddComponent<EventSystemRaycaster>();
                        instance.enabled = true;
                    }
                }
                return instance;
            }
        }

        private void Awake()
        {
            instance = this;
        }

        private void FixedUpdate()
        {
            raycastResults = RaycastAll();
        }

        public static RaycastResult[] RaycastAll()
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = Input.mousePosition;
            List<RaycastResult> resultList = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, resultList);
            RaycastResult[] results = resultList.ToArray();
            resultList.Clear();
            return results;
        }

        public GameObject GetGameObjectOverPointer()
        {
            GameObject result = null;
            int maxDepth = -1;
            foreach (RaycastResult raycastResult in raycastResults)
            {
                if (maxDepth < raycastResult.depth)
                {
                    maxDepth = raycastResult.depth;
                    result = raycastResult.gameObject;
                }
            }
            return result;
        }

        public bool IsPointerOverGameObject(Type type, params Type[] otherTypes)
        {
            GameObject currentGameObject = GetGameObjectOverPointer();
            if (currentGameObject != null)
            {
                if (currentGameObject.GetType() == type || currentGameObject.GetComponentInParent(type) != null)
                {
                    return true;
                }
                foreach (Type otherType in otherTypes)
                {
                    if (currentGameObject.GetType() == otherType || currentGameObject.GetComponentInParent(otherType) != null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool IsPointerOverGameObject<T>()
        {
            return IsPointerOverGameObject(typeof(T));
        }

        public bool IsPointerOverGameObject(GameObject targetObject, bool passThroughFamily)
        {
            GameObject topMostObject = null;
            int maxDepth = -1;
            bool rayHitTarget = false;
            foreach (RaycastResult raycastResult in raycastResults)
            {
                if (raycastResult.gameObject == targetObject)
                {
                    rayHitTarget = true;
                }
                if (maxDepth < raycastResult.depth)
                {
                    maxDepth = raycastResult.depth;
                    topMostObject = raycastResult.gameObject;
                }
            }
            if (rayHitTarget && topMostObject != null)
            {
                if (passThroughFamily)
                {
                    return topMostObject.transform.IsChildOf(targetObject.transform) || targetObject.transform.parent == topMostObject.transform.parent;
                }
                else
                {
                    return targetObject == topMostObject;
                }
            }
            return false;
        }
    }
}
