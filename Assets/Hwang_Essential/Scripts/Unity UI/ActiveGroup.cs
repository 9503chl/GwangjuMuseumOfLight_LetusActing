using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace UnityEngine.UI
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    public sealed class ActiveGroup : MonoBehaviour, IEnumerable
    {
        [SerializeField]
        private int activedIndex = -1;

        public int ActivedIndex
        {
            get
            {
                return activedIndex;
            }
            set
            {
                SetGameObjectActived(value);
            }
        }

        [NonSerialized]
        private GameObject activedObject;
        public GameObject ActivedObject
        {
            get
            {
                return activedObject;
            }
            set
            {
                if (SetGameObjectActived(value))
                {
                    CallOnChange();
                }
            }
        }

        [SerializeField]
        private bool active0ToIndex = false;

        public bool Active0ToIndex
        {
            get
            {
                return active0ToIndex;
            }
            set
            {
                if (active0ToIndex != value)
                {
                    active0ToIndex = value;
                    if (!SetGameObjectActived(activedIndex))
                    {
                        CallOnChange();
                    }
                }
            }
        }

        [SerializeField]
        private List<GameObject> gameObjects = new List<GameObject>();

        public GameObject this[int index]
        {
            get
            {
                return gameObjects[index];
            }
        }

        public int Count
        {
            get
            {
                return gameObjects.Count;
            }
        }

        public IEnumerator GetEnumerator()
        {
            return gameObjects.GetEnumerator();
        }

        public UnityEvent onChange;

#if UNITY_EDITOR
        [NonSerialized]
        private int oldCount = 0;

        [NonSerialized]
        private int oldIndex = -1;

        [NonSerialized]
        private bool zeroToIndex = false;

        public void OnValidate()
        {
            if (oldIndex != activedIndex)
            {
                SetGameObjectActived(activedIndex);
                oldIndex = activedIndex;
            }
            else if (oldCount != gameObjects.Count)
            {
                SetGameObjectActived(activedIndex);
                oldCount = gameObjects.Count;
            }
            else if (zeroToIndex != active0ToIndex)
            {
                zeroToIndex = active0ToIndex;
                if (!SetGameObjectActived(activedIndex))
                {
                    CallOnChange();
                }
            }
        }

        private void Start()
        {
            oldCount = gameObjects.Count;
            oldIndex = activedIndex;
            zeroToIndex = active0ToIndex;
        }
#endif

        private void OnEnable()
        {
            SetGameObjectActived(activedIndex);
        }

        private bool SetGameObjectActived(int index)
        {
            GameObject go = null;
            activedIndex = Mathf.Max(-1, index);
            if (index >= 0)
            {
                if (index < gameObjects.Count)
                {
                    go = gameObjects[index];
                }
                else if (active0ToIndex && gameObjects.Count > 0)
                {
                    go = gameObjects[gameObjects.Count - 1];
                }
            }
            if (SetGameObjectActived(go))
            {
                CallOnChange();
                return true;
            }
            return false;
        }

        private bool SetGameObjectActived(GameObject go)
        {
            int oldActivedIndex = activedIndex;
            GameObject oldActivedObject = activedObject;
            activedObject = go;
            if (!active0ToIndex && go != null)
            {
                if (activedIndex >= 0 && activedIndex < gameObjects.Count)
                {
                    if (gameObjects[activedIndex] != go)
                    {
                        int index = gameObjects.IndexOf(go);
                        if (index >= 0)
                        {
                            activedIndex = index;
                        }
                    }
                }
            }
            if (isActiveAndEnabled)
            {
                for (int i = 0; i < gameObjects.Count; i++)
                {
                    if (gameObjects[i] != null)
                    {
                        if (active0ToIndex)
                        {
                            gameObjects[i].SetActive(activedIndex >= gameObjects.Count || i <= activedIndex);
                        }
                        else
                        {
                            gameObjects[i].SetActive(gameObjects[i] == go);
                        }
                    }
                }
            }
            if (oldActivedIndex != activedIndex || oldActivedObject != go)
            {
                return true;
            }
            return false;
        }

        private void CallOnChange()
        {
            if (Application.isPlaying && isActiveAndEnabled)
            {
                if (onChange != null)
                {
                    onChange.Invoke();
                }
            }
        }

        public void SetActive(bool value)
        {
            gameObject.SetActive(value);
        }

        public void Add(GameObject go)
        {
            if (go != null)
            {
                gameObjects.Add(go);
                if (activedIndex == gameObjects.Count - 1)
                {
                    SetGameObjectActived(activedIndex);
                }
            }
        }

        public void Remove(GameObject go)
        {
            if (go != null)
            {
                gameObjects.Remove(go);
                if (go == activedObject)
                {
                    SetGameObjectActived(null);
                }
            }
        }

        public void Clear()
        {
            gameObjects.Clear();
            SetGameObjectActived(null);
        }
    }
}
