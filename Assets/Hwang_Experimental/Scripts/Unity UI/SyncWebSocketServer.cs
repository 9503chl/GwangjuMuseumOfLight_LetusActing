using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;

[DisallowMultipleComponent]
public class SyncWebSocketServer : MonoBehaviour
{
    public int PortNumber = 8280;
    public bool ListenOnEnable = true;
    public bool UseRootService = true;

    [SerializeField]
    private List<string> otherServicePaths = new List<string>();

    [NonSerialized]
    private string savePath;
    public string SavePath
    {
        get { return savePath; }
        set { savePath = value; }
    }

    [NonSerialized]
    private WebSocketServer server;

    [Serializable]
    public class Service
    {
        public string ServicePath = "/";

        public event Action<WebSocketSession> OnConnect;
        public event Action<WebSocketSession> OnDisconnect;
        public event Action<WebSocketSession, byte[]> OnReceive;
        public event Action<WebSocketSession, string> OnReceiveText;
        public event Action<WebSocketSession, string, bool> OnReceiveFile;
        public event Action<WebSocketSession, string, bool> OnSendFile;

        private enum EventType
        {
            Connect,
            Disconnect,
            Receive,
            ReceiveText,
            ReceiveFile,
            SendFile
        }

        private class EventData
        {
            public EventType Type;
            public WebSocketSession Session;
            public byte[] Data;
            public string Text;
            public bool Completed;

            public EventData(EventType type, WebSocketSession session)
            {
                Session = session;
                Type = type;
            }

            public EventData(EventType type, WebSocketSession session, byte[] data) : this(type, session)
            {
                Data = data;
            }

            public EventData(EventType type, WebSocketSession session, string text) : this(type, session)
            {
                Text = text;
            }

            public EventData(EventType type, WebSocketSession session, string path, bool completed) : this(type, session)
            {
                Text = path;
                Completed = completed;
            }
        }

        [NonSerialized]
        private WebSocketServiceHost serviceHost;

        public WebSocketSession[] Sessions
        {
            get
            {
                List<WebSocketSession> result = new List<WebSocketSession>();
                if (serviceHost != null)
                {
                    foreach (string activeID in serviceHost.Sessions.ActiveIDs)
                    {
                        Session session = GetSession(activeID);
                        if (session != null)
                        {
                            result.Add(session.ClientSession);
                        }
                    }
                }
                return result.ToArray();
            }
        }

        [NonSerialized]
        private int sessionCount;
        public int SessionCount
        {
            get { return sessionCount; }
        }

        [NonSerialized]
        private bool enableChecking;

        private readonly Queue<EventData> eventQueue = new Queue<EventData>();

        public Service(string path)
        {
            ServicePath = path;
        }

        public void StartChecking(MonoBehaviour owner, WebSocketServiceHost host)
        {
            if (host != null)
            {
                serviceHost = host;
            }
            if (!enableChecking)
            {
                if (owner != null && owner.isActiveAndEnabled)
                {
                    enableChecking = true;
                    owner.StartCoroutine(Checking());
                }
            }
        }

        public void StopChecking()
        {
            enableChecking = false;
            serviceHost = null;
            sessionCount = 0;
        }

        private IEnumerator Checking()
        {
            while (enableChecking)
            {
                if (eventQueue.Count > 0)
                {
                    EventData eventData = eventQueue.Dequeue();
                    switch (eventData.Type)
                    {
                        case EventType.Connect:
                            if (OnConnect != null)
                            {
                                OnConnect.Invoke(eventData.Session);
                            }
                            break;
                        case EventType.Disconnect:
                            if (OnDisconnect != null)
                            {
                                OnDisconnect.Invoke(eventData.Session);
                            }
                            break;
                        case EventType.Receive:
                            if (OnReceive != null)
                            {
                                OnReceive.Invoke(eventData.Session, eventData.Data);
                            }
                            break;
                        case EventType.ReceiveText:
                            if (OnReceiveText != null)
                            {
                                OnReceiveText.Invoke(eventData.Session, eventData.Text);
                            }
                            break;
                        case EventType.ReceiveFile:
                            if (OnReceiveFile != null)
                            {
                                OnReceiveFile.Invoke(eventData.Session, eventData.Text, eventData.Completed);
                            }
                            break;
                        case EventType.SendFile:
                            if (OnSendFile != null)
                            {
                                OnSendFile.Invoke(eventData.Session, eventData.Text, eventData.Completed);
                            }
                            break;
                    }
                }
                yield return null;
            }
            eventQueue.Clear();
        }

        public void CallOnConnect(WebSocketSession session)
        {
            sessionCount++;
            lock (eventQueue)
            {
                eventQueue.Enqueue(new EventData(EventType.Connect, session));
            }
        }

        public void CallOnDisconnect(WebSocketSession session)
        {
            sessionCount--;
            lock (eventQueue)
            {
                eventQueue.Enqueue(new EventData(EventType.Disconnect, session));
            }
        }

        public void CallOnReceive(WebSocketSession session, byte[] data)
        {
            lock (eventQueue)
            {
                eventQueue.Enqueue(new EventData(EventType.Receive, session, data));
            }
        }

        public void CallOnReceiveText(WebSocketSession session, string text)
        {
            lock (eventQueue)
            {
                eventQueue.Enqueue(new EventData(EventType.ReceiveText, session, text));
            }
        }

        public void CallOnReceiveFile(WebSocketSession session, string path, bool completed)
        {
            lock (eventQueue)
            {
                eventQueue.Enqueue(new EventData(EventType.ReceiveFile, session, path, completed));
            }
        }

        public void CallOnSendFile(WebSocketSession session, string path, bool completed)
        {
            lock (eventQueue)
            {
                eventQueue.Enqueue(new EventData(EventType.SendFile, session, path, completed));
            }
        }

        public void Disconnect(string ID)
        {
            if (serviceHost != null)
            {
                foreach (string activeID in serviceHost.Sessions.ActiveIDs)
                {
                    if (string.Compare(activeID, ID, true) == 0)
                    {
                        serviceHost.Sessions.CloseSession(ID);
                        break;
                    }
                }
            }
        }

        public void DisconnectAll()
        {
            if (serviceHost != null)
            {
                foreach (string activeID in serviceHost.Sessions.ActiveIDs)
                {
                    serviceHost.Sessions.CloseSession(activeID);
                }
            }
        }

        private Session GetSession(string ID)
        {
            if (!string.IsNullOrEmpty(ID))
            {
                IWebSocketSession webSocketSession;
                if (serviceHost.Sessions.TryGetSession(ID, out webSocketSession))
                {
                    return webSocketSession as Session;
                }
            }
            return null;
        }

        public WebSocketSession FindSession(string ID)
        {
            Session session = GetSession(ID);
            if (session != null)
            {
                return session.ClientSession;
            }
            return null;
        }

        public WebSocketSession[] FindSessions(string key, string value)
        {
            List<WebSocketSession> result = new List<WebSocketSession>();
            foreach (string activeID in serviceHost.Sessions.ActiveIDs)
            {
                Session session = GetSession(activeID);
                if (session != null && session.ClientSession != null)
                {
                    if (string.Compare(session.ClientSession[key], value, true) == 0)
                    {
                        result.Add(session.ClientSession);
                    }
                }
            }
            return result.ToArray();
        }

        public void SendTo(string ID, byte[] data)
        {
            if (serviceHost != null)
            {
                Session session = GetSession(ID);
                if (session != null && session.IsTransferFile)
                {
                    Debug.LogWarning(string.Format("WebSocketServer({0}) : Cannot send anything to {1} while transfering file", ServicePath, ID));
                    return;
                }
                serviceHost.Sessions.SendToAsync(data, ID, null);
            }
        }

        public void SendTextTo(string ID, string text)
        {
            if (serviceHost != null)
            {
                Session session = GetSession(ID);
                if (session != null && session.IsTransferFile)
                {
                    Debug.LogWarning(string.Format("WebSocketServer({0}) : Cannot send anything to {1} while transfering file", ServicePath, ID));
                    return;
                }
                serviceHost.Sessions.SendToAsync(text, ID, null);
            }
        }

        public void SendFileTo(string ID, string path, string fileName = null)
        {
            if (serviceHost != null)
            {
                Session session = GetSession(ID);
                if (session != null && session.IsTransferFile)
                {
                    Debug.LogWarning(string.Format("WebSocketServer({0}) : Cannot send anything to {1} while transfering file", ServicePath, ID));
                    return;
                }
                FileInfo fileInfo = new FileInfo(path);
                if (fileInfo.Exists)
                {
                    if (string.IsNullOrEmpty(fileName))
                    {
                        fileName = fileInfo.Name;
                    }
                    session.FilePath = path;
                    session.FileSize = fileInfo.Length;
                    session.IsTransferFile = true;
                    serviceHost.Sessions.SendToAsync(string.Format("\bFILE\b*{0}*{1}", fileName, fileInfo.Length), ID, null);
                }
                else
                {
                    Debug.LogWarning("WebSocketServer : Cannot find file to send");
                }
            }
        }

        public void SendToAll(byte[] data, string excludeID = null)
        {
            if (serviceHost != null)
            {
                foreach (string activeID in serviceHost.Sessions.ActiveIDs)
                {
                    if (!string.IsNullOrEmpty(excludeID) && string.Compare(activeID, excludeID, true) == 0)
                    {
                        continue;
                    }
                    Session session = GetSession(activeID);
                    if (session != null && session.IsTransferFile)
                    {
                        Debug.LogWarning(string.Format("WebSocketServer({0}) : Cannot send anything to {1} while transfering file", ServicePath, activeID));
                        continue;
                    }
                    serviceHost.Sessions.SendToAsync(data, activeID, null);
                }
            }
        }

        public void SendTextToAll(string text, string excludeID = null)
        {
            if (serviceHost != null)
            {
                foreach (string activeID in serviceHost.Sessions.ActiveIDs)
                {
                    if (!string.IsNullOrEmpty(excludeID) && string.Compare(activeID, excludeID, true) == 0)
                    {
                        continue;
                    }
                    Session session = GetSession(activeID);
                    if (session != null && session.IsTransferFile)
                    {
                        Debug.LogWarning(string.Format("WebSocketServer({0}) : Cannot send anything to {1} while transfering file", ServicePath, activeID));
                        continue;
                    }
                    serviceHost.Sessions.SendToAsync(text, activeID, null);
                }
            }
        }

        public void SendFileToAll(string path, string fileName = null, string excludeID = null)
        {
            if (serviceHost != null)
            {
                FileInfo fileInfo = new FileInfo(path);
                foreach (string activeID in serviceHost.Sessions.ActiveIDs)
                {
                    if (!string.IsNullOrEmpty(excludeID) && string.Compare(activeID, excludeID, true) == 0)
                    {
                        continue;
                    }
                    Session session = GetSession(activeID);
                    if (session != null && session.IsTransferFile)
                    {
                        Debug.LogWarning(string.Format("WebSocketServer({0}) : Cannot send anything to {1} while transfering file", ServicePath, activeID));
                        continue;
                    }
                    if (fileInfo.Exists)
                    {
                        if (string.IsNullOrEmpty(fileName))
                        {
                            fileName = fileInfo.Name;
                        }
                        session.FilePath = path;
                        session.FileSize = fileInfo.Length;
                        session.IsTransferFile = true;
                        serviceHost.Sessions.SendToAsync(string.Format("\bFILE\b*{0}*{1}", fileName, fileInfo.Length), activeID, null);
                    }
                    else
                    {
                        Debug.LogWarning("WebSocketServer : Cannot find file to send");
                        break;
                    }
                }
            }
        }
    }

    protected class Session : WebSocketBehavior
    {
        public Service Service;
        public string SavePath;
        public WebSocketSession ClientSession;

        public string FilePath;
        public long FileSize;
        public Stream FileStream;

        public bool IsTransferFile
        {
            get { return ClientSession.IsTransferFile; }
            set { ClientSession.IsTransferFile = value; }
        }

        public Session() : base()
        {
            IgnoreExtensions = true;
        }

        protected override void OnOpen()
        {
            ClientSession = new WebSocketSession(ID, Context.WebSocket, Context.UserEndPoint);
            if (Service != null)
            {
                Debug.Log(string.Format("WebSocketServer({0}) : Connected {1} from {2}", Service.ServicePath, ID, ClientSession.RemoteIP));
                Service.CallOnConnect(ClientSession);
            }
        }

        protected override void OnClose(CloseEventArgs e)
        {
            if (Service != null)
            {
                Debug.Log(string.Format("WebSocketServer({0}) : Disconnected {1} from {2}", Service.ServicePath, ID, ClientSession.RemoteIP));
                Service.CallOnDisconnect(ClientSession);
            }
            if (FileStream != null)
            {
                FileStream.Close();
                FileStream = null;
                if (File.Exists(FilePath))
                {
                    try
                    {
                        File.Delete(FilePath);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            if (Service != null)
            {
                if (e.IsText)
                {
                    if (e.Data.StartsWith("\bFILE\b"))
                    {
                        string[] parts = e.Data.Split('*');
                        if (parts.Length == 3 && parts[1].Length > 0 && parts[2].Length > 0)
                        {
                            FilePath = string.Format("{0}/{1}", SavePath, parts[1].Trim());
                            FileSize = Convert.ToInt64(parts[2].Trim());
                            IsTransferFile = true;
                            try
                            {
                                string saveFilePath = FilePath;
                                string dir = Path.GetDirectoryName(FilePath);
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
                                            FileStream = new FileStream(tempPath, FileMode.CreateNew);
                                            FilePath = tempPath;
                                            break;
                                        }
                                        catch (Exception)
                                        {
                                        }
                                    }
                                }
                                if (FileStream == null)
                                {
                                    FilePath = string.Format("{0}.[{1}]", saveFilePath, Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
                                    FileStream = new FileStream(FilePath, FileMode.CreateNew);
                                }
                                SendAsync("\bFILE\b*ACCEPT", null);
                                Service.CallOnReceiveFile(ClientSession, saveFilePath, false);
                            }
                            catch (Exception ex)
                            {
                                IsTransferFile = false;
                                Debug.LogWarning(string.Format("WebSocketServer({0}) : {1}", Service.ServicePath, ex.Message));
                                SendAsync("\bFILE\b*REJECT", null);
                            }
                        }
                        else if (parts.Length == 2 && parts[1].Length > 0)
                        {
                            string answer = parts[1].Trim();
                            if (string.Compare(answer, "ACCEPT", true) == 0)
                            {
                                if (IsTransferFile)
                                {
                                    SendAsync(new FileInfo(FilePath), null);
                                }
                            }
                            else if (string.Compare(answer, "FINISH", true) == 0)
                            {
                                IsTransferFile = false;
                                Service.CallOnSendFile(ClientSession, FilePath, true);
                            }
                            else
                            {
                                IsTransferFile = false;
                                Service.CallOnSendFile(ClientSession, FilePath, false);
                            }
                        }
                    }
                    else
                    {
                        Service.CallOnReceiveText(ClientSession, e.Data);
                    }
                }
                else if (e.RawData.Length > 0)
                {
                    if (IsTransferFile)
                    {
                        if (FileStream == null)
                        {
                            FilePath = null;
                            FileStream = new MemoryStream();
                        }
                        FileStream.Write(e.RawData, 0, e.RawData.Length);
                        if (FileStream.Length >= FileSize)
                        {
                            FileStream.SetLength(FileSize);
                            FileStream.Flush();
                            FileStream.Close();
                            FileStream = null;
                            IsTransferFile = false;
                            try
                            {
                                string tempPath = Path.ChangeExtension(FilePath, null);
                                try
                                {
                                    File.Move(FilePath, tempPath);
                                    FilePath = tempPath;
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
                                                File.Move(FilePath, tempPath);
                                                FilePath = tempPath;
                                                break;
                                            }
                                            catch (Exception)
                                            {
                                            }
                                        }
                                    }
                                    if (string.Compare(FilePath, tempPath) != 0)
                                    {
                                        tempPath = string.Format("{0}/{1} ({2}){3}", path, name, Path.GetFileNameWithoutExtension(Path.GetRandomFileName()), ext);
                                        File.Move(FilePath, tempPath);
                                        FilePath = tempPath;
                                    }
                                }
                                SendAsync("\bFILE\b*FINISH", null);
                                Service.CallOnReceiveFile(ClientSession, FilePath, true);
                            }
                            catch (Exception)
                            {
                                SendAsync("\bFILE\b*FAILED", null);
                            }
                        }
                    }
                    else
                    {
                        Service.CallOnReceive(ClientSession, e.RawData);
                    }
                }
            }
        }

        protected override void OnError(WebSocketSharp.ErrorEventArgs e)
        {
            Debug.LogError(string.Format("WebSocketServer({0}) : {1}", Service.ServicePath, e.Message));
        }
    }

    [NonSerialized]
    private Service rootService;
    public Service RootService
    {
        get { return rootService; }
    }

    [NonSerialized]
    private List<Service> otherServices = new List<Service>();
    public Service[] OtherServices
    {
        get { return otherServices.ToArray(); }
    }

    public bool IsBound
    {
        get { return server != null && server.IsListening; }
    }

    private void Awake()
    {
        if (string.IsNullOrEmpty(savePath))
        {
            savePath = Application.persistentDataPath;
        }
        rootService = new Service("/");
        for (int i = 0; i < otherServicePaths.Count; i++)
        {
            string servicePath = FixServicePath(otherServicePaths[i]);
            otherServicePaths[i] = servicePath;
            otherServices.Add(new Service(servicePath));
        }
    }

    private void OnEnable()
    {
        if (ListenOnEnable)
        {
            Listen();
        }
    }

    private void OnDisable()
    {
        Close();
    }

    private string FixServicePath(string servicePath)
    {
        if (!string.IsNullOrEmpty(servicePath))
        {
            servicePath = servicePath.Replace("?", "_").Replace("#", "_");
            if (!servicePath.StartsWith("/"))
            {
                return "/" + servicePath;
            }
        }
        else
        {
            return string.Format("/{0:X8}", UnityEngine.Random.Range(0, int.MaxValue));
        }
        return servicePath;
    }

    private Service FindService(string servicePath)
    {
        foreach (Service service in otherServices)
        {
            if (string.Compare(service.ServicePath, servicePath, true) == 0)
            {
                return service;
            }
        }
        return null;
    }

    private void RegisterService(Service service)
    {
        if (server != null && service != null)
        {
            server.AddWebSocketService(service.ServicePath, delegate (Session session)
            {
                session.Service = service;
                session.SavePath = savePath;
            });
            if (server.IsListening)
            {
                WebSocketServiceHost host;
                if (server.WebSocketServices.TryGetServiceHost(service.ServicePath, out host))
                {
                    service.StartChecking(this, host);
                }
            }
        }
    }

    private void UnregisterService(Service service)
    {
        if (server != null && service != null)
        {
            service.StopChecking();
            server.RemoveWebSocketService(service.ServicePath);
        }
    }

    public Service AddService(string servicePath)
    {
        servicePath = FixServicePath(servicePath);
        Service service = FindService(servicePath);
        if (service == null)
        {
            service = new Service(servicePath);
            otherServicePaths.Add(servicePath);
            otherServices.Add(service);
        }
        RegisterService(service);
        return service;
    }

    public void RemoveService(Service service)
    {
        if (service != null)
        {
            UnregisterService(service);
            otherServicePaths.Remove(service.ServicePath);
            otherServices.Remove(service);
        }
    }

    public bool Listen()
    {
        if (server != null)
        {
            if (server.IsListening)
            {
                Debug.LogWarning("WebSocketServer : Already bound and listening");
                return false;
            }
            server.Stop();
            server = null;
        }
        try
        {
            server = new WebSocketServer(PortNumber);
            server.WaitTime = TimeSpan.FromSeconds(10);
            server.AllowForwardedRequest = true;
            server.KeepClean = true;
            server.Start();
            if (UseRootService)
            {
                RegisterService(rootService);
            }
            for (int i = 0; i < otherServicePaths.Count; i++)
            {
                string servicePath = FixServicePath(otherServicePaths[i]);
                Service service = FindService(servicePath);
                if (service == null)
                {
                    service = new Service(servicePath);
                    otherServices.Add(service);
                }
                otherServicePaths[i] = servicePath;
                RegisterService(service);
            }
            Debug.Log(string.Format("WebSocketServer : Listening TCP port {0}", PortNumber));
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError(string.Format("WebSocketServer : {0}", ex.Message));
        }
        return false;
    }

    public void Close()
    {
        if (server != null && server.IsListening)
        {
            try
            {
                server.Stop();
                if (UseRootService)
                {
                    UnregisterService(rootService);
                }
                foreach (Service otherService in otherServices)
                {
                    UnregisterService(otherService);
                }
                server = null;
                Debug.Log("WebSocketServer : Closed");
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("WebSocketServer : {0}", ex.Message));
            }
        }
    }
}
