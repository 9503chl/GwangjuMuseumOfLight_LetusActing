using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using WebSocketSharp;

[DisallowMultipleComponent]
public class SyncWebSocket : MonoBehaviour
{
    public string WebSocketURL = "ws://127.0.0.1:8280";
    [Range(0f, 60f)]
    public float ReconnectInterval = 10f;
    public bool ConnectOnEnable = true;

    public event Action OnConnect;
    public event Action OnDisconnect;
    public event Action<byte[]> OnReceive;
    public event Action<string> OnReceiveText;
    public event Action<string, bool> OnReceiveFile;
    public event Action<string, bool> OnSendFile;

    [NonSerialized]
    private WebSocket client;

    [NonSerialized]
    private bool callOnConnect;

    [NonSerialized]
    private bool callOnDisconnect;

    private enum EventType
    {
        Receive,
        ReceiveText,
        ReceiveFile,
        SendFile
    }

    private class EventData
    {
        public EventType Type;
        public byte[] Data;
        public string Text;
        public bool Completed;

        public EventData(EventType type)
        {
            Type = type;
        }

        public EventData(EventType type, byte[] data) : this(type)
        {
            Data = data;
        }

        public EventData(EventType type, string text) : this(type)
        {
            Text = text;
        }

        public EventData(EventType type, string path, bool completed) : this(type)
        {
            Text = path;
            Completed = completed;
        }
    }

    private readonly Queue<EventData> eventQueue = new Queue<EventData>();

    [NonSerialized]
    private string filePath;

    [NonSerialized]
    private long fileSize;

    [NonSerialized]
    private Stream fileStream;

    [NonSerialized]
    private float delayedTime;

    [NonSerialized]
    private Coroutine checkingRoutine;

    [NonSerialized]
    private bool connecting;
    public bool Connecting
    {
        get { return connecting; }
    }

    public bool Connected
    {
        get { return client != null && client.ReadyState == WebSocketState.Open; }
    }

    [NonSerialized]
    private string remoteAddress;
    public string RemoteAddress
    {
        get { return (remoteAddress != null) ? remoteAddress : string.Empty; }
    }

    [NonSerialized]
    private string savePath;
    public string SavePath
    {
        get { return savePath; }
        set { savePath = value; }
    }

    [NonSerialized]
    private bool isTransferFile;
    public bool IsTransferFile
    {
        get { return isTransferFile; }
    }

    private void Awake()
    {
        savePath = Application.persistentDataPath;
    }

    private void OnEnable()
    {
        if (ConnectOnEnable)
        {
            Connect();
        }
    }

    private void OnDisable()
    {
        if (client != null)
        {
            client.Close();
        }
        if (!isAppQuitting && callOnDisconnect)
        {
            callOnDisconnect = false;
            if (OnDisconnect != null)
            {
                OnDisconnect.Invoke();
            }
        }
    }

    private void OnDestroy()
    {
        Close();
    }

    [NonSerialized]
    private bool isAppQuitting;

    private void OnApplicationQuit()
    {
        isAppQuitting = true;
    }

    public void Connect()
    {
        if (client != null)
        {
            Debug.LogWarning("WebSocket : Already connected or connecting");
            return;
        }
        if (checkingRoutine != null)
        {
            StopCoroutine(checkingRoutine);
            checkingRoutine = null;
        }
        connecting = true;
        try
        {
            client = new WebSocket(WebSocketURL);
            client.WaitTime = TimeSpan.FromSeconds(10);
            client.EnableRedirection = true;
            client.OnOpen += Client_OnOpen;
            client.OnClose += Client_OnClose;
            client.OnMessage += Client_OnMessage;
            client.ConnectAsync();
            checkingRoutine = StartCoroutine(Checking());
        }
        catch (Exception ex)
        {
            Debug.LogError(string.Format("WebSocket : {0}", ex.Message));
            Disconnect();
        }
    }

    public void Close()
    {
        Disconnect();
        if (checkingRoutine != null)
        {
            StopCoroutine(checkingRoutine);
            checkingRoutine = null;
        }
        callOnConnect = false;
        callOnDisconnect = false;
    }

    private void Disconnect()
    {
        if (client != null)
        {
            client.OnOpen -= Client_OnOpen;
            client.OnClose -= Client_OnClose;
            client.OnMessage -= Client_OnMessage;
            client.Close();
            client = null;
        }
        if (fileStream != null)
        {
            fileStream.Close();
            fileStream = null;
        }
        delayedTime = 0f;
        connecting = false;
        remoteAddress = null;
        isTransferFile = false;
        eventQueue.Clear();
    }

    private IEnumerator Checking()
    {
        yield return null;

        while (enabled)
        {
            if (client == null && ReconnectInterval > 0f)
            {
                if (delayedTime < ReconnectInterval)
                {
                    delayedTime += Time.unscaledDeltaTime;
                }
                else
                {
                    delayedTime = 0f;
                    Connect();
                }
            }
            if (callOnConnect)
            {
                callOnConnect = false;
                if (OnConnect != null)
                {
                    OnConnect.Invoke();
                }
            }
            if (callOnDisconnect)
            {
                callOnDisconnect = false;
                if (OnDisconnect != null)
                {
                    OnDisconnect.Invoke();
                }
            }
            if (client != null && client.ReadyState == WebSocketState.Open)
            {
                ProcessReceivedBuffer();
            }
            yield return null;
        }
    }

    private void Client_OnOpen(object sender, EventArgs e)
    {
        connecting = false;
        remoteAddress = client.Url.ToString();
        Debug.Log(string.Format("WebSocket : Connected to {0}", RemoteAddress));
        callOnConnect = true;
    }

    private void Client_OnClose(object sender, CloseEventArgs e)
    {
        if (connecting)
        {
            Debug.LogWarning(string.Format("WebSocket : {0}", e.Reason));
        }
        else
        {
            Debug.Log(string.Format("WebSocket : Disconnected from {0}", RemoteAddress));
            callOnDisconnect = true;
        }
        Disconnect();
    }

    private void Client_OnMessage(object sender, MessageEventArgs e)
    {
        if (e.IsText)
        {
            if (e.Data.StartsWith("\bFILE\b"))
            {
                string[] parts = e.Data.Split('*');
                if (parts.Length == 3 && parts[1].Length > 0 && parts[2].Length > 0)
                {
                    filePath = string.Format("{0}/{1}", savePath, parts[1].Trim());
                    fileSize = Convert.ToInt64(parts[2].Trim());
                    isTransferFile = true;
                    try
                    {
                        string saveFilePath = filePath;
                        string dir = Path.GetDirectoryName(filePath);
                        if (!Directory.Exists(dir))
                        {
                            try
                            {
                                Directory.CreateDirectory(dir);
                            }
                            catch (Exception)
                            {
                            }
                        }
                        string tempPath;
                        for (int i = 0; i < 10000; i++)
                        {
                            tempPath = string.Format("{0}.[{1}]", saveFilePath, i);
                            if (!File.Exists(tempPath))
                            {
                                try
                                {
                                    fileStream = new FileStream(tempPath, FileMode.CreateNew);
                                    filePath = tempPath;
                                    break;
                                }
                                catch (Exception)
                                {
                                }
                            }
                        }
                        if (fileStream == null)
                        {
                            filePath = string.Format("{0}.[{1}]", saveFilePath, Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
                            fileStream = new FileStream(filePath, FileMode.CreateNew);
                        }
                        client.SendAsync("\bFILE\b*ACCEPT", null);
                        eventQueue.Enqueue(new EventData(EventType.ReceiveFile, saveFilePath, false));
                    }
                    catch (Exception ex)
                    {
                        isTransferFile = false;
                        Debug.LogError(string.Format("WebSocket : {0}", ex.Message));
                        client.SendAsync("\bFILE\b*REJECT", null);
                    }
                }
                else if (parts.Length == 2 && parts[1].Length > 0)
                {
                    string answer = parts[1].Trim();
                    if (string.Compare(answer, "ACCEPT", true) == 0)
                    {
                        if (isTransferFile)
                        {
                            client.SendAsync(new FileInfo(filePath), null);
                        }
                    }
                    else if (string.Compare(answer, "FINISH", true) == 0)
                    {
                        isTransferFile = false;
                        eventQueue.Enqueue(new EventData(EventType.SendFile, filePath, true));
                    }
                    else
                    {
                        isTransferFile = false;
                        eventQueue.Enqueue(new EventData(EventType.SendFile, filePath, false));
                    }
                }
            }
            else
            {
                eventQueue.Enqueue(new EventData(EventType.ReceiveText, e.Data));
            }
        }
        else if (e.RawData.Length > 0)
        {
            if (isTransferFile)
            {
                if (fileStream == null)
                {
                    filePath = null;
                    fileStream = new MemoryStream();
                }
                fileStream.Write(e.RawData, 0, e.RawData.Length);
                if (fileStream.Length >= fileSize)
                {
                    fileStream.SetLength(fileSize);
                    fileStream.Flush();
                    fileStream.Close();
                    fileStream = null;
                    isTransferFile = false;
                    try
                    {
                        string tempPath = Path.ChangeExtension(filePath, null);
                        try
                        {
                            File.Move(filePath, tempPath);
                            filePath = tempPath;
                        }
                        catch (Exception)
                        {
                            string path = Path.GetDirectoryName(tempPath).Replace('\\', '/');
                            string name = Path.GetFileNameWithoutExtension(tempPath);
                            string ext = Path.GetExtension(tempPath);
                            for (int i = 0; i < 10000; i++)
                            {
                                tempPath = string.Format("{0}/{1} ({2}){3}", path, name, i + 1, ext);
                                if (!File.Exists(tempPath))
                                {
                                    try
                                    {
                                        File.Move(filePath, tempPath);
                                        filePath = tempPath;
                                        break;
                                    }
                                    catch (Exception)
                                    {
                                    }
                                }
                            }
                            if (string.Compare(filePath, tempPath) != 0)
                            {
                                tempPath = string.Format("{0}/{1} ({2}){3}", path, name, Path.GetFileNameWithoutExtension(Path.GetRandomFileName()), ext);
                                File.Move(filePath, tempPath);
                                filePath = tempPath;
                            }
                        }
                        client.SendAsync("\bFILE\b*FINISH", null);
                        eventQueue.Enqueue(new EventData(EventType.ReceiveFile, filePath, true));
                    }
                    catch (Exception)
                    {
                        client.SendAsync("\bFILE\b*FAILED", null);
                    }
                }
            }
            else
            {
                eventQueue.Enqueue(new EventData(EventType.Receive, e.RawData));
            }
        }
    }

    private void ProcessReceivedBuffer()
    {
        if (eventQueue.Count > 0)
        {
            EventData eventData = eventQueue.Dequeue();
            switch (eventData.Type)
            {
                case EventType.Receive:
                    if (OnReceive != null)
                    {
                        OnReceive.Invoke(eventData.Data);
                    }
                    break;
                case EventType.ReceiveText:
                    if (OnReceiveText != null)
                    {
                        OnReceiveText.Invoke(eventData.Text);
                    }
                    break;
                case EventType.ReceiveFile:
                    if (OnReceiveFile != null)
                    {
                        OnReceiveFile.Invoke(eventData.Text, eventData.Completed);
                    }
                    break;
                case EventType.SendFile:
                    if (OnSendFile != null)
                    {
                        OnSendFile.Invoke(eventData.Text, eventData.Completed);
                    }
                    break;
            }
        }
    }

    public void Send(byte[] data)
    {
        if (client != null && client.ReadyState == WebSocketState.Open)
        {
            if (isTransferFile)
            {
                Debug.LogWarning("WebSocket : Cannot send anything while transfering file");
                return;
            }
            client.SendAsync(data, null);
        }
    }

    public void SendText(string text)
    {
        if (client != null && client.ReadyState == WebSocketState.Open)
        {
            if (isTransferFile)
            {
                Debug.LogWarning("WebSocket : Cannot send anything while transfering file");
                return;
            }
            client.SendAsync(text, null);
        }
    }

    public void SendFile(string path, string fileName = null)
    {
        if (client != null && client.ReadyState == WebSocketState.Open)
        {
            if (isTransferFile)
            {
                Debug.LogWarning("WebSocket : Cannot send anything while transfering file");
                return;
            }
            FileInfo fileInfo = new FileInfo(path);
            if (fileInfo.Exists)
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    fileName = fileInfo.Name;
                }
                filePath = path;
                fileSize = fileInfo.Length;
                isTransferFile = true;
                client.SendAsync(string.Format("\bFILE\b*{0}*{1}", fileName, fileInfo.Length), null);
            }
            else
            {
                Debug.LogWarning("WebSocket : Cannot find file to send");
            }
        }
    }
}
