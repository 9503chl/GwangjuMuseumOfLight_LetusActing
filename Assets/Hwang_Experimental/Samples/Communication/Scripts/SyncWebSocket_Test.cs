using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class SyncWebSocket_Test : MonoBehaviour
{
    public Button ConnectButton;
    public Button DisconnectButton;
    public Button SendBinaryButton;
    public Button SendTextButton;
    public Button SendFileButton;
    public ScrollRect HistoryScrollView;
    public Text HistoryText;
    public Button ClearHistoryButton;

    [NonSerialized]
    private SyncWebSocket syncWebSocket;

    private const int MAX_STATUS_LINES = 200;

    public static string ApplicationPath
    {
        get { return Path.GetDirectoryName(Path.GetDirectoryName(Application.streamingAssetsPath)).Replace('\\', '/'); }
    }

    private void Awake()
    {
        syncWebSocket = gameObject.EnsureComponent<SyncWebSocket>();

        ConnectButton.interactable = true;
        ConnectButton.onClick.AddListener(ConnectButton_Click);
        DisconnectButton.interactable = false;
        DisconnectButton.onClick.AddListener(DisconnectButton_Click);
        SendBinaryButton.interactable = false;
        SendBinaryButton.onClick.AddListener(SendBinaryButton_Click);
        SendTextButton.interactable = false;
        SendTextButton.onClick.AddListener(SendTextButton_Click);
        SendFileButton.interactable = false;
        SendFileButton.onClick.AddListener(SendFileButton_Click);
        ClearHistoryButton.interactable = false;
        ClearHistoryButton.onClick.AddListener(ClearHistoryButton_Click);
    }

    private void Start()
    {
        if (!Application.isMobilePlatform)
        {
            syncWebSocket.SavePath = string.Format("{0}/Download", ApplicationPath);
        }
        syncWebSocket.OnConnect += SyncWebSocket_OnConnect;
        syncWebSocket.OnDisconnect += SyncWebSocket_OnDisconnect;
        syncWebSocket.OnReceive += SyncWebSocket_OnReceive;
        syncWebSocket.OnReceiveText += SyncWebSocket_OnReceiveText;
        syncWebSocket.OnReceiveFile += SyncWebSocket_OnReceiveFile;
        syncWebSocket.OnSendFile += SyncWebSocket_OnSendFile;

        ClearHistory();

        if (syncWebSocket.ConnectOnEnable)
        {
            ConnectButton.interactable = false;
            DisconnectButton.interactable = false;
            StartCoroutine(Connecting());
        }
    }

    private void ClearHistory()
    {
        HistoryText.text = string.Empty;
        HistoryText.SetLayoutDirty();
        HistoryScrollView.RecalulateContentSize();
        HistoryScrollView.verticalNormalizedPosition = 0f;
        ClearHistoryButton.interactable = false;
    }

    private void AddHistory(string text)
    {
        StringReader reader = new StringReader(HistoryText.text);
        List<string> lines = new List<string>();
        while (reader.Peek() != -1)
        {
            lines.Add(reader.ReadLine());
        }
        while (lines.Count >= MAX_STATUS_LINES)
        {
            lines.RemoveAt(0);
        }
        lines.Add(text);
        StringWriter writer = new StringWriter();
        for (int i = 0; i < lines.Count; i++)
        {
            writer.WriteLine(lines[i]);
        }
        HistoryText.text = writer.ToString();
        HistoryText.SetLayoutDirty();
        HistoryScrollView.RecalulateContentSize();
        HistoryScrollView.verticalNormalizedPosition = 0f;
        ClearHistoryButton.interactable = true;
    }

    private IEnumerator Connecting()
    {
        while (syncWebSocket.Connecting)
        {
            yield return null;
        }
        if (!syncWebSocket.Connected)
        {
            ConnectButton.interactable = true;
            DisconnectButton.interactable = false;
            AddHistory("서버에 연결하지 못했습니다.");
        }
    }

    private void ConnectButton_Click()
    {
        ConnectButton.interactable = false;
        DisconnectButton.interactable = false;
        syncWebSocket.Connect();
        StartCoroutine(Connecting());
    }

    private void DisconnectButton_Click()
    {
        syncWebSocket.Close();
        ConnectButton.interactable = true;
        DisconnectButton.interactable = false;
        SendBinaryButton.interactable = false;
        SendTextButton.interactable = false;
        SendFileButton.interactable = false;
        AddHistory("서버와의 연결을 끊었습니다.");
    }

    private void SendBinaryButton_Click()
    {
        if (syncWebSocket.Connected)
        {
            if (syncWebSocket.IsTransferFile)
            {
                AddHistory("파일을 보내거나 받는 중이므로 다른 데이터를 보낼 수 없습니다.");
            }
            else
            {
                byte[] data = Encoding.UTF8.GetBytes("Hi! 안녕");
                syncWebSocket.Send(data);
                AddHistory(string.Format("서버에게 바이너리({0} 바이트)를 보냈습니다.", data.Length));
            }
        }
        else
        {
            MessageBox.Show("서버에 연결되어 있지 않습니다.", "확인", 2f);
        }
    }

    private void SendTextButton_Click()
    {
        if (syncWebSocket.Connected)
        {
            if (syncWebSocket.IsTransferFile)
            {
                AddHistory("파일을 보내거나 받는 중이므로 다른 데이터를 보낼 수 없습니다.");
            }
            else
            {
                string text = "Good afternoon? 좋은 오후";
                syncWebSocket.SendText(text);
                AddHistory(string.Format("서버에게 텍스트(\"{0}\")를 보냈습니다.", text));
            }
        }
        else
        {
            MessageBox.Show("서버에 연결되어 있지 않습니다.", "확인", 2f);
        }
    }

    private void SendFileButton_Click()
    {
        if (syncWebSocket.Connected)
        {
            if (syncWebSocket.IsTransferFile)
            {
                AddHistory("파일을 보내거나 받는 중이므로 다른 데이터를 보낼 수 없습니다.");
            }
            else
            {
                //string path = string.Format("{0}/Upload/image.png", ApplicationPath);
                //string fileName = "image/web_socket_client.png";
                string path = string.Format("{0}/Upload/video.mp4", ApplicationPath);
                string fileName = "video/web_socket_client.mp4";
                if (File.Exists(path))
                {
                    syncWebSocket.SendFile(path, fileName);
                    AddHistory(string.Format("서버에게 파일(\"{0}\")을 보내기 시작합니다.", path));
                }
                else
                {
                    AddHistory(string.Format("서버에게 보낼 파일(\"{0}\")이 없습니다.", path));
                }
            }
        }
        else
        {
            MessageBox.Show("서버에 연결되어 있지 않습니다.", "확인", 2f);
        }
    }

    private void ClearHistoryButton_Click()
    {
        ClearHistory();
    }

    private void SyncWebSocket_OnConnect()
    {
        ConnectButton.interactable = false;
        DisconnectButton.interactable = true;
        SendBinaryButton.interactable = true;
        SendTextButton.interactable = true;
        SendFileButton.interactable = true;
        AddHistory("서버에 연결되었습니다.");
    }

    private void SyncWebSocket_OnDisconnect()
    {
        ConnectButton.interactable = true;
        DisconnectButton.interactable = false;
        SendBinaryButton.interactable = false;
        SendTextButton.interactable = false;
        SendFileButton.interactable = false;
        AddHistory("서버와의 연결이 끊어졌습니다.");
    }

    private void SyncWebSocket_OnReceive(byte[] data)
    {
        AddHistory(string.Format("서버로부터 바이너리({0} 바이트)를 받았습니다.", data.Length));
    }

    private void SyncWebSocket_OnReceiveText(string text)
    {
        AddHistory(string.Format("서버로부터 텍스트(\"{0}\")를 받았습니다.", text));
    }

    private void SyncWebSocket_OnReceiveFile(string path, bool completed)
    {
        if (completed)
        {
            AddHistory(string.Format("서버로부터 파일(\"{0}\")을 성공적으로 받았습니다.", path));
        }
        else
        {
            AddHistory(string.Format("서버로부터 파일(\"{0}\")을 받기 시작합니다.", path));
        }
    }

    private void SyncWebSocket_OnSendFile(string path, bool completed)
    {
        if (completed)
        {
            AddHistory(string.Format("서버에게 파일(\"{0}\")을 성공적으로 보냈습니다.", path));
        }
        else
        {
            AddHistory(string.Format("서버에게 파일(\"{0}\")을 보내지 못했습니다.", path));
        }
    }
}
