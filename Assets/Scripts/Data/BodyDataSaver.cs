using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Diagnostics.CodeAnalysis;
using LitJson;
using System.Linq;
using Unity.VisualScripting;

public class BodyDataSaver : MonoBehaviour
{
    private Transform bodyRoot;

    private Transform[] Bone_TFs;

    private BodyDataList[] bodyDataList;

    private Coroutine coroutine;

    private void Awake()
    {
        Transform[] gameObjects = GetComponentsInChildren<Transform>();

        for (int i = 0; i < gameObjects.Length; i++)
        {
            if (gameObjects[i].name == "root")
            {
                bodyRoot = gameObjects[i];
                break;
            }
        }
        Bone_TFs = bodyRoot.GetComponentsInChildren<Transform>();

        bodyDataList = WebServerUtility.dataArray;
    }

    public void SaveData()
    {
        bodyDataList[WebServerUtility.captureIndex].Clear();

        for (int j = 0; j < Bone_TFs.Length; j++)
        {
            BodyData bodyData = new BodyData();
            bodyDataList[WebServerUtility.captureIndex].Add(bodyData);
        }

        bodyDataList[WebServerUtility.captureIndex].FrameCount = 0;

        coroutine = StartCoroutine(ISaveData(WebServerUtility.captureIndex));
    }
    private IEnumerator ISaveData(int index)
    {
        float time = 0;
        while (time < ProjectSettings.TargetTime)
        {
            for (int i = 0; i < Bone_TFs.Length; i++)
            {
                bodyDataList[WebServerUtility.captureIndex].datas[i].AddingData(Bone_TFs[i].localPosition, Bone_TFs[i].localRotation);
            }
            time += Time.deltaTime;
            bodyDataList[WebServerUtility.captureIndex].FrameCount++;
            yield return null;
        }

        bodyDataList[WebServerUtility.captureIndex].SaveToJson();
        coroutine = null;
    }

    public bool HasBodyData()
    {
        int count = 0;

        for (int i = 0; i < bodyDataList.Length; i++)
        {
            if (!bodyDataList[i].IsEmpty()) count++;
        }

        return count == 5;
    }
}
