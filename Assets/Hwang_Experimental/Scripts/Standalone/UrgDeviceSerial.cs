using System;
using System.Collections;
using System.Threading;
using UnityEngine;

#if SYSTEM_IO_PORTS
#if NET_2_0 || NET_2_0_SUBSET || NET_STANDARD_2_0
#error System.IO.Ports requires .NET Framework 4.x
#endif
using System.IO.Ports;
#else
using OpenNETCF.IO.Ports;
#endif

[DisallowMultipleComponent]
public class UrgDeviceSerial : UrgDevice
{
    public string PortName = "COM1";
    public int BaudRate = 19200;
    [Range(0f, 60f)]
    public float ReconnectInterval = 10f;
    public bool ConnectOnEnable = false;

    [NonSerialized]
    private SerialPort serialPort;

    [NonSerialized]
    private Thread clientThread;

    [NonSerialized]
    private bool connecting = false;

    public override bool Connected
    {
        get
        {
            return serialPort != null && serialPort.IsOpen;
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

    public void Connect(string portName, int baudRate)
    {
        PortName = portName;
        BaudRate = baudRate;
        try
        {
            serialPort = new SerialPort(PortName, BaudRate);
            connecting = true;
            if (ReconnectInterval > 0f)
            {
                StartCoroutine(Reconnect());
            }
            serialPort.Open();
            stream = serialPort.BaseStream;
            connecting = false;
            Debug.Log(string.Format("URG Device : Connected to {0}:{1}", PortName, BaudRate));
            StartClientThread();
            eventQueue.Enqueue(EventType.Connect);
        }
        catch (Exception ex)
        {
            Debug.LogWarning(string.Format("URG Device : {0}", ex.Message));
            Disconnect();
        }
    }

    public override void Connect()
    {
        base.Connect();
        Connect(PortName, BaudRate);
    }

    public override void Disconnect()
    {
        connecting = false;
        if (clientThread != null)
        {
            clientThread.Abort();
        }
        if (serialPort != null)
        {
            if (serialPort.IsOpen)
            {
                stream = serialPort.BaseStream;
                if (stream != null)
                {
                    stream.Close();
                }
                eventQueue.Enqueue(EventType.Connect);
                Debug.Log("URG Device : Disconnected");
            }
            serialPort.Close();
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
