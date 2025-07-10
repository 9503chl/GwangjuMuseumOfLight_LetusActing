using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

[DisallowMultipleComponent]
public class SyncTcpSocketServer : MonoBehaviour
{
    public int PortNumber = 8281;
    [Range(0f, 60f)]
    public float ReceiveTimeout = 10f;
    [Range(0f, 60f)]
    public float SendTimeout = 10f;
    public bool ListenOnEnable = true;
    public bool ReceiveAsText = false;

    [NonSerialized]
    private string savePath;
    public string SavePath
    {
        get { return savePath; }
        set { savePath = value; }
    }

    public event Action<TcpSocketSession> OnConnect;
    public event Action<TcpSocketSession> OnDisconnect;
    public event Action<TcpSocketSession, byte[]> OnReceive;
    public event Action<TcpSocketSession, string> OnReceiveText;
    public event Action<TcpSocketSession, string, bool> OnReceiveFile;
    public event Action<TcpSocketSession, string, bool> OnSendFile;

    protected class Session : TcpSocketSession
    {
        public Socket Client
        {
            get { return client; }
        }

        public byte[] ReceiveBuffer;
        public readonly Queue<byte[]> ReceiveQueue = new Queue<byte[]>();

        public string FilePath;
        public long FileSize;
        public Stream FileStream;

        public Session(Socket socket) : base(socket)
        {
            ReceiveBuffer = new byte[socket.ReceiveBufferSize];
        }

        public void Close()
        {
            if (client != null)
            {
                client.Close();
                client = null;
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
            base.isTransferFile = false;
            ReceiveQueue.Clear();
        }
    }

    [NonSerialized]
    private Socket server;

    private readonly Queue<Session> connectedQueue = new Queue<Session>();
    private readonly Queue<Session> disconnectedQueue = new Queue<Session>();

    [NonSerialized]
    private Coroutine receivingRoutine;

    public bool IsBound
    {
        get { return server != null && server.IsBound; }
    }

    [NonSerialized]
    private List<Session> sessionList = new List<Session>();
    public TcpSocketSession[] Sessions
    {
        get { return sessionList.ToArray(); }
    }

    [NonSerialized]
    private int sessionCount;
    public int SessionCount
    {
        get { return sessionCount; }
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
        if (ListenOnEnable)
        {
            Listen();
        }
    }

    private void OnDisable()
    {
        Close();
    }

    public bool Listen()
    {
        return Listen(10);
    }

    public bool Listen(int backlog)
    {
        if (server != null)
        {
            if (server.IsBound)
            {
                Debug.LogWarning("TcpSocketServer : Already bound and listening");
                return false;
            }
            server.Close();
            server = null;
        }
        if (ReceiveAsText)
        {
            if (OnReceive != null && OnReceiveText == null)
            {
                ReceiveAsText = false;
            }
        }
        else
        {
            if (OnReceiveText != null && OnReceive == null)
            {
                ReceiveAsText = true;
            }
        }
        try
        {
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.ReceiveTimeout = Mathf.RoundToInt(ReceiveTimeout * 1000);
            server.SendTimeout = Mathf.RoundToInt(SendTimeout * 1000);
            server.DontFragment = true;
            server.NoDelay = true;
            server.Bind(new IPEndPoint(IPAddress.Any, PortNumber));
            server.Listen(backlog);
            Debug.Log(string.Format("TcpSocketServer : Listening TCP port {0}", PortNumber));
            receivingRoutine = StartCoroutine(Receiving());
            AcceptAsync();
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError(string.Format("TcpSocketServer : {0}", ex.Message));
            if (server != null)
            {
                server.Close();
                server = null;
            }
        }
        return false;
    }

    private IEnumerator Receiving()
    {
        yield return null;

        while (enabled)
        {
            if (server != null && server.IsBound)
            {
                while (connectedQueue.Count > 0)
                {
                    Session session = connectedQueue.Dequeue();
                    if (session != null)
                    {
                        if (OnConnect != null)
                        {
                            OnConnect.Invoke(session);
                        }
                    }
                }
                while (disconnectedQueue.Count > 0)
                {
                    Session session = disconnectedQueue.Dequeue();
                    if (session != null)
                    {
                        if (OnDisconnect != null)
                        {
                            OnDisconnect.Invoke(session);
                        }
                    }
                }
                Session[] sessions = sessionList.ToArray();
                for (int i = 0; i < sessions.Length; i++)
                {
                    if (sessions[i] != null && sessions[i].Client != null)
                    {
                        ProcessReceivedBuffer(sessions[i]);
                    }
                }
            }
            yield return null;
        }
        receivingRoutine = null;
    }

    public void Close()
    {
        if (receivingRoutine != null)
        {
            StopCoroutine(receivingRoutine);
            receivingRoutine = null;
        }
        if (server != null)
        {
            server.Close();
            server = null;
            Debug.Log("TcpSocketServer : Closed");
        }
        ClearSessions();
    }

    private void Disconnect(Socket socket)
    {
        if (socket != null)
        {
            bool connected = socket.Connected;
            Session session = RemoveSession(socket);
            if (connected && session != null)
            {
                Debug.Log(string.Format("TcpSocketServer : Disconnected {0} from {1}", session.ID, session.RemoteIP));
                if (session != null)
                {
                    disconnectedQueue.Enqueue(session);
                }
            }
        }
    }

    private void AcceptAsync()
    {
        if (server != null)
        {
            try
            {
                server.BeginAccept(delegate (IAsyncResult ar)
                {
                    if (server != null && server.IsBound)
                    {
                        Socket socket = server.EndAccept(ar);
                        if (socket != null)
                        {
                            Session session = AddSession(socket);
                            Debug.Log(string.Format("TcpSocketServer : Connected {0} from {1}", session.ID, session.RemoteIP));
                            ReceiveAsync(socket);
                        }
                        AcceptAsync();
                    }
                }, null);
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("TcpSocketServer : {0}", ex.Message));
            }
        }
    }

    private void ReceiveAsync(Socket socket)
    {
        if (server != null && socket != null)
        {
            try
            {
                Session session = GetSession(socket);
                socket.BeginReceive(session.ReceiveBuffer, 0, session.ReceiveBuffer.Length, SocketFlags.None, delegate (IAsyncResult ar)
                {
                    if (server != null && server.IsBound)
                    {
                        if (socket != null && socket.Connected)
                        {
                            int length = socket.EndReceive(ar);
                            if (length > 0)
                            {
                                if (session != null)
                                {
                                    byte[] buffer = new byte[length];
                                    Buffer.BlockCopy(session.ReceiveBuffer, 0, buffer, 0, length);
                                    session.ReceiveQueue.Enqueue(buffer);
                                }
                                ReceiveAsync(socket);
                            }
                            else
                            {
                                Disconnect(socket);
                            }
                        }
                        else
                        {
                            Disconnect(socket);
                        }
                    }
                }, null);
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("TcpSocketServer : {0}", ex.Message));
                Disconnect(socket);
            }
        }
    }

    private void SendAsync(Socket socket, byte[] data)
    {
        if (server != null && socket != null)
        {
            try
            {
                socket.BeginSend(data, 0, data.Length, SocketFlags.None, delegate (IAsyncResult ar)
                {
                    if (server != null && server.IsBound)
                    {
                        if (socket != null && socket.Connected)
                        {
                            socket.EndSend(ar);
                        }
                        else
                        {
                            Disconnect(socket);
                        }
                    }
                }, null);
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("TcpSocketServer : {0}", ex.Message));
                Disconnect(socket);
            }
        }
    }

    private void SendFileAsync(Socket socket, string path)
    {
        if (server != null && socket != null)
        {
            try
            {
                socket.BeginSendFile(path, null, null, TransmitFileOptions.UseDefaultWorkerThread, delegate (IAsyncResult ar)
                {
                    if (server != null && server.IsBound)
                    {
                        if (socket != null && socket.Connected)
                        {
                            socket.EndSendFile(ar);
                        }
                        else
                        {
                            Disconnect(socket);
                        }
                    }
                }, null);
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("TcpSocketServer : {0}", ex.Message));
                Disconnect(socket);
            }
        }
    }

    private Session AddSession(Socket socket)
    {
        Session session = null;
        if (socket != null)
        {
            Session[] sessions = sessionList.ToArray();
            for (int i = 0; i < sessions.Length; i++)
            {
                if (sessions[i] != null && sessions[i].Client == socket)
                {
                    session = sessions[i];
                    break;
                }
            }
            if (session == null)
            {
                session = new Session(socket);
                socket.SendTimeout = Mathf.RoundToInt(SendTimeout * 1000);
                socket.ReceiveTimeout = Mathf.RoundToInt(ReceiveTimeout * 1000);
                socket.NoDelay = true;
                socket.DontFragment = true;
                sessionList.Add(session);
                sessionCount++;
                connectedQueue.Enqueue(session);
            }
        }
        return session;
    }

    private Session GetSession(Socket socket)
    {
        Session session = null;
        if (socket != null)
        {
            Session[] sessions = sessionList.ToArray();
            for (int i = 0; i < sessions.Length; i++)
            {
                if (sessions[i] != null && sessions[i].Client == socket)
                {
                    session = sessions[i];
                    break;
                }
            }
        }
        return session;
    }

    private Session RemoveSession(Socket socket)
    {
        Session session = GetSession(socket);
        if (session != null)
        {
            session.Close();
            sessionList.Remove(session);
            sessionCount--;
        }
        return session;
    }

    private void ClearSessions()
    {
        Session[] sessions = sessionList.ToArray();
        foreach (Session session in sessions)
        {
            if (session != null)
            {
                session.Close();
            }
        }
        sessionList.Clear();
        sessionCount = 0;
    }

    private void ProcessReceivedBuffer(Session session)
    {
        if (session.ReceiveQueue.Count > 0)
        {
            byte[] buffer = session.ReceiveQueue.Dequeue();
            string text = null;
            try
            {
                text = Encoding.UTF8.GetString(buffer);
            }
            catch (Exception)
            {
            }
            if (!string.IsNullOrEmpty(text))
            {
                if (text.StartsWith("\bFILE\b"))
                {
                    string[] parts = text.Split('*');
                    if (parts.Length == 3 && parts[1].Length > 0 && parts[2].Length > 0)
                    {
                        session.FilePath = string.Format("{0}/{1}", savePath, parts[1].Trim());
                        session.FileSize = Convert.ToInt64(parts[2].Trim());
                        session.IsTransferFile = true;
                        try
                        {
                            string saveFilePath = session.FilePath;
                            string dir = Path.GetDirectoryName(session.FilePath);
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
                                        session.FileStream = new FileStream(tempPath, FileMode.CreateNew);
                                        session.FilePath = tempPath;
                                        break;
                                    }
                                    catch (Exception)
                                    {
                                    }
                                }
                            }
                            if (session.FileStream == null)
                            {
                                session.FilePath = string.Format("{0}.[{1}]", saveFilePath, Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
                                session.FileStream = new FileStream(session.FilePath, FileMode.CreateNew);
                            }
                            SendAsync(session.Client, Encoding.UTF8.GetBytes("\bFILE\b*ACCEPT"));
                            if (OnReceiveFile != null)
                            {
                                OnReceiveFile.Invoke(session, saveFilePath, false);
                            }
                        }
                        catch (Exception ex)
                        {
                            session.IsTransferFile = false;
                            Debug.LogWarning(string.Format("TcpSocketServer : {0}", ex.Message));
                            SendAsync(session.Client, Encoding.UTF8.GetBytes("\bFILE\b*REJECT"));
                        }
                    }
                    else if (parts.Length == 2 && parts[1].Length > 0)
                    {
                        string answer = parts[1].Trim();
                        if (string.Compare(answer, "ACCEPT", true) == 0)
                        {
                            if (session.IsTransferFile)
                            {
                                SendFileAsync(session.Client, session.FilePath);
                            }
                        }
                        else if (string.Compare(answer, "FINISH", true) == 0)
                        {
                            session.IsTransferFile = false;
                            if (OnSendFile != null)
                            {
                                OnSendFile.Invoke(session, session.FilePath, true);
                            }
                        }
                        else
                        {
                            session.IsTransferFile = false;
                            if (OnSendFile != null)
                            {
                                OnSendFile.Invoke(session, session.FilePath, false);
                            }
                        }
                    }
                    return;
                }
            }
            if (session.IsTransferFile)
            {
                if (session.FileStream == null)
                {
                    session.FilePath = null;
                    session.FileStream = new MemoryStream();
                }
                session.FileStream.Write(buffer, 0, buffer.Length);
                if (session.FileStream.Length >= session.FileSize)
                {
                    session.FileStream.SetLength(session.FileSize);
                    session.FileStream.Flush();
                    session.FileStream.Close();
                    session.FileStream = null;
                    session.IsTransferFile = false;
                    try
                    {
                        string tempPath = Path.ChangeExtension(session.FilePath, null);
                        try
                        {
                            File.Move(session.FilePath, tempPath);
                            session.FilePath = tempPath;
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
                                        File.Move(session.FilePath, tempPath);
                                        session.FilePath = tempPath;
                                        break;
                                    }
                                    catch (Exception)
                                    {
                                    }
                                }
                            }
                            if (string.Compare(session.FilePath, tempPath) != 0)
                            {
                                tempPath = string.Format("{0}/{1} ({2}){3}", path, name, Path.GetFileNameWithoutExtension(Path.GetRandomFileName()), ext);
                                File.Move(session.FilePath, tempPath);
                                session.FilePath = tempPath;
                            }
                        }
                        SendAsync(session.Client, Encoding.UTF8.GetBytes("\bFILE\b*FINISH"));
                        if (OnReceiveFile != null)
                        {
                            OnReceiveFile.Invoke(session, session.FilePath, true);
                        }
                    }
                    catch (Exception)
                    {
                        SendAsync(session.Client, Encoding.UTF8.GetBytes("\bFILE\b*FAILED"));
                    }
                }
                return;
            }
            if (ReceiveAsText)
            {
                if (OnReceiveText != null)
                {
                    OnReceiveText.Invoke(session, text);
                }
            }
            else
            {
                if (OnReceive != null)
                {
                    OnReceive.Invoke(session, buffer);
                }
            }
        }
    }

    private Session GetSession(string ID)
    {
        if (!string.IsNullOrEmpty(ID))
        {
            Session[] sessions = sessionList.ToArray();
            for (int i = 0; i < sessions.Length; i++)
            {
                if (string.Compare(sessions[i].ID, ID, true) == 0)
                {
                    return sessions[i];
                }
            }
        }
        return null;
    }

    public TcpSocketSession FindSession(string ID)
    {
        return GetSession(ID);
    }

    public TcpSocketSession[] FindSessions(string key, string value)
    {
        List<TcpSocketSession> result = new List<TcpSocketSession>();
        Session[] sessions = sessionList.ToArray();
        foreach (Session session in sessions)
        {
            if (session != null)
            {
                if (string.Compare(session[key], value, true) == 0)
                {
                    result.Add(session);
                }
            }
        }
        return result.ToArray();
    }

    public void Disconnect(string ID)
    {
        Session session = GetSession(ID);
        if (session != null && session.Connected)
        {
            Disconnect(session.Client);
        }
    }

    public void DisconnectAll()
    {
        Session[] sessions = sessionList.ToArray();
        foreach (Session session in sessions)
        {
            if (session != null && session.Connected)
            {
                Disconnect(session.Client);
            }
        }
    }

    public void SendTo(string ID, byte[] data)
    {
        Session session = GetSession(ID);
        if (session != null && session.Connected)
        {
            if (session.IsTransferFile)
            {
                Debug.LogWarning(string.Format("TcpSocketServer : Cannot send anything to {0} while transfering file", ID));
                return;
            }
            SendAsync(session.Client, data);
        }
    }

    public void SendTextTo(string ID, string text)
    {
        Session session = GetSession(ID);
        if (session != null && session.Connected)
        {
            if (session.IsTransferFile)
            {
                Debug.LogWarning(string.Format("TcpSocketServer : Cannot send anything to {0} while transfering file", ID));
                return;
            }
            try
            {
                SendAsync(session.Client, Encoding.UTF8.GetBytes(text));
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("TcpSocketServer : {0}", ex.Message));
            }
        }
    }

    public void SendFileTo(string ID, string path, string fileName = null)
    {
        Session session = GetSession(ID);
        if (session != null && session.Connected)
        {
            if (session.IsTransferFile)
            {
                Debug.LogWarning(string.Format("TcpSocketServer : Cannot send anything to {0} while transfering file", ID));
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
                SendAsync(session.Client, Encoding.UTF8.GetBytes(string.Format("\bFILE\b*{0}*{1}", fileName, fileInfo.Length)));
            }
            else
            {
                Debug.LogWarning("TcpSocketServer : Cannot find file to send");
            }
        }
    }

    public void SendToAll(byte[] data, string excludeID = null)
    {
        Session[] sessions = sessionList.ToArray();
        foreach (Session session in sessions)
        {
            if (session != null && session.Connected && string.Compare(session.ID, excludeID) != 0)
            {
                if (session.IsTransferFile)
                {
                    Debug.LogWarning(string.Format("TcpSocketServer : Cannot send anything to {0} while transfering file", session.ID));
                    continue;
                }
                SendAsync(session.Client, data);
            }
        }
    }

    public void SendTextToAll(string text, string excludeID = null)
    {
        Session[] sessions = sessionList.ToArray();
        try
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            foreach (Session session in sessions)
            {
                if (session != null && session.Connected && string.Compare(session.ID, excludeID) != 0)
                {
                    if (session.IsTransferFile)
                    {
                        Debug.LogWarning(string.Format("TcpSocketServer : Cannot send anything to {0} while transfering file", session.ID));
                        continue;
                    }
                    SendAsync(session.Client, buffer);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning(string.Format("TcpSocketServer : {0}", ex.Message));
        }
    }

    public void SendFileToAll(string path, string fileName = null, string excludeID = null)
    {
        FileInfo fileInfo = new FileInfo(path);
        Session[] sessions = sessionList.ToArray();
        foreach (Session session in sessions)
        {
            if (session != null && session.Connected && string.Compare(session.ID, excludeID) != 0)
            {
                if (session.IsTransferFile)
                {
                    Debug.LogWarning(string.Format("TcpSocketServer : Cannot send anything to {0} while transfering file", session.ID));
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
                    SendAsync(session.Client, Encoding.UTF8.GetBytes(string.Format("\bFILE\b*{0}*{1}", fileName, fileInfo.Length)));
                }
                else
                {
                    Debug.LogWarning("TcpSocketServer : Cannot find file to send");
                    break;
                }
            }
        }
    }
}
