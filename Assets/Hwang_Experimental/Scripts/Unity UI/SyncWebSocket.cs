using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using WebSocketSharp;

[DisallowMultipleComponent]
public class SyncWebSocket : MonoBehaviour
{
    public string WebSocketURL = "ws://127.0.0.1:8280";
    [Range(0f, 60f)]
    public float ReconnectInterval = 10f;
    public bool ConnectOnEnable = true;

    [NonSerialized]
    private string savePath;
    public string SavePath
    {
        get { return savePath; }
        set { savePath = value; }
    }

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
    private UriBuilder remoteUri;
    public string RemoteIP
    {
        get { return (remoteUri != null) ? remoteUri.ToString() : string.Empty; }
    }

    [NonSerialized]
    private string remoteAddress;
    public string RemoteAddress
    {
        get { return (remoteAddress != null) ? remoteAddress : string.Empty; }
    }

    [NonSerialized]
    private bool isTransferFile;
    public bool IsTransferFile
    {
        get { return isTransferFile; }
    }

    private void Awake()
    {
        if (string.IsNullOrEmpty(savePath))
        {
            savePath = Application.persistentDataPath;
        }
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
        if (client != null || connecting)
        {
            Debug.LogWarning("WebSocket : Already connected or connecting");
            return;
        }
        if (checkingRoutine != null)
        {
            StopCoroutine(checkingRoutine);
            checkingRoutine = null;
        }
        string scheme = null;
        string host = null;
        int portNumber = 8280;
        string path = null;
        bool modified = false;
        if (!string.IsNullOrEmpty(WebSocketURL))
        {
            int p1 = WebSocketURL.IndexOf("://");
            if (p1 == -1)
            {
                p1 = -3;
            }
            else
            {
                scheme = WebSocketURL.Substring(0, p1);
            }
            int p2 = WebSocketURL.IndexOf(':', p1 + 3);
            if (p2 == -1)
            {
                p2 = WebSocketURL.IndexOf("/", p1 + 3);
                if (p2 == -1)
                {
                    p2 = WebSocketURL.Length;
                }
                else
                {
                    path = WebSocketURL.Substring(p2 + 1, WebSocketURL.Length - p2 - 1);
                }
            }
            host = WebSocketURL.Substring(p1 + 3, p2 - p1 - 3);
            if (p2 < WebSocketURL.Length)
            {
                int p3 = WebSocketURL.IndexOf("/", p2 + 1);
                if (p3 == -1)
                {
                    p3 = WebSocketURL.Length;
                }
                else
                {
                    path = WebSocketURL.Substring(p3 + 1, WebSocketURL.Length - p3 - 1);
                }
                try
                {
                    portNumber = Convert.ToInt32(WebSocketURL.Substring(p2 + 1, p3 - p2 - 1));
                }
                catch (Exception)
                {
                    modified = true;
                }
            }
        }
        if (string.IsNullOrEmpty(scheme))
        {
            scheme = "ws";
            modified = true;
        }
        if (string.IsNullOrEmpty(host))
        {
            host = "127.0.0.1";
            modified = true;
        }
        remoteUri = new UriBuilder(scheme, host, portNumber, path);
        if (modified)
        {
            WebSocketURL = remoteUri.ToString();
        }
        connecting = true;
        checkingRoutine = StartCoroutine(Checking());
        IPAddress hostIPAddress;
        if (IPAddress.TryParse(host, out hostIPAddress))
        {
            remoteAddress = WebSocketURL;
            ConnectAsync();
        }
        else
        {
            Dns.BeginGetHostAddresses(host, delegate (IAsyncResult ar)
            {
                try
                {
                    IPAddress[] ipAddresses = Dns.EndGetHostAddresses(ar);
                    foreach (IPAddress ipAddress in ipAddresses)
                    {
                        if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                        {
                            remoteUri.Host = ipAddress.ToString();
                            remoteAddress = WebSocketURL;
                            ConnectAsync();
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError(string.Format("WebSocket : {0}", ex.Message));
                    Disconnect();
                }
            }, null);
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
            bool connected = client.ReadyState == WebSocketState.Open;
            client.OnOpen -= Client_OnOpen;
            client.OnClose -= Client_OnClose;
            client.OnMessage -= Client_OnMessage;
            client.OnError -= Client_OnError;
            client.Close();
            client = null;
            if (connected)
            {
#if UNITY_EDITOR
                Debug.Log(string.Format("WebSocket : Disconnected from {0}", remoteAddress));
#else
                Debug.Log("WebSocket : Disconnected from server");
#endif
                callOnDisconnect = true;
            }
        }
        if (fileStream != null)
        {
            fileStream.Close();
            fileStream = null;
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (Exception)
                {
                }
            }
        }
        delayedTime = 0f;
        connecting = false;
        remoteUri = null;
        remoteAddress = null;
        isTransferFile = false;
        eventQueue.Clear();
    }

    private IEnumerator Checking()
    {
        yield return null;

        while (enabled)
        {
            if (client == null && ReconnectInterval > 0f && !connecting)
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
        checkingRoutine = null;
    }

    private void ConnectAsync()
    {
        if (client == null)
        {
            try
            {
                client = new WebSocket(remoteUri.ToString());
                client.WaitTime = TimeSpan.FromSeconds(10);
                client.EnableRedirection = true;
                client.OnOpen += Client_OnOpen;
                client.OnClose += Client_OnClose;
                client.OnMessage += Client_OnMessage;
                client.OnError += Client_OnError;
                client.ConnectAsync();
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("WebSocket : {0}", ex.Message));
                Disconnect();
            }
        }
    }

    private void Client_OnOpen(object sender, EventArgs e)
    {
        connecting = false;
#if UNITY_EDITOR
        Debug.Log(string.Format("WebSocket : Connected to {0}", remoteAddress));
#else
        Debug.Log("WebSocket : Connected to server");
#endif
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
#if UNITY_EDITOR
            Debug.Log(string.Format("WebSocket : Disconnected from {0}", remoteAddress));
#else
            Debug.Log("WebSocket : Disconnected from server");
#endif
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
                        Debug.LogWarning(string.Format("WebSocket : {0}", ex.Message));
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

    private void Client_OnError(object sender, WebSocketSharp.ErrorEventArgs e)
    {
        Debug.LogError(string.Format("WebSocket : {0}", e.Message));
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
