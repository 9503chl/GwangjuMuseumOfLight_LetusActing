using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
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
public sealed class SyncSerialPort : MonoBehaviour
{
    public string PortName = "COM1";
    public int BaudRate = 9600;
    public Parity Parity = Parity.None;
    [Range(5, 8)]
    public int DataBits = 8;
    public StopBits StopBits = StopBits.One;
    public Handshake Handshake = Handshake.None;
    [Range(0f, 60f)]
    public float ReadTimeout = 10f;
    [Range(0f, 60f)]
    public float WriteTimeout = 10f;
    [Range(0f, 60f)]
    public float ReopenInterval = 10f;
    public bool OpenOnEnable = true;
    public bool ReadAsText = false;

    public event Action OnOpen;
    public event Action OnClose;
    public event Action<byte[]> OnRead;
    public event Action<string> OnReadText;

    [NonSerialized]
    private SerialPort serialPort;

    [NonSerialized]
    private string openPortName;

    private readonly Queue<byte[]> readQueue = new Queue<byte[]>();

    [NonSerialized]
    private float delayedTime;

    [NonSerialized]
    private Coroutine checkingRoutine;

    public bool IsOpen
    {
        get { return serialPort != null && serialPort.IsOpen; }
    }

    private void OnEnable()
    {
        if (OpenOnEnable)
        {
            Open();
        }
    }

    private void OnDisable()
    {
        Close();
    }

#if UNITY_EDITOR
#if SYSTEM_IO_PORTS
    private const bool isNet4X = true;
#else
    private const bool isNet4X = false;
#endif

    [HideInInspector]
    public bool wasNet4X = isNet4X;

    private void OnValidate() 
    {
        if (wasNet4X != isNet4X)
        {
#if SYSTEM_IO_PORTS
            if (StopBits == (StopBits)2)
            {
                StopBits = StopBits.OnePointFive;
            }
            else if (StopBits == (StopBits)4)
            {
                StopBits = StopBits.Two;
            }
#else
            if (StopBits == (StopBits)0)
            {
                StopBits = StopBits.One;
            }
            else if (StopBits == (StopBits)2)
            {
                StopBits = StopBits.Two;
            }
            else if (StopBits == (StopBits)3)
            {
                StopBits = StopBits.OnePointFive;
            }
#endif
            wasNet4X = isNet4X;
        }
    }
#endif

    public void Open()
    {
        if (serialPort != null)
        {
            Debug.LogWarning("SerialPort : Already open");
            return;
        }
        if (checkingRoutine != null)
        {
            StopCoroutine(checkingRoutine);
            checkingRoutine = null;
        }
        if (ReadAsText)
        {
            if (OnRead != null && OnReadText == null)
            {
                ReadAsText = false;
            }
        }
        else
        {
            if (OnReadText != null && OnRead == null)
            {
                ReadAsText = true;
            }
        }
        serialPort = new SerialPort(PortName, BaudRate);
        serialPort.Parity = Parity;
        serialPort.DataBits = DataBits;
        serialPort.StopBits = StopBits;
        serialPort.Handshake = Handshake;
        serialPort.ReadTimeout = Mathf.RoundToInt(ReadTimeout * 1000);
        serialPort.WriteTimeout = Mathf.RoundToInt(WriteTimeout * 1000);
#if SYSTEM_IO_PORTS
        serialPort.DataReceived += SerialPort_DataReceived;
#else
        serialPort.ReceivedEvent += SerialPort_ReceivedEvent;
#endif
        try
        {
            openPortName = PortName;
            serialPort.Open();
            Debug.Log(string.Format("SerialPort : Open {0} OK", openPortName));
            if (OnOpen != null)
            {
                OnOpen.Invoke();
            }
            checkingRoutine = StartCoroutine(Checking());
        }
        catch (Exception ex)
        {
            Debug.LogError(string.Format("SerialPort : {0}", ex.Message));
            Shutdown();
        }
    }

    public void Close()
    {
        Shutdown();
        if (checkingRoutine != null)
        {
            StopCoroutine(checkingRoutine);
            checkingRoutine = null;
        }
    }

    private void Shutdown()
    {
        if (serialPort != null)
        {
            bool wasOpen = serialPort.IsOpen;
            serialPort.Close();
            serialPort = null;
            if (wasOpen)
            {
                Debug.Log(string.Format("SerialPort : Closed {0}", openPortName));
                if (isActiveAndEnabled && OnClose != null)
                {
                    OnClose.Invoke();
                }
            }
        }
        delayedTime = 0f;
        readQueue.Clear();
    }

    private IEnumerator Checking()
    {
        yield return null;
        while (enabled)
        {
            if (serialPort == null && ReopenInterval > 0f)
            {
                if (delayedTime < ReopenInterval)
                {
                    delayedTime += Time.unscaledDeltaTime;
                }
                else
                {
                    delayedTime = 0f;
                    Open();
                }
            }
            if (serialPort != null && serialPort.IsOpen)
            {
                ProcessReadBuffer();
            }
            yield return null;
        }
        checkingRoutine = null;
    }

#if SYSTEM_IO_PORTS
    private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
#else
    private void SerialPort_ReceivedEvent(object sender, SerialReceivedEventArgs e)
#endif
    {
        int bytesToRead = serialPort.BytesToRead;
        byte[] buffer = new byte[bytesToRead];
        try
        {
            int bytesRead = serialPort.Read(buffer, 0, bytesToRead);
            if (bytesRead > 0)
            {
                readQueue.Enqueue(buffer);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(string.Format("SerialPort : {0}", ex.Message));
        }
    }

    private void ProcessReadBuffer()
    {
        if (readQueue.Count > 0)
        {
            byte[] buffer = readQueue.Dequeue();
            try
            {
                if (ReadAsText)
                {
                    if (OnReadText != null)
                    {
                        try
                        {
                            OnReadText.Invoke(Encoding.UTF8.GetString(buffer));
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning(string.Format("SerialPort : {0}", ex.Message));
                        }
                    }
                }
                else
                {
                    if (OnRead != null)
                    {
                        OnRead.Invoke(buffer);
                    }
                }
            }
            catch (Exception)
            {
            }
        }
    }

    public void Write(byte[] buffer)
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Write(buffer, 0, buffer.Length);
        }
    }

    public void WriteText(string text)
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            try
            {
                byte[] buffer = Encoding.UTF8.GetBytes(text);
                serialPort.Write(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("SerialPort : {0}", ex.Message));
            }
        }
    }
}
