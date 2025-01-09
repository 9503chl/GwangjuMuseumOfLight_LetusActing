using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using WebSocketSharp;

public class SyncWebSocketClient : MonoBehaviour
{
    public string WebSocketURL = "ws://127.0.0.1";
    public bool ConnectOnEnable = true;
    public float ReconnectInterval = 10f;

    public event Action OnConnect;
    public event Action OnDisconnect;
    public event Action<string> OnReceiveText;

    [NonSerialized]
    private WebSocket webSocket;

    [NonSerialized]
    private bool callOnConnect;

    [NonSerialized]
    private bool callOnDisconnect;

    [NonSerialized]
    private bool connecting;

    [NonSerialized]
    private Queue<string> receive_queue = new Queue<string>();

    [NonSerialized]
    private float delayedTime;

    [NonSerialized]
    private Coroutine checkingRoutine;

    public bool Connecting
    {
        get
        {
            return connecting;
        }
    }

    public bool Connected
    {
        get { return webSocket != null && webSocket.IsAlive; }
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
        if (callOnDisconnect)
        {
            callOnDisconnect = false;
            if (OnDisconnect != null)
            {
                OnDisconnect.Invoke();
            }
        }
        if (checkingRoutine != null)
        {
            StopCoroutine(checkingRoutine);
        }
    }

    private void OnDestroy()
    {
        Disconnect();
    }

    public void Connect()
    {
        if (checkingRoutine != null)
        {
            StopCoroutine(checkingRoutine);
        }
        delayedTime = 0f;
        connecting = true;
        webSocket = new WebSocket(WebSocketURL);
        webSocket.WaitTime = TimeSpan.FromSeconds(20);
        webSocket.OnOpen += WebSocket_OnOpen;
        webSocket.OnClose += WebSocket_OnClose;
        webSocket.OnMessage += WebSocket_OnMessage;
        webSocket.OnError += WebSocket_OnError;
        webSocket.ConnectAsync();
        checkingRoutine = StartCoroutine(Checking());
    }

    public void Disconnect()
    {
        if (webSocket != null)
        {
            webSocket.OnOpen -= WebSocket_OnOpen;
            webSocket.OnClose -= WebSocket_OnClose;
            webSocket.OnMessage -= WebSocket_OnMessage;
            webSocket.OnError -= WebSocket_OnError;
            webSocket.CloseAsync();
            webSocket = null;
        }
        connecting = false;
        delayedTime = 0f;
        receive_queue.Clear();
    }

    private IEnumerator Checking()
    {
        while (true)
        {
            if (webSocket == null)
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
            //else if (!webSocket.IsAlive && !connecting)
            //{
            //    Debug.LogWarning("WebSocket : Abnormally disconnected");
            //    Disconnect();
            //}
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
            ProcessReceivedBuffer();
            yield return null;
        }
    }

    private void WebSocket_OnOpen(object sender, EventArgs e)
    {
        connecting = false;
#if UNITY_EDITOR
        Debug.Log(string.Format("WebSocket : Connected to {0}", WebSocketURL));
#else
        Debug.Log("WebSocket : Connected");
#endif
        callOnConnect = true;
    }

    private void WebSocket_OnClose(object sender, CloseEventArgs e)
    {
        if (connecting)
        {
            Debug.LogWarning("WebSocket : Failed to connect");
        }
        else
        {
            Debug.Log("WebSocket : Disconnected");
            callOnDisconnect = true;
        }
        Disconnect();
    }

    private void WebSocket_OnMessage(object sender, MessageEventArgs e)
    {
        if (e.IsText)
        {
#if UNITY_EDITOR
            //Debug.Log(string.Format("WebSocket : Received text {0} bytes", e.Data.Length)); // 로그가 너무 많아서 주석처리
#endif
            receive_queue.Enqueue(e.Data);
        }
    }

    private void WebSocket_OnError(object sender, ErrorEventArgs e)
    {
        Debug.LogError(string.Format("WebSocket : {0}", e.Message));
    }

    private void ProcessReceivedBuffer()
    {
        if (receive_queue.Count > 0)
        {
            string data = receive_queue.Dequeue();
            if (OnReceiveText != null)
            {
                OnReceiveText.Invoke(data);
            }
        }
    }

    public void SendText(string text)
    {
        if (Connected)
        {
            webSocket.SendAsync(text, delegate (bool success)
            {
#if UNITY_EDITOR
                if (!success)
                {
                    Debug.Log("WebSocket : Failed to send text");
                }
#endif
            });
        }
    }
    public void SendFile(string path, string fileName)
    {
        if (Connected)
        {
            System.IO.FileInfo fileInfo = new System.IO.FileInfo(path);
            webSocket.SendAsync(fileName, delegate (bool success)
            {
                webSocket.SendAsync(fileInfo, delegate (bool success)
                {
                    if (success)
                    {
                        webSocket.SendAsync("*", null);
                    }
#if UNITY_EDITOR
                    else
                    {
                        Debug.Log("WebSocket : Failed to send file");
                    }
#endif
                });
            });
        }
    }
}
