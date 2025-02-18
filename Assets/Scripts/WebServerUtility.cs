using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using LitJson;

public class WebServerUtility
{
    private const string baseUrl = "http://101.101.219.13:8080/ArtStationExternalAPI/jux";

    private static readonly string[] getUrls = { "E1Get.do", "E1Get.do", "E1Get.do", "E1Get.do", "E1Get.do" };
    private static readonly string[] postUrls = { "E1Post.do", "E2Post.do", "E3Post.do", "E4Post.do", "" };
    private const int getTimeout = 30;
    private const int postTimeout = 60;

    public static string user_info;
    public static string userId;
    public static string studentId;

    public static WebServerData E1Data = new WebServerData();
    public static WebServerData E2Data = new WebServerData();
    public static WebServerData E3Data = new WebServerData();
    public static WebServerData E4Data = new WebServerData();
    public static WebServerData E5Data = new WebServerData();

    private static Texture2D[] textures = new Texture2D[5];
    private static string[] texts = new string[5];

    public static BodyDataList[] dataArray = new BodyDataList[5] { new BodyDataList() , new BodyDataList(), new BodyDataList(), new BodyDataList(), new BodyDataList() } ;

    public static int captureIndex = 0;

    public static void Clear()
    {
        E3Data.Clear();
        for (int i = 0; i < dataArray.Length; i++)
        {
            dataArray[i].Clear();
        }
    }

    private static string JsonDataToString(JsonData data, string fieldName)
    {
        if (data != null && fieldName != null)
        {
            if (data.ContainsKey(fieldName) && data[fieldName] != null)
            {
                return data[fieldName].ToString();
            }
        }
        return null;
    }

    public static IEnumerator E1Get(string user_info)
    {
        int index = 0;
        WebServerData result = E1Data;
        result.Clear();
        WWWForm form = new WWWForm();
        form.AddField("user_info", user_info);
        UnityWebRequest www = UnityWebRequest.Post(string.Format("{0}/{1}", baseUrl, getUrls[index]), form);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.disposeDownloadHandlerOnDispose = true;
        www.timeout = getTimeout;
        yield return www.SendWebRequest();

        if (www.error != null)
        {
            Debug.LogError(www.error);
        }
        else
        {
            string json = www.downloadHandler.text;
            JsonData data = JsonMapper.ToObject(json);
            result.status = JsonDataToString(data, "status");
            if (data != null && data.ContainsKey("data"))
            {
                result.userId = JsonDataToString(data["data"], "user_id");
                result.studentId = JsonDataToString(data["data"], "student_id");
                result.userName = JsonDataToString(data["data"], "user_name");
            }
            result.message = JsonDataToString(data, "message");
            result.errorCode = JsonDataToString(data, "error_code");
        }
        www.Dispose();
    }

    public static IEnumerator E2Get(string user_info, MonoBehaviour monoBehaviour = null)
    {
        int index = 1;
        WebServerData result = E2Data;
        result.Clear();
        WWWForm form = new WWWForm();
        form.AddField("user_info", user_info);
        UnityWebRequest www = UnityWebRequest.Post(string.Format("{0}/{1}", baseUrl, getUrls[index]), form);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.disposeDownloadHandlerOnDispose = true;
        www.timeout = getTimeout;
        yield return www.SendWebRequest();

        if (www.error != null)
        {
            Debug.LogError(www.error);
        }
        else
        {
            string json = www.downloadHandler.text;
            JsonData data = JsonMapper.ToObject(json);
            result.status = JsonDataToString(data, "status");
            if (data != null && data.ContainsKey("data"))
            {
                result.userId = JsonDataToString(data["data"], "user_id");
                result.studentId = JsonDataToString(data["data"], "student_id");
                result.userName = JsonDataToString(data["data"], "user_name");
                result.characterType = JsonDataToString(data["data"], "character_type");
                result.screenshotImageUrl = JsonDataToString(data["data"], "screenshot_image");
            }
            result.message = JsonDataToString(data, "message");
            result.errorCode = JsonDataToString(data, "error_code");
        }
        www.Dispose();
    }

    public static IEnumerator E2Download(MonoBehaviour monoBehaviour)
    {
        int index = 1;
        WebServerData result = E2Data;
        if (monoBehaviour != null)
        {
            yield return monoBehaviour.StartCoroutine(DownloadTexture(result.screenshotImageUrl, index));
            result.screenshotImage = textures[index];
        }
    }

    public static IEnumerator E3Get(string user_info)
    {
        int index = 2;
        WebServerData result = E3Data;
        result.Clear();
        WWWForm form = new WWWForm();
        form.AddField("user_info", user_info);
        UnityWebRequest www = UnityWebRequest.Post(string.Format("{0}/{1}", baseUrl, getUrls[index]), form);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.disposeDownloadHandlerOnDispose = true;
        www.timeout = getTimeout;
        yield return www.SendWebRequest();

        if (www.error != null)
        {
            Debug.LogError(www.error);
        }
        else
        {
            string json = www.downloadHandler.text;
            JsonData data = JsonMapper.ToObject(json);
            result.status = JsonDataToString(data, "status");
            if (data != null && data.ContainsKey("data"))
            {
                result.userId = JsonDataToString(data["data"], "user_id");
                result.studentId = JsonDataToString(data["data"], "student_id");
                result.userName = JsonDataToString(data["data"], "user_name");
                result.characterType = JsonDataToString(data["data"], "character_type");
                result.materialTexture1Url = JsonDataToString(data["data"], "material_texture_1");
                result.materialTexture2Url = JsonDataToString(data["data"], "material_texture_2");
                result.facialExpression1Url = JsonDataToString(data["data"], "facial_expression_1");
            }
            result.message = JsonDataToString(data, "message");
            result.errorCode = JsonDataToString(data, "error_code");
        }
        www.Dispose();
    }

    public static IEnumerator E3Download(MonoBehaviour monoBehaviour)
    {
        int index = 2;
        WebServerData result = E3Data;
        if (monoBehaviour != null)
        {
            yield return monoBehaviour.StartCoroutine(DownloadTexture(result.materialTexture1Url, index));
            result.materialTexture1 = textures[index];
            yield return monoBehaviour.StartCoroutine(DownloadTexture(result.materialTexture2Url, index));
            result.materialTexture2 = textures[index];
            yield return monoBehaviour.StartCoroutine(DownloadTexture(result.facialExpression1Url, index));
            result.facialExpression1 = textures[index];
        }
    }

    public static IEnumerator E4Get(string user_info)
    {
        int index = 3;
        WebServerData result = E4Data;
        result.Clear();
        WWWForm form = new WWWForm();
        form.AddField("user_info", user_info);
        UnityWebRequest www = UnityWebRequest.Post(string.Format("{0}/{1}", baseUrl, getUrls[index]), form);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.disposeDownloadHandlerOnDispose = true;
        www.timeout = getTimeout;
        yield return www.SendWebRequest();

        if (www.error != null)
        {
            Debug.LogError(www.error);
        }
        else
        {
            string json = www.downloadHandler.text;
            JsonData data = JsonMapper.ToObject(json);
            result.status = JsonDataToString(data, "status");
            if (data != null && data.ContainsKey("data"))
            {
                result.userId = JsonDataToString(data["data"], "user_id");
                result.studentId = JsonDataToString(data["data"], "student_id");
                result.userName = JsonDataToString(data["data"], "user_name");
                result.characterType = JsonDataToString(data["data"], "character_type");
                result.facialExpression1Url = JsonDataToString(data["data"], "facial_expression_1");
                result.facialExpression2Url = JsonDataToString(data["data"], "facial_expression_2");
                result.facialExpression3Url = JsonDataToString(data["data"], "facial_expression_3");
                result.facialExpression4Url = JsonDataToString(data["data"], "facial_expression_4");
                result.materialTexture1Url = JsonDataToString(data["data"], "material_texture_1");
                result.materialTexture2Url = JsonDataToString(data["data"], "material_texture_2");
                result.friendName = JsonDataToString(data["data"], "friend_name");
                result.villainName = JsonDataToString(data["data"], "villain_name");
                result.bgName = JsonDataToString(data["data"], "bg_name");
                result.scenarioText = JsonDataToString(data["data"], "scenario_text");
                result.motionData1Url = JsonDataToString(data["data"], "motion_data_1");
                result.motionData2Url = JsonDataToString(data["data"], "motion_data_2");
                result.motionData3Url = JsonDataToString(data["data"], "motion_data_3");
                result.motionData4Url = JsonDataToString(data["data"], "motion_data_4");
                result.motionData5Url = JsonDataToString(data["data"], "motion_data_5");
            }
            result.message = JsonDataToString(data, "message");
            result.errorCode = JsonDataToString(data, "error_code");
        }
        www.Dispose();
    }

    public static IEnumerator E4Download(MonoBehaviour monoBehaviour)
    {
        int index = 3;
        WebServerData result = E4Data;
        if (monoBehaviour != null)
        {
            yield return monoBehaviour.StartCoroutine(DownloadTexture(result.facialExpression1Url, index));
            result.facialExpression1 = textures[index];
            yield return monoBehaviour.StartCoroutine(DownloadTexture(result.facialExpression2Url, index));
            result.facialExpression2 = textures[index];
            yield return monoBehaviour.StartCoroutine(DownloadTexture(result.facialExpression3Url, index));
            result.facialExpression3 = textures[index];
            yield return monoBehaviour.StartCoroutine(DownloadTexture(result.facialExpression4Url, index));
            result.facialExpression4 = textures[index];
            yield return monoBehaviour.StartCoroutine(DownloadTexture(result.materialTexture1Url, index));
            result.materialTexture1 = textures[index];
            yield return monoBehaviour.StartCoroutine(DownloadTexture(result.materialTexture2Url, index));
            result.materialTexture2 = textures[index];
            yield return monoBehaviour.StartCoroutine(DownloadText(result.motionData1Url, index));
            result.motionData1 = texts[index];
            yield return monoBehaviour.StartCoroutine(DownloadText(result.motionData2Url, index));
            result.motionData2 = texts[index];
            yield return monoBehaviour.StartCoroutine(DownloadText(result.motionData3Url, index));
            result.motionData3 = texts[index];
            yield return monoBehaviour.StartCoroutine(DownloadText(result.motionData4Url, index));
            result.motionData4 = texts[index];
            yield return monoBehaviour.StartCoroutine(DownloadText(result.motionData5Url, index));
            result.motionData5 = texts[index];
        }
    }

    public static IEnumerator E5Get(string user_info)
    {
        int index = 4;
        WebServerData result = E5Data;
        result.Clear();
        WWWForm form = new WWWForm();
        form.AddField("user_info", user_info);
        UnityWebRequest www = UnityWebRequest.Post(string.Format("{0}/{1}", baseUrl, getUrls[index]), form);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.disposeDownloadHandlerOnDispose = true;
        www.timeout = getTimeout;
        yield return www.SendWebRequest();

        if (www.error != null)
        {
            Debug.LogError(www.error);
        }
        else
        {
            string json = www.downloadHandler.text;
            JsonData data = JsonMapper.ToObject(json);
            result.status = JsonDataToString(data, "status");
            if (data != null && data.ContainsKey("data"))
            {
                result.userId = JsonDataToString(data["data"], "user_id");
                result.studentId = JsonDataToString(data["data"], "student_id");
                result.userName = JsonDataToString(data["data"], "user_name");
                result.scenarioText = JsonDataToString(data["data"], "scenario_text");
                result.videoFileUrl = JsonDataToString(data["data"], "video_file");
                result.videoThumbnailUrl = JsonDataToString(data["data"], "video_thumbnail");
            }
            result.message = JsonDataToString(data, "message");
            result.errorCode = JsonDataToString(data, "error_code");
        }
        www.Dispose();
    }

    public static IEnumerator E5Download(MonoBehaviour monoBehaviour)
    {
        int index = 4;
        WebServerData result = E5Data;
        if (monoBehaviour != null)
        {
            yield return monoBehaviour.StartCoroutine(DownloadTexture(result.videoThumbnailUrl, index));
            result.videoThumbnail = textures[index];
        }
    }

    public static IEnumerator E1Post(string user_info, string[] expressionFiles, string[] textureFiles)
    {
        int index = 0;
        WWWForm form = new WWWForm();
        form.AddField("user_info", user_info);
        for (int i = 0; i < expressionFiles.Length; i++)
        {
            form.AddBinaryData(string.Format("facial_expression_{0}", i + 1), File.ReadAllBytes(expressionFiles[i]));
        }
        for (int i = 0; i < textureFiles.Length; i++)
        {
            form.AddBinaryData(string.Format("material_texture_{0}", i + 1), File.ReadAllBytes(textureFiles[i]));
        }
        UnityWebRequest www = UnityWebRequest.Post(string.Format("{0}/{1}", baseUrl, postUrls[index]), form);
        www.timeout = postTimeout;
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
    }

    public static IEnumerator E2Post(string user_info, string friend_name, string villain_name, string bg_name, string scenario_text)
    {
        int index = 1;
        WWWForm form = new WWWForm();
        form.AddField("user_info", user_info);
        form.AddField("friend_name", friend_name);
        form.AddField("villain_name", villain_name);
        form.AddField("bg_name", bg_name);
        form.AddField("scenario_text", scenario_text);
        UnityWebRequest www = UnityWebRequest.Post(string.Format("{0}/{1}", baseUrl, postUrls[index]), form);
        www.timeout = postTimeout;
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
    }

    //public static IEnumerator E3Post(string user_info, string[] motionDataFiles)
    //{
    //    int index = 2;
    //    WWWForm form = new WWWForm();
    //    form.AddField("user_info", user_info);
    //    for (int i = 0; i < motionDataFiles.Length; i++)
    //    {
    //        form.AddBinaryData(string.Format("motion_data_{0}", i + 1), File.ReadAllBytes(motionDataFiles[i]));
    //    }
    //    UnityWebRequest www = UnityWebRequest.Post(string.Format("{0}/{1}", baseUrl, postUrls[index]), form);
    //    www.timeout = postTimeout;
    //    yield return www.SendWebRequest();

    //    if (www.error != null)
    //    {
    //        Debug.LogError(www.error);
    //    }
    //    else
    //    {
    //        Debug.Log(www.downloadHandler.text);
    //    }
    //    www.Dispose();
    //}

    public static IEnumerator E3Post(string user_info)
    {
        int index = 2;
        WWWForm form = new WWWForm();
        form.AddField("user_info", user_info);

        for (int i = 0; i < dataArray.Length; i++)
        {
            form.AddBinaryData(string.Format("motion_data_{0}", i + 1), Encoding.ASCII.GetBytes(dataArray[i].json), string.Format("motion_data_{0}.json", i + 1));
        }
        UnityWebRequest www = UnityWebRequest.Post(string.Format("{0}/{1}", baseUrl, postUrls[index]), form);
        www.timeout = postTimeout;
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
    }

    public static IEnumerator E4Post(string user_info, string video_file, string video_thumbnail)
    {
        int index = 3;
        WWWForm form = new WWWForm();
        form.AddField("user_info", user_info);
        form.AddBinaryData("video_file", File.ReadAllBytes(video_file));
        form.AddBinaryData("video_thumbnail", File.ReadAllBytes(video_thumbnail));

        UnityWebRequest www = UnityWebRequest.Post(string.Format("{0}/{1}", baseUrl, postUrls[index]), form);
        www.timeout = postTimeout;
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
    }

    private static IEnumerator DownloadTexture(string url, int index)
    {
        if (index >= 0 && index < textures.Length)
        {
            textures[index] = null;
        }
        else
        {
            textures[0] = null;
        }
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        www.SetRequestHeader("Cache-Control", "no-cache, no-store, max-age=0");
        www.SetRequestHeader("Pragma", "no-cache");
        www.timeout = getTimeout;
        www.disposeDownloadHandlerOnDispose = true;
        yield return www.SendWebRequest();

        if (www.error != null)
        {
            Debug.LogError(www.error);
        }
        else
        {
            if (index >= 0 && index < textures.Length)
            {
                textures[index] = DownloadHandlerTexture.GetContent(www);
            }
            else
            {
                textures[0] = DownloadHandlerTexture.GetContent(www);
            }
        }
        www.Dispose();
    }

    private static IEnumerator DownloadText(string url, int index)
    {
        if (index >= 0 && index < texts.Length)
        {
            texts[index] = null;
        }
        else
        {
            texts[0] = null;
        }
        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("Cache-Control", "no-cache, no-store, max-age=0");
        www.SetRequestHeader("Pragma", "no-cache");
        www.timeout = getTimeout;
        www.downloadHandler = new DownloadHandlerBuffer();
        www.disposeDownloadHandlerOnDispose = true;
        yield return www.SendWebRequest();

        if (www.error != null)
        {
            Debug.LogError(www.error);
        }
        else
        {
            if (index >= 0 && index < texts.Length)
            {
                texts[index] = www.downloadHandler.text;
            }
            else
            {
                texts[0] = www.downloadHandler.text;
            }
        }
        www.Dispose();
    }
}
