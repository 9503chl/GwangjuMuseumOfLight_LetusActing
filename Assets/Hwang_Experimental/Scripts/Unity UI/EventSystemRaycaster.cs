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
        private List<RaycastResult> raycastResultList = new List<RaycastResult>();

        private static EventSystemRaycaster instance;
        public static EventSystemRaycaster Instance
        {
            get
            {
                if (instance == null)
                {
#if UNITY_2020_1_OR_NEWER
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
#if UNITY_2020_1_OR_NEWER
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
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = Input.mousePosition;
            List<RaycastResult> resultList = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, resultList);
            raycastResultList.Clear();
            raycastResultList.AddRange(resultList);
            resultList.Clear();
        }

        public bool IsPointerOverGameObject(GameObject targetObject, bool passThroughFamily)
        {
            GameObject topMostObject = null;
            int maxDepth = -1;
            bool rayHitTarget = false;
            RaycastResult[] raycastResults = raycastResultList.ToArray();
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
