using System;
using System.Collections;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

[DisallowMultipleComponent]
public class UrgDeviceEthernet : UrgDevice
{
    public string IPAddress = "192.168.0.10";
    public int PortNumber = 10940;
    [Range(0f, 60f)]
    public float ReconnectInterval = 10f;
    public bool ConnectOnEnable = false;

    [NonSerialized]
    private TcpClient tcpClient;

    [NonSerialized]
    private Thread clientThread;

    [NonSerialized]
    private bool connecting = false;

    public override bool Connected
    {
        get
        {
            return tcpClient != null && tcpClient.Client != null && tcpClient.Connected;
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
        Disconnect();
    }

    private void OnApplicationQuit()
    {
        Disconnect();
    }

    public void Connect(string ipAddress, int portNumber)
    {
        IPAddress = ipAddress;
        PortNumber = portNumber;

        try
        {
            tcpClient = new TcpClient();
            connecting = true;
            if (ReconnectInterval > 0f)
            {
                StartCoroutine(Reconnect());
            }
            tcpClient.BeginConnect(IPAddress, PortNumber, ConnectCallback, null);
        }
        catch (Exception ex)
        {
            Debug.LogWarning(string.Format("URG Device : {0}", ex.Message));
            Disconnect();
        }
    }

    private void ConnectCallback(IAsyncResult ar)
    {
        if (tcpClient != null && tcpClient.Client != null)
        {
            try
            {
                tcpClient.EndConnect(ar);
                stream = tcpClient.GetStream();
                connecting = false;
                Debug.Log(string.Format("URG Device : Connected to {0}:{1}", IPAddress, PortNumber));
                StartClientThread();
                eventQueue.Enqueue(EventType.Connect);
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("URG Device : {0}", ex.Message));
                Disconnect();
            }
        }
    }

    public override void Connect()
    {
        base.Connect();
        Connect(IPAddress, PortNumber);
    }

    public override void Disconnect()
    {
        connecting = false;
        if (clientThread != null)
        {
            clientThread.Abort();
        }
        if (tcpClient != null && tcpClient.Client != null)
        {
            if (tcpClient.Connected)
            {
                stream = tcpClient.GetStream();
                if (stream != null)
                {
                    stream.Close();
                }
                eventQueue.Enqueue(EventType.Disconnect);
                Debug.Log("URG Device : Disconnected");
            }
            tcpClient.Close();
        }
        stream = null;
        base.Disconnect();
    }

    private void StartClientThread()
    {
        if (clientThread != null)
        {
            clientThread.Abort();
        }
        clientThread = new Thread(new ThreadStart(ClientThreadFunc));
        clientThread.IsBackground = true;
        clientThread.Start();
    }

    private IEnumerator Reconnect()
    {
        while (connecting)
        {
            yield return null;
        }

        if (!Connected)
        {
            yield return new WaitForSecondsRealtime(ReconnectInterval);
            if (!Connected)
            {
                Connect();
            }
        }
    }

    private void ClientThreadFunc()
    {
        try
        {
            while (Connected)
            {
                Thread.Sleep(1);
                ProcessRead();
            }
        }
        catch (ThreadAbortException)
        {
        }
        catch (Exception ex)
        {
            Debug.LogError(string.Format("URG Device : {0}", ex.Message));
        }
    }
}
