using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

public class TcpSocketSession
{
    protected string id;
    public string ID
    {
        get { return id; }
    }

    protected Socket client;
    public bool Connected
    {
        get { return client != null && client.Connected; }
    }

    protected IPEndPoint remoteEndPoint;
    public string RemoteIP
    {
        get { return (remoteEndPoint != null) ? remoteEndPoint.Address.ToString() : string.Empty; }
    }

    protected readonly Dictionary<string, string> values = new Dictionary<string, string>();
    public string this[string key]
    {
        get { return values.ContainsKey(key) ? values[key] : null; }
        set { values[key] = value; }
    }

    protected bool isTransferFile;
    public bool IsTransferFile
    {
        get { return isTransferFile; }
        set { isTransferFile = value; }
    }

    public TcpSocketSession(Socket socket)
    {
        id = Guid.NewGuid().ToString().Replace("-", string.Empty);
        client = socket;
        remoteEndPoint = socket.RemoteEndPoint as IPEndPoint;
    }
}
