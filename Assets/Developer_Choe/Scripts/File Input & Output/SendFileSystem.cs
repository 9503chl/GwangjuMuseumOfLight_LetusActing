using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.UI;

public class SendFileSystem : MonoBehaviour
{
    public static SendFileSystem Instacne;

    private SyncWebSocketClient webSocketFileClient;

    public string FilePath;
    public string SaveName;

    public SendFileSystem(string filePath, string saveName = null)
    {
        FilePath = filePath;
        if (string.IsNullOrEmpty(saveName))
        {
            SaveName = Path.GetFileName(filePath);
        }
        else
        {
            SaveName = saveName;
        }
    }
    private static readonly Queue<SendFileSystem> sendFileQueue = new Queue<SendFileSystem>();
    private bool isSendingFile = false;

    private void Start()
    {
        StartCoroutine(CheckSendFile());
    }
    private void Awake()
    {
        Instacne = this;

        webSocketFileClient = new SyncWebSocketClient();

        if (webSocketFileClient != null)
        {
            webSocketFileClient.OnConnect += WebSocketFileClient_OnConnect;
            webSocketFileClient.OnDisconnect += WebSocketFileClient_OnDisconnect;
            webSocketFileClient.OnReceiveText += WebSocketFileClient_OnReceiveText;
            webSocketFileClient.WebSocketURL = ProjectSettings.WebSocketFileUrl;
            webSocketFileClient.Connect();
        }
    }

    private void WebSocketFileClient_OnConnect()
    {
        isSendingFile = false;
    }

    private void WebSocketFileClient_OnDisconnect()
    {

    }

    private void WebSocketFileClient_OnReceiveText(string text)
    {
        isSendingFile = false;
        Debug.Log("Send file result : " + text);
    }
    private IEnumerator CheckSendFile()
    {
        while (isActiveAndEnabled)
        {
            if (webSocketFileClient != null && webSocketFileClient.Connected && sendFileQueue.Count > 0)
            {
                SendFileSystem item = sendFileQueue.Dequeue();
                isSendingFile = true;
                webSocketFileClient.SendFile(item.FilePath, item.SaveName);
                while (isSendingFile)
                {
                    yield return null;
                }
            }
            yield return null;
        }
    }

    public static void AddToSendFileQueue(string path, string fileName)
    {
        if (File.Exists(path))
        {
            sendFileQueue.Enqueue(new SendFileSystem(path, fileName));
        }
    }
    public static void SendFile(string filePath, string saveName)
    {
        string fileGuid = Guid.NewGuid().ToString();
        {
            Texture2D texture = null;
                                //FaceMat.mainTexture as Texture2D;
            //Texture2D texture = Resources.Load<Texture2D>("Face_001");
            string path = string.Format("{0}/{1}.png", Application.persistentDataPath, fileGuid);
            File.WriteAllBytes(path, texture.EncodeToPNG());
            AddToSendFileQueue(path, string.Format("{0}/{1}.png", "Temp", fileGuid));
        }
    }
}
