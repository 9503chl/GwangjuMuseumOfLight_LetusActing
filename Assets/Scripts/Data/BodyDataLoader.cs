using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using WebSocketSharp;

public class BodyDataLoader : MonoBehaviour
{
    private Transform bodyRoot;

    private Transform[] Bone_TFs;

    private List<Vector3> OriginPoses = new List<Vector3>();
    private List<Quaternion> OriginAngles = new List<Quaternion>();

    private BodyDataList dataList;

    private Coroutine coroutine;

    private int index = 0;

    void Awake()
    {
        Transform[] gameObjects = GetComponentsInChildren<Transform>();

        for(int i = 0; i <gameObjects.Length; i++)
        {
            if(gameObjects[i].name == "root")
            {
                bodyRoot = gameObjects[i];
                break;
            }
        }

        Bone_TFs = bodyRoot.GetComponentsInChildren<Transform>();

        for(int i = 0; i<Bone_TFs.Length; i++)
        {
            OriginPoses.Add(Bone_TFs[i].transform.position);
            OriginAngles.Add(Bone_TFs[i].transform.rotation);
        }
    }

    private void OnEnable()
    {
        StopData();
    }

    public void PlayData()
    {
        coroutine = StartCoroutine(IPlayData());
    }

    public void StopData()
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
            ContentPanel.Instance.PlayBtnOnOff(index, true);
            Debug.Log(string.Format("All Data Played ! Count : {0}", dataList.FrameCount));
        }
        for(int i = 0;i < Bone_TFs.Length; i++)
        {
            Bone_TFs[i].transform.position = OriginPoses[i];
            Bone_TFs[i].transform.rotation = OriginAngles[i];
        }
    }


    private IEnumerator LoadDatas()
    {
        if (WebServerUtility.dataArray[WebServerUtility.captureIndex] != null)
        {
            dataList = WebServerUtility.dataArray[WebServerUtility.captureIndex];

            yield return new WaitForEndOfFrame();

            Debug.Log("Datas Load Success!");
        }
        yield return null;
    }

    private IEnumerator IPlayData()
    {
        yield return StartCoroutine(LoadDatas());

        ContentPanel.Instance.PlayBtnOnOff(WebServerUtility.captureIndex, false);
        index = WebServerUtility.captureIndex;

        Debug.Log("Playing Datas...");

        float fps = dataList.FrameCount / dataList.Duration;
        float delay = 1f / fps;
        //Debug.Log(string.Format("Playing body data : {0} seconds, {1} fps", dataList.Duration, fps));
        int frame = 0, prevFrame = -1;
        float time = 0f;
        if (isActiveAndEnabled)
        {
            while (frame < dataList.FrameCount)
            {
                if (prevFrame != frame)
                {
                    for (int i = 0; i < Bone_TFs.Length; i++)
                    {
                        Bone_TFs[i].localPosition = dataList.datas[i].GetPosition(frame);
                        Bone_TFs[i].localRotation = dataList.datas[i].GetRotation(frame);
                    }
                    prevFrame = frame;
                }
                time += Time.deltaTime;
                if (time > delay)
                {
                    frame += (int)(time / delay);
                    time = 0f;
                }
                yield return null;
            }
            frame = 0;
        }
        StopData();
    }
}
