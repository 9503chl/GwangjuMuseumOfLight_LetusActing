using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER
using UnityEngine.Pool;

public class PoolManager : PivotalManager
{

    public int ObjectCapacity;

    public GameObject ObjectPrefab;

    public IObjectPool<GameObject> ObjectPool { get; private set; }

    private List<GameObject> objList = new List<GameObject>();

    public override void OnAwake()
    {
        Init();

        base.OnAwake();
    }


    private void Init()
    {
        ObjectPool = new UnityEngine.Pool.ObjectPool<GameObject>(CreateItem, OnTakeFromPool, OnReturnedToPool,
        OnDestroyPoolObject, true, ObjectCapacity, ObjectCapacity);
        for (int i = 0; i < ObjectCapacity; i++)
        {
            GameObject Paper = CreateItem();
            ObjectPool.Release(Paper.gameObject);
        }
    }
    private GameObject CreateItem()
    {
        GameObject poolGo = Instantiate(ObjectPrefab);
        objList.Add(poolGo);
        return poolGo;
    }
    private void OnTakeFromPool(GameObject poolGo)
    {

    }
    private void OnReturnedToPool(GameObject poolGo)
    {
    }
    private void OnDestroyPoolObject(GameObject poolGo)
    {
        //Destroy(poolGo);
    }

    public override void Clear(bool @true)
    {
        if (!@true)
        {
            for(int i = 0;i< objList.Count; i++)
            {
                ObjectPool.Release(objList[i]);
            }
            objList.Clear();
        }

        base.Clear(@true);
    }
}
#endif