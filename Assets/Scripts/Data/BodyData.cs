using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using LitJson;

[Serializable]
public class BodyData 
{
    public List<float> pos_x= new List<float>();
    public List<float> pos_y = new List<float>();
    public List<float> pos_z = new List<float>();
        
    public List<float> rotation_x= new List<float>();
    public List<float> rotation_y = new List<float>();
    public List<float> rotation_z = new List<float>();
    public List<float> rotation_w = new List<float>();

    public void AddingData(Vector3 pos, Quaternion angles)
    {
        pos_x.Add(pos.x);
        pos_y.Add(pos.y);
        pos_z.Add(pos.z);

        rotation_x.Add(angles.x);
        rotation_y.Add(angles.y);
        rotation_z.Add(angles.z);
        rotation_w.Add(angles.w);
    }

    public Vector3 GetPosition(int index)
    {
        return new Vector3(pos_x[index], pos_y[index], pos_z[index]);
    }
    public Quaternion GetRotation(int index)
    {
        return new Quaternion(rotation_x[index], rotation_y[index], rotation_z[index], rotation_w[index]);
    }

    public void Clear()
    {
        pos_x.Clear();
        pos_y.Clear();
        pos_z.Clear();

        rotation_x.Clear();
        rotation_y.Clear();
        rotation_z.Clear();
        rotation_w.Clear();
    }
}

[Serializable]
public class BodyDataList
{
    public List<BodyData> datas = new List<BodyData>();

    public int FrameCount = 0;

    public float Duration = 3;

    public string json = string.Empty;

    public void SaveToJson()
    {
        JsonWriter jsonWriter = new JsonWriter();
        jsonWriter.PrettyPrint = true;

        JsonMapper.ToJson(this, jsonWriter);
        json = jsonWriter.ToString();
    }

    public void Add(BodyData bodyData)
    {
        datas.Add(bodyData);
    }
    public void Clear()
    {
        for(int i = 0; i< datas.Count; i++)
        {
            datas[i].Clear();
        }
        json = string.Empty;
        FrameCount = 0;
    }

    public bool IsEmpty()
    {
        return datas.Count == 0;
    }
}
