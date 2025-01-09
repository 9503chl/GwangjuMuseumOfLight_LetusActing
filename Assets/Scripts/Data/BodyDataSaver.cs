using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Diagnostics.CodeAnalysis;

public class BodyDataSaver : MonoBehaviour
{
    [SerializeField] private GameObject BodyRoot;

    private Transform[] Bone_TFs;

    private BodyDataList[] bodyDataList;

    private Coroutine coroutine;

    private void Awake()
    {
        Bone_TFs = BodyRoot.GetComponentsInChildren<Transform>();
        bodyDataList = DataSettings.dataArray;

        for (int i = 0; i < 5; i++)
        {
            bodyDataList[i] = new BodyDataList();
            for (int j = 0; j < Bone_TFs.Length; j++)
            {
                BodyData bodyData = new BodyData();
                bodyDataList[i].Add(bodyData);
            }
        }
    }

    public void SaveData()
    {
        Debug.Log("BodyData Saving...");

        int index = DataSettings.captureIndex;

        for (int i = 0; i < bodyDataList[index].FrameCount; i++)
        {
            bodyDataList[index].datas.Clear();
        }
        bodyDataList[index].FrameCount = 0;

        coroutine = StartCoroutine(ISaveData(DataSettings.captureIndex));
    }
    private IEnumerator ISaveData(int index)
    {
        float time = 0;
        while (time < DataSettings.TargetTime)
        {
            for (int i = 0; i < Bone_TFs.Length; i++)
            {
                bodyDataList[index].datas[i].AddingData(Bone_TFs[i].localPosition, Bone_TFs[i].localRotation);
            }
            time += Time.deltaTime;
            bodyDataList[index].FrameCount++;
            yield return null;
        }

        Debug.Log(string.Format("BodyData is Saved"));
        coroutine = null;
    }
}
