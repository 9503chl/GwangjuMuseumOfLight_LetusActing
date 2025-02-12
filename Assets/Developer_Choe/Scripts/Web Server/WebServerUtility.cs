using LitJson;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Networking;

public class WebServerUtility : MonoBehaviour
{
    public static WebServerUtility Instance;

    private Coroutine getCoroutine;
    private Coroutine postCoroutine;

    

    private void Awake()
    {
        Instance = FindObjectOfType<WebServerUtility>(true);
    }
 
    public void ApiGet(string user_id, string student_id)
    {
        if(getCoroutine == null)
            getCoroutine = StartCoroutine(IApiGet(user_id, student_id));
    }

    private int fileCount = 0;

    private IEnumerator IApiGet(string user_id, string student_id)
    {
        WebUri webUri = new WebUri(WebServerData.GetUrls[2].ToString());
        webUri.AddField("user_id", user_id);
        webUri.AddField("student_id", student_id);

        UnityWebRequest www = UnityWebRequest.Get(webUri.ToString());
        www.useHttpContinue = false;
        www.downloadHandler = new DownloadHandlerBuffer();
        www.disposeDownloadHandlerOnDispose = true;

        www.timeout = 30;
        fileCount = 0;

        yield return www.SendWebRequest();

        if (www.error != null)
        {
            Debug.LogError(www.error);
        }
        else
        {
            string json = www.downloadHandler.text;
            JsonData data = JsonMapper.ToObject(json);
            //if (data != null && data["friend_name"].ToString() != string.Empty)
            if (data != null)
            {
                //성공
                WebServerData.userId = data["user_id"].ToString();
                WebServerData.studentId = data["student_id"].ToString();
                WebServerData.userName = data["user_name"].ToString();
                WebServerData.characterType = data["character_type"].ToString();

                StartCoroutine(DownloadRequest(WebServerData.facialExpression1, data["facial_expression_1"].ToString()));

                StartCoroutine(DownloadRequest(WebServerData.materialTexture1, data["material_texture_1"].ToString()));
                StartCoroutine(DownloadRequest(WebServerData.materialTexture2, data["material_texture_2"].ToString()));

                yield return new WaitUntil(() => fileCount == 3);

                Debug.Log(www.downloadHandler.text);
            }
            else
            {
                Debug.LogError(www.downloadHandler.text);
            }
        }
        www.Dispose();
        getCoroutine = null;
    }

    public void ApiE1Post(string user_id, string student_id, List<byte[]> expressionDatas, List<byte[]> textureDatas)
    {
        if(postCoroutine == null)
            postCoroutine = StartCoroutine(ApiE1PostInfo(user_id, student_id, expressionDatas, textureDatas));
    }

    private IEnumerator ApiE1PostInfo(string user_id, string student_id, List<byte[]> expressionDatas, List<byte[]> textureDatas)
    {
        WWWForm form = new WWWForm();
        form.AddField("user_id", user_id);
        form.AddField("student_id", student_id);

        for (int i = 0; i < 4; i++)
        {
            form.AddBinaryData(string.Format("facial_expression_{0}", i+1), expressionDatas[i]);
        }
        for (int i = 0; i < 5; i++)
        {
            form.AddBinaryData(string.Format("material_texture_{0}", i + 1), textureDatas[i]);
        }

        UnityWebRequest www = UnityWebRequest.Post(WebServerData.PostUrls[0].ToString(), form);
        www.useHttpContinue = false;

        yield return www.SendWebRequest();

        if (www.error != null)
        {
            Debug.LogError(www.error);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);
        }
        www.Dispose();
        postCoroutine = null;
    }

    public void ApiE2Post(string user_id, string student_id, string friend_name, string villain_name, string bg_name, string scenario_text)
    {
        if(postCoroutine == null)
            postCoroutine = StartCoroutine(ApiE2PostInfo(user_id, student_id, friend_name, villain_name, bg_name, scenario_text));
    }

    private IEnumerator ApiE2PostInfo(string user_id, string student_id, string friend_name, string villain_name, string bg_name, string scenario_text)
    {
        WWWForm form = new WWWForm();
        form.AddField("user_id", user_id);
        form.AddField("student_id", student_id);
        form.AddField("friend_name", friend_name);
        form.AddField("villain_name", villain_name);
        form.AddField("bg_name", bg_name);
        form.AddField("scenario_text", scenario_text);
        

        UnityWebRequest www = UnityWebRequest.Post(WebServerData.PostUrls[1].ToString(), form);
        www.useHttpContinue = false;

        yield return www.SendWebRequest();

        if (www.error != null)
        {
            Debug.LogError(www.error);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);
        }
        www.Dispose();
        postCoroutine = null;
    }

    public void ApiE3Post(string user_id, string student_id, BodyDataList[] dataList)
    {
        if (postCoroutine == null)
            postCoroutine = StartCoroutine(ApiE3PostInfo(user_id, student_id, dataList));
    }

    private IEnumerator ApiE3PostInfo(string user_id, string student_id, BodyDataList[] dataList)
    {
        WWWForm form = new WWWForm();
        form.AddField("user_id", user_id);
        form.AddField("student_id", student_id);

        for (int i = 0; i < 5; i++)
        {
            form.AddField(string.Format("motion_data_{0}", i + 1), dataList[i].json);
        }

        UnityWebRequest www = UnityWebRequest.Post(WebServerData.PostUrls[2].ToString(), form);
        www.useHttpContinue = false;

        yield return www.SendWebRequest();

        if (www.error != null)
        {
            Debug.LogError(www.error);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);
        }
        www.Dispose();
        postCoroutine = null;
    }

    public void ApiE4Post(string user_id, string student_id, byte[] videoFile)
    {
        if(postCoroutine == null)
            postCoroutine = StartCoroutine(ApiE4PostInfo(user_id, student_id, videoFile));
    }

    private IEnumerator ApiE4PostInfo(string user_id, string student_id, byte[] videoFile)
    {
        WWWForm form = new WWWForm();
        form.AddField("user_id", user_id);
        form.AddField("student_id", student_id);

        form.AddBinaryData("video_file", videoFile);

        UnityWebRequest www = UnityWebRequest.Post(WebServerData.PostUrls[3].ToString(), form);
        www.useHttpContinue = false;

        yield return www.SendWebRequest();

        if (www.error != null)
        {
            Debug.LogError(www.error);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);
        }
        www.Dispose();
        postCoroutine = null;
    }


    private IEnumerator DownloadRequest(Texture2D targetTextrue, string targetURL)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(targetURL);

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            targetTextrue = ((DownloadHandlerTexture)www.downloadHandler).texture;
            fileCount++;
        }
    }
}
