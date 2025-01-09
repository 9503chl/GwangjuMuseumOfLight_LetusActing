using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using WebSocketSharp;

public class WebSocketSession
{
    protected string id;
    public string ID
    {
        get { return id; }
    }

    protected WebSocket client;
    public bool Connected
    {
        get { return client != null && client.ReadyState == WebSocketState.Open; }
    }

    protected EndPoint remoteEndPoint;
    public string RemoteIP
    {
        get { if (remoteEndPoint != null && remoteEndPoint is IPEndPoint) return ((IPEndPoint)remoteEndPoint).Address.ToString(); else return string.Empty; }
    }

    protected readonly Dictionary<string, string> values = new Dictionary<string, string>();
    public string this[string key]
    {
        get { if (values.ContainsKey(key)) return values[key]; else return null; }
        set { values[key] = value; }
    }

    protected bool isTransferFile;
    public bool IsTransferFile
    {
        get { return isTransferFile; }
        set { isTransferFile = value; }
    }

    public WebSocketSession(string ID, WebSocket socket, EndPoint endPoint)
    {
        id = ID;
        client = socket;
        remoteEndPoint = endPoint;
    }
}
