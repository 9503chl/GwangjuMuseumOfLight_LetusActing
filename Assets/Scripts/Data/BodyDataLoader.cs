using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using WebSocketSharp;

public class BodyDataLoader : MonoBehaviour
{
    [SerializeField] private GameObject BodyRoot;

    private Transform[] Bone_TFs;

    private List<Vector3> OriginPoses = new List<Vector3>();
    private List<Quaternion> OriginAngles = new List<Quaternion>();

    private BodyDataList dataList;

    private Coroutine coroutine;

    private string path = string.Empty;

    private int? index = null;

    void Awake()
    {
        Bone_TFs = BodyRoot.GetComponentsInChildren<Transform>();

        for(int i = 0; i<Bone_TFs.Length; i++)
        {
            OriginPoses.Add(Bone_TFs[i].transform.position);
            OriginAngles.Add(Bone_TFs[i].transform.rotation);
        }

        if (name.Contains("Viva"))
        {
            index = 0;
        }
        if (name.Contains("Dance"))
        {
            index = 1;
        }
        if (name.Contains("Anger"))
        {
            index = 2;
        }
        if (name.Contains("Surprise"))
        {
            index = 3;
        }
        if (name.Contains("Sad"))
        {
            index = 4;
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
            ContentPanel.Instance.PlayBtnOnOff((int)index, true);
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
        if (ProjectSettings.dataArray[(int)index] != null)
        {
            dataList = ProjectSettings.dataArray[(int)index];

            yield return new WaitForEndOfFrame();

            Debug.Log("Datas Load Success!");
        }
        yield return null;
    }

    private IEnumerator IPlayData()
    {
        yield return StartCoroutine(LoadDatas());

        ContentPanel.Instance.PlayBtnOnOff((int)index, false);

        Debug.Log("Playing Datas...");

        int frame = 0;
        while (frame < dataList.FrameCount)
        {
            for (int i = 0; i < Bone_TFs.Length; i++)
            {
                Bone_TFs[i].localPosition = dataList.datas[i].GetPosition(frame);
                Bone_TFs[i].localRotation = dataList.datas[i].GetRotation(frame);
            }
            frame++;
            yield return null;
        }
        StopData();
    }
}
