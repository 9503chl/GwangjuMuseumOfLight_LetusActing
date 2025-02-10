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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            CreateUser("adasd");
        }
    }

    public void CreateUser(string playerName)
    {
        postCoroutine = StartCoroutine(ICreateUser(playerName));
    }

    private IEnumerator ICreateUser(string playerName)
    {
        WWWForm form = new WWWForm();
        form.AddField("user_name", playerName);

        UnityWebRequest www = UnityWebRequest.Post(WebServerData.BaseUrl + "/test/", form);
        www.useHttpContinue = false;
        www.downloadHandler = new DownloadHandlerBuffer();
        www.disposeDownloadHandlerOnDispose = true;

        www.timeout = 30;

        yield return www.SendWebRequest();

        if (www.error != null)
        {
            Debug.LogError(www.error);
        }
        else
        {
            string json = www.downloadHandler.text;
            JsonData data = JsonMapper.ToObject(json);
            if (data != null)
            {
                Debug.Log(string.Format("user id = {0}", data["user_id"].ToString()));
                Debug.Log(string.Format("student id = {0}", data["student_id"].ToString()));
            }
        }
        www.Dispose();
        postCoroutine = null;
    }

    public void ApiE1Get(string user_id, string student_id)
    {
        if(getCoroutine == null)
            getCoroutine = StartCoroutine(IApiE1Get(user_id, student_id));
    }

    private IEnumerator IApiE1Get(string user_id, string student_id) 
    {
        WebUri webUri = new WebUri(string.Format("{0}{1}",WebServerData.BaseUrl, WebServerData.GetUrls[0]));
        webUri.AddField("user_id", user_id);
        webUri.AddField("student_id", student_id);

        UnityWebRequest www = UnityWebRequest.Get(webUri.ToString());
        www.useHttpContinue = false;
        www.downloadHandler = new DownloadHandlerBuffer();
        www.disposeDownloadHandlerOnDispose = true;

        www.timeout = 30;

        yield return www.SendWebRequest();

        if (www.error != null)
        {
            Debug.LogError(www.error);
        }
        else
        {
            string json = www.downloadHandler.text;
            JsonData data = JsonMapper.ToObject(json);
            if (data != null)
            {
                //data["user_id"].ToString();
                //data["student_id"].ToString();
                //data["user_name"].ToString();
            }
        }
        www.Dispose();
        getCoroutine = null;
    }

    public void ApiE2Get(string user_id, string student_id)
    {
        if(getCoroutine == null)
            getCoroutine = StartCoroutine(IApiE2Get(user_id, student_id));
    }

    private IEnumerator IApiE2Get(string user_id, string student_id)
    {
        WebUri webUri = new WebUri(string.Format("{0}{1}", WebServerData.BaseUrl, WebServerData.GetUrls[1]));
        webUri.AddField("user_id", user_id); 
        webUri.AddField("student_id", student_id);

        UnityWebRequest www = UnityWebRequest.Get(webUri.ToString());
        www.useHttpContinue = false;
        www.downloadHandler = new DownloadHandlerBuffer();
        www.disposeDownloadHandlerOnDispose = true;

        www.timeout = 30;

        yield return www.SendWebRequest();

        if (www.error != null)
        {
            Debug.LogError(www.error);
        }
        else
        {
            string json = www.downloadHandler.text;
            JsonData data = JsonMapper.ToObject(json);
            if (data != null && data["character_type"].ToString() != string.Empty)
            {
                //성공

                //data["user_id"].ToString();
                //data["student_id"].ToString();
                //data["user_name"].ToString();
                //data["character_type"].ToString();

                yield return StartCoroutine(DownloadRequest(user_id, student_id, WebServerData.data_types[2], string.Empty));

                fileCount = 0;
            }
            else
            {
                //실패
            }
        }
        www.Dispose();
        getCoroutine = null;
    }

    public void ApiE3Get(string user_id, string student_id)
    {
        if(getCoroutine == null)
            getCoroutine = StartCoroutine(IApiE3Get(user_id, student_id));
    }

    private IEnumerator IApiE3Get(string user_id, string student_id)
    {
        WebUri webUri = new WebUri(string.Format("{0}{1}", WebServerData.BaseUrl, WebServerData.GetUrls[2]));
        webUri.AddField("user_id", user_id);
        webUri.AddField("student_id", student_id);

        UnityWebRequest www = UnityWebRequest.Get(webUri.ToString());
        www.useHttpContinue = false;
        www.downloadHandler = new DownloadHandlerBuffer();
        www.disposeDownloadHandlerOnDispose = true;

        www.timeout = 30;

        yield return www.SendWebRequest();

        if (www.error != null)
        {
            Debug.LogError(www.error);
        }
        else
        {
            string json = www.downloadHandler.text;
            JsonData data = JsonMapper.ToObject(json);
            if (data != null && data["friend_name"].ToString() != string.Empty)
            {
                //성공

                WebServerData.userId = data["user_id"].ToString();
                WebServerData.studentId = data["student_id"].ToString();
                WebServerData.userName = data["user_name"].ToString();

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

    public void ApiE4Get(string user_id, string student_id)
    {
        if(getCoroutine == null)
            getCoroutine = StartCoroutine(IApiE4Get(user_id, student_id));
    }

    private int fileCount = 0;

    private IEnumerator IApiE4Get(string user_id, string student_id)
    {
        WebUri webUri = new WebUri(string.Format("{0}{1}", WebServerData.BaseUrl, WebServerData.GetUrls[3]));
        webUri.AddField("user_id", user_id);
        webUri.AddField("student_id", student_id);

        UnityWebRequest www = UnityWebRequest.Get(webUri.ToString());
        www.useHttpContinue = false;
        www.downloadHandler = new DownloadHandlerBuffer();
        www.disposeDownloadHandlerOnDispose = true;

        www.timeout = 30;

        yield return www.SendWebRequest();

        if (www.error != null)
        {
            Debug.LogError(www.error);
        }
        else
        {
            string json = www.downloadHandler.text;
            JsonData data = JsonMapper.ToObject(json);
            if (data != null && data["motion_data_1"].ToString() != string.Empty)
            {
                //성공

                //data["user_id"].ToString();
                //data["student_id"].ToString();
                //data["user_name"].ToString();
                //data["character_type"].ToString();

                //data["friend_name"].ToString();
                //data["villain_name"].ToString();
                //data["scenario_text"].ToString();

                //data["motion_data_1"].ToString();
                //data["motion_data_2"].ToString();
                //data["motion_data_3"].ToString();
                //data["motion_data_4"].ToString();
                //data["motion_data_5"].ToString();
                
                StartCoroutine(DownloadRequest(user_id, student_id, WebServerData.data_types[0], "1"));
                StartCoroutine(DownloadRequest(user_id, student_id, WebServerData.data_types[0], "2"));
                StartCoroutine(DownloadRequest(user_id, student_id, WebServerData.data_types[0], "3"));
                StartCoroutine(DownloadRequest(user_id, student_id, WebServerData.data_types[0], "4"));

                StartCoroutine(DownloadRequest(user_id, student_id, WebServerData.data_types[1], "1"));
                StartCoroutine(DownloadRequest(user_id, student_id, WebServerData.data_types[1], "2"));
                //StartCoroutine(DownloadRequest(user_id, student_id, data_types[1], "3"));
                //StartCoroutine(DownloadRequest(user_id, student_id, data_types[1], "4"));
                //StartCoroutine(DownloadRequest(user_id, student_id, data_types[1], "5"));

                yield return new WaitUntil(() => fileCount == 6);

                fileCount = 0;
            }
            else
            {
                //실패
            }
            www.Dispose();
        }
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

        UnityWebRequest www = UnityWebRequest.Post(string.Format("{0}{1}", WebServerData.BaseUrl, WebServerData.PostUrls[0]), form);
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
        

        UnityWebRequest www = UnityWebRequest.Post(string.Format("{0}{1}", WebServerData.BaseUrl, WebServerData.PostUrls[1]), form);
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

        UnityWebRequest www = UnityWebRequest.Post(string.Format("{0}{1}", WebServerData.BaseUrl, WebServerData.PostUrls[2]), form);
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

        UnityWebRequest www = UnityWebRequest.Post(string.Format("{0}{1}" ,WebServerData.BaseUrl, WebServerData.PostUrls[3]), form);
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


    public void ApiE5Get(string user_id, string student_id)
    {
        if (getCoroutine == null)
            getCoroutine = StartCoroutine(ApiE5GetInfo(user_id, student_id));
    }
    private IEnumerator ApiE5GetInfo(string user_id, string student_id)
    {
        WebUri webUri = new WebUri(string.Format("{0}{1}", WebServerData.BaseUrl, WebServerData.GetUrls[4]));
        webUri.AddField("user_id", user_id);
        webUri.AddField("student_id", student_id);

        UnityWebRequest www = UnityWebRequest.Get(webUri.ToString());
        www.useHttpContinue = false;
        www.downloadHandler = new DownloadHandlerBuffer();
        www.disposeDownloadHandlerOnDispose = true;

        www.timeout = 30;

        yield return www.SendWebRequest();

        if (www.error != null)
        {
            Debug.LogError(www.error);
        }
        else
        {
            string json = www.downloadHandler.text;
            JsonData data = JsonMapper.ToObject(json);
            if (data != null)
            {
                if (data["video_url"].ToString() != string.Empty)
                {
                    //ProjectSettings.user_id = data["user_id"].ToString();
                    //ProjectSettings.user_name = data["user_name"].ToString();
                    //ProjectSettings.student_id = data["student_id"].ToString();
                    //ProjectSettings.scenario_text = data["scenario_text"].ToString();
                    //ProjectSettings.StringSplit();

                    yield return StartCoroutine(DownloadRequest(user_id, student_id, "video_file", string.Empty));

                    fileCount = 0;

                    //BaseManager._TitlePanel.OnSuccess();
                    //yield return new WaitForSeconds(3);
                    //BaseManager.ActiveView = ViewKind.Video;
                }
                else
                {
                    //BaseManager._TitlePanel.OnFail();
                }
            }
        }
        www.Dispose();
        getCoroutine = null;
    }

    private IEnumerator DownloadRequest(string userId, string studentId, string dataType, string dataNumber)
    {
        // URL 생성
        string url = $"{WebServerData.BaseUrl}download/{dataType}";

        if (dataNumber != string.Empty)
        {
            url += $"/{dataNumber}"; // dataNumber가 있으면 경로에 추가
        }

        // Query Parameters 추가
        url += $"?user_id={userId}&student_id={studentId}";

        // 요청 생성
        UnityWebRequest request = UnityWebRequest.Get(url);

        // 요청 보내기
        yield return request.SendWebRequest();

        // 오류 처리
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError($"Error: {request.error}");
        }
        else if (request.responseCode == 422)
        {
            Debug.LogError("Unprocessable Entity: 요청이 잘못되었습니다. 입력값을 확인하세요.");
        }
        else
        {
            // 요청 성공
            Debug.Log("Success: " + request.downloadHandler.text);
            //ProjectSettings.videoDownloadURL = url;
            //ProjectSettings.qrTexture = QrCodeUtility.GenerateQR(url);
            fileCount++;
        }
        request.Dispose();
    }

    private static IEnumerator GetTexture(string targetURL)
    {
        string downloadURL = string.Format("{0}{1}?filename={2}", targetURL);
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(targetURL);

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
            //BaseManager._TitlePanel.OnFail();
        }
        else
        {
            //agentStructs[num].texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
        }
    }
}
