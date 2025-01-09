using LitJson;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class WebServerUtility : MonoBehaviour
{
    public static WebServerUtility Instance;

    [SerializeField] private string targetURL;
    [SerializeField] private string downloadURL;

    private TitlePanel titlePanel;

    private void Awake()
    {
        Instance = FindObjectOfType<WebServerUtility>(true);

        titlePanel = FindObjectOfType<TitlePanel>(true);
    }
    public void ApiGet()
    {
        StartCoroutine(ApiGetInfo());
    }
    IEnumerator ApiGetInfo()
    {
        UnityWebRequest www = UnityWebRequest.Get(Path.Combine(targetURL , ""));//나중에 뒤에 빈칸 채워야함.
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

            //if (data.ContainsKey("user_id"))
            //{
            //    DataSettings.PlayerID = data["user_id"].ToString();
            //}
            if (data.ContainsKey("user_name"))
            {
                DataSettings.PlayerName = data["user_name"].ToString();
            }

            if(titlePanel != null)
            {
                titlePanel.OnSuccess();
            }

             www.Dispose();
        }
    }
    public void ApiPost()//테스트 필요.
    {
        StartCoroutine(ApiPostInfo());
    }
    IEnumerator ApiPostInfo()
    {
        WWWForm wWWForm = new WWWForm();

        wWWForm.AddField("user_id", DataSettings.PlayerID);

        for (int i = 0; i < 5; i++) 
        {
            string json = JsonUtility.ToJson(DataSettings.dataArray[i], true);
            wWWForm.AddField(string.Format("motion_data_{0}", i + 1), json);
        }

        UnityWebRequest www = UnityWebRequest.Post(targetURL, wWWForm);
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
            if (data.ContainsKey("status"))
            {
                Debug.Log(data["status"]);
            }
            BaseManager.ActiveView = ViewKind.Finish;
            www.Dispose();
        }
    }


    public void GetTextureByURL()
    {
        StartCoroutine(GetTexture());
    }
    IEnumerator GetTexture()
    {
        string downloadURL = this.downloadURL;
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(downloadURL);
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {

        }
    }
}
