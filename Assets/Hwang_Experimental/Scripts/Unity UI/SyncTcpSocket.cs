using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

[DisallowMultipleComponent]
public class SyncTcpSocket : MonoBehaviour
{
    public string HostAddress = "127.0.0.1";
    public int PortNumber = 8281;
    [Range(0f, 60f)]
    public float ReceiveTimeout = 10f;
    [Range(0f, 60f)]
    public float SendTimeout = 10f;
    [Range(0f, 60f)]
    public float ReconnectInterval = 10f;
    public bool ConnectOnEnable = true;
    public bool ReceiveAsText = false;

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
    private Socket client;

    [NonSerialized]
    private bool callOnConnect;

    [NonSerialized]
    private bool callOnDisconnect;

    [NonSerialized]
    private byte[] receiveBuffer;

    private readonly Queue<byte[]> receiveQueue = new Queue<byte[]>();

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
        get { return client != null && client.Connected; }
    }

    [NonSerialized]
    private IPEndPoint remoteEndPoint;
    public string RemoteIP
    {
        get { return (remoteEndPoint != null) ? remoteEndPoint.Address.ToString() : string.Empty; }
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
        Close();
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
            Debug.LogWarning("TcpSocket : Already connected or connecting");
            return;
        }
        if (checkingRoutine != null)
        {
            StopCoroutine(checkingRoutine);
            checkingRoutine = null;
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
        if (string.IsNullOrEmpty(HostAddress))
        {
            HostAddress = "127.0.0.1";
        }
        connecting = true;
        checkingRoutine = StartCoroutine(Checking());
        IPAddress hostIPAddress;
        if (IPAddress.TryParse(HostAddress, out hostIPAddress))
        {
            try
            {
                remoteAddress = string.Format("{0}:{1}", HostAddress, PortNumber);
                remoteEndPoint = new IPEndPoint(hostIPAddress, PortNumber);
                ConnectAsync();
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("TcpSocket : {0}", ex.Message));
                Disconnect();
            }
        }
        else
        {
            Dns.BeginGetHostAddresses(HostAddress, delegate (IAsyncResult ar)
            {
                try
                {
                    IPAddress[] ipAddresses = Dns.EndGetHostAddresses(ar);
                    foreach (IPAddress ipAddress in ipAddresses)
                    {
                        if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                        {
                            remoteAddress = string.Format("{0}:{1}", HostAddress, PortNumber);
                            remoteEndPoint = new IPEndPoint(ipAddress, PortNumber);
                            ConnectAsync();
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError(string.Format("TcpSocket : {0}", ex.Message));
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
            bool connected = client.Connected;
            client.Close();
            client = null;
            if (connected)
            {
#if UNITY_EDITOR
                Debug.Log(string.Format("TcpSocket : Disconnected from {0}", remoteAddress));
#else
                Debug.Log("TcpSocket : Disconnected from server");
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
        remoteEndPoint = null;
        remoteAddress = null;
        connecting = false;
        isTransferFile = false;
        receiveQueue.Clear();
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
            if (client != null && client.Connected)
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
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.ReceiveTimeout = Mathf.RoundToInt(ReceiveTimeout * 1000);
                client.SendTimeout = Mathf.RoundToInt(SendTimeout * 1000);
                client.DontFragment = true;
                client.NoDelay = true;
                if (receiveBuffer == null)
                {
                    receiveBuffer = new byte[client.ReceiveBufferSize];
                }
                client.BeginConnect(remoteEndPoint, delegate (IAsyncResult ar)
                {
                    if (client != null)
                    {
                        try
                        {
                            client.EndConnect(ar);
                            connecting = false;
                            callOnConnect = true;
#if UNITY_EDITOR
                            Debug.Log(string.Format("TcpSocket : Connected to {0}", remoteAddress));
#else
                            Debug.Log("TcpSocket : Connected to server");
#endif
                            ReceiveAsync();
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning(string.Format("TcpSocket : {0}", ex.Message));
                            Disconnect();
                        }
                    }
                }, null);
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("TcpSocket : {0}", ex.Message));
                Disconnect();
            }
        }
    }

    private void ReceiveAsync()
    {
        if (client != null && client.Connected)
        {
            try
            {
                client.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, delegate (IAsyncResult ar)
                {
                    if (client != null && client.Connected)
                    {
                        int length = client.EndReceive(ar);
                        if (length > 0)
                        {
                            byte[] buffer = new byte[length];
                            Buffer.BlockCopy(receiveBuffer, 0, buffer, 0, length);
                            receiveQueue.Enqueue(buffer);
                            ReceiveAsync();
                        }
                        else
                        {
                            Disconnect();
                        }
                    }
                }, null);
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("TcpSocket : {0}", ex.Message));
                Disconnect();
            }
        }
    }

    private void SendAsync(byte[] data)
    {
        if (client != null && client.Connected)
        {
            try
            {
                client.BeginSend(data, 0, data.Length, SocketFlags.None, delegate (IAsyncResult ar)
                {
                    if (client != null && client.Connected)
                    {
                        client.EndSend(ar);
                    }
                }, null);
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("TcpSocket : {0}", ex.Message));
                Disconnect();
            }
        }
    }

    private void SendFileAsync(string path)
    {
        if (client != null && client.Connected)
        {
            try
            {
                client.BeginSendFile(path, null, null, TransmitFileOptions.UseSystemThread, delegate (IAsyncResult ar)
                {
                    if (client != null && client.Connected)
                    {
                        client.EndSendFile(ar);
                    }
                }, null);
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("TcpSocket : {0}", ex.Message));
                Disconnect();
            }
        }
    }

    private void ProcessReceivedBuffer()
    {
        if (receiveQueue.Count > 0)
        {
            byte[] data = receiveQueue.Dequeue();
            string text = null;
            try
            {
                text = Encoding.UTF8.GetString(data);
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
                            SendAsync(Encoding.UTF8.GetBytes("\bFILE\b*ACCEPT"));
                            if (OnReceiveFile != null)
                            {
                                OnReceiveFile.Invoke(saveFilePath, false);
                            }
                        }
                        catch (Exception ex)
                        {
                            isTransferFile = false;
                            Debug.LogWarning(string.Format("TcpSocket : {0}", ex.Message));
                            SendAsync(Encoding.UTF8.GetBytes("\bFILE\b*REJECT"));
                        }
                    }
                    else if (parts.Length == 2 && parts[1].Length > 0)
                    {
                        string answer = parts[1].Trim();
                        if (string.Compare(answer, "ACCEPT", true) == 0)
                        {
                            if (isTransferFile)
                            {
                                SendFileAsync(filePath);
                            }
                        }
                        else if (string.Compare(answer, "FINISH", true) == 0)
                        {
                            isTransferFile = false;
                            if (OnSendFile != null)
                            {
                                OnSendFile.Invoke(filePath, true);
                            }
                        }
                        else
                        {
                            isTransferFile = false;
                            if (OnSendFile != null)
                            {
                                OnSendFile.Invoke(filePath, false);
                            }
                        }
                    }
                    return;
                }
            }
            if (isTransferFile)
            {
                if (fileStream == null)
                {
                    filePath = null;
                    fileStream = new MemoryStream();
                }
                fileStream.Write(data, 0, data.Length);
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
                        SendAsync(Encoding.UTF8.GetBytes("\bFILE\b*FINISH"));
                        if (OnReceiveFile != null)
                        {
                            OnReceiveFile.Invoke(filePath, true);
                        }
                    }
                    catch (Exception)
                    {
                        SendAsync(Encoding.UTF8.GetBytes("\bFILE\b*FAILED"));
                    }
                }
                return;
            }
            if (ReceiveAsText)
            {
                if (OnReceiveText != null)
                {
                    OnReceiveText.Invoke(text);
                }
            }
            else
            {
                if (OnReceive != null)
                {
                    OnReceive.Invoke(data);
                }
            }
        }
    }

    public void Send(byte[] data)
    {
        if (client != null && client.Connected)
        {
            if (isTransferFile)
            {
                Debug.LogWarning("TcpSocket : Cannot send anything while transfering file");
                return;
            }
            SendAsync(data);
        }
    }

    public void SendText(string text)
    {
        if (client != null && client.Connected)
        {
            if (isTransferFile)
            {
                Debug.LogWarning("TcpSocket : Cannot send anything while transfering file");
                return;
            }
            try
            {
                SendAsync(Encoding.UTF8.GetBytes(text));
            }
            catch (Exception)
            {
            }
        }
    }

    public void SendFile(string path, string fileName = null)
    {
        if (client != null && client.Connected)
        {
            if (isTransferFile)
            {
                Debug.LogWarning("TcpSocket : Cannot send anything while transfering file");
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
                SendAsync(Encoding.UTF8.GetBytes(string.Format("\bFILE\b*{0}*{1}", fileName, fileInfo.Length)));
            }
            else
            {
                Debug.LogWarning("TcpSocket : Cannot find file to send");
            }
        }
    }
}
