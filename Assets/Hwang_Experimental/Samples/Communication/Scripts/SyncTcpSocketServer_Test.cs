using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class SyncTcpSocketServer_Test : MonoBehaviour
{
    public Button StartServerButton;
    public Button StopServerButton;
    public Dropdown SessionListDropdown;
    public Button SendBinaryButton;
    public Button SendTextButton;
    public Button SendFileButton;
    public Button CloseSessionButton;
    public ScrollRect HistoryScrollView;
    public Text HistoryText;
    public Toggle ReceiveAsTextToggle;
    public Button ClearHistoryButton;

    [NonSerialized]
    private SyncTcpSocketServer syncTcpSocketServer;

    private const int MAX_STATUS_LINES = 200;

    public static string ApplicationPath
    {
        get { return Path.GetDirectoryName(Path.GetDirectoryName(Application.streamingAssetsPath)).Replace('\\', '/'); }
    }

    private void Awake()
    {
        syncTcpSocketServer = gameObject.EnsureComponent<SyncTcpSocketServer>();

        StartServerButton.interactable = true;
        StartServerButton.onClick.AddListener(StartServerButton_Click);
        StopServerButton.interactable = false;
        StopServerButton.onClick.AddListener(StopServerButton_Click);
        SessionListDropdown.interactable = false;
        SendBinaryButton.interactable = false;
        SendBinaryButton.onClick.AddListener(SendBinaryButton_Click);
        SendTextButton.interactable = false;
        SendTextButton.onClick.AddListener(SendTextButton_Click);
        SendFileButton.interactable = false;
        SendFileButton.onClick.AddListener(SendFileButton_Click);
        CloseSessionButton.interactable = false;
        CloseSessionButton.onClick.AddListener(CloseSessionButton_Click);
        ReceiveAsTextToggle.isOn = syncTcpSocketServer.ReceiveAsText;
        ReceiveAsTextToggle.onValueChanged.AddListener(ReceiveAsTextToggle_ValueChanged);
        ClearHistoryButton.interactable = false;
        ClearHistoryButton.onClick.AddListener(ClearHistoryButton_Click);
    }

    private void Start()
    {
        if (!Application.isMobilePlatform)
        {
            syncTcpSocketServer.SavePath = string.Format("{0}/Download", ApplicationPath);
        }
        syncTcpSocketServer.OnConnect += SyncTcpSocketServer_OnConnect;
        syncTcpSocketServer.OnDisconnect += SyncTcpSocketServer_OnDisconnect;
        syncTcpSocketServer.OnReceive += SyncTcpSocketServer_OnReceive;
        syncTcpSocketServer.OnReceiveText += SyncTcpSocketServer_OnReceiveText;
        syncTcpSocketServer.OnReceiveFile += SyncTcpSocketServer_OnReceiveFile;
        syncTcpSocketServer.OnSendFile += SyncTcpSocketServer_OnSendFile;

        ClearHistory();
        RebuildSessions();

        if (syncTcpSocketServer.ListenOnEnable)
        {
            StartServerButton.interactable = false;
            StopServerButton.interactable = false;
            if (syncTcpSocketServer.IsBound)
            {
                StopServerButton.interactable = true;
                SessionListDropdown.interactable = true;
                SendBinaryButton.interactable = true;
                SendTextButton.interactable = true;
                SendFileButton.interactable = true;
                CloseSessionButton.interactable = true;
                AddHistory(string.Format("서버를 시작했습니다. (포트 : {0})", syncTcpSocketServer.PortNumber));
            }
            else
            {
                StartServerButton.interactable = true;
                AddHistory("서버를 시작하지 못했습니다.");
            }
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

    private void RebuildSessions()
    {
        SessionListDropdown.ClearOptions();
        SessionListDropdown.options.Add(new Dropdown.OptionData("클라이언트 전체"));
        SessionListDropdown.value = -1;
        if (syncTcpSocketServer.IsBound)
        {
            TcpSocketSession[] sessions = syncTcpSocketServer.Sessions;
            foreach (TcpSocketSession session in sessions)
            {
                SessionListDropdown.options.Add(new Dropdown.OptionData(session.ID));
            }
            SessionListDropdown.interactable = true;
        }
        else
        {
            SessionListDropdown.interactable = false;
        }
        SessionListDropdown.value = 0;
    }

    private void AddSession(string ID)
    {
        foreach (Dropdown.OptionData option in SessionListDropdown.options)
        {
            if (string.Compare(option.text, ID) == 0)
            {
                return;
            }
        }
        SessionListDropdown.options.Add(new Dropdown.OptionData(ID));
    }

    private void RemoveSession(string ID)
    {
        int index = 0;
        int value = SessionListDropdown.value;
        foreach (Dropdown.OptionData option in SessionListDropdown.options)
        {
            if (string.Compare(option.text, ID) == 0)
            {
                SessionListDropdown.options.Remove(option);
                if (value > 0 && index <= value)
                {
                    SessionListDropdown.value = 0;
                }
                break;
            }
            index++;
        }
    }

    private string GetSelectedSessionID()
    {
        if (SessionListDropdown.interactable)
        {
            int value = SessionListDropdown.value;
            if (value > 0 && value < SessionListDropdown.options.Count)
            {
                return SessionListDropdown.options[value].text;
            }
        }
        return null;
    }

    private void StartServerButton_Click()
    {
        StartServerButton.interactable = false;
        StopServerButton.interactable = false;
        if (syncTcpSocketServer.Listen())
        {
            StopServerButton.interactable = true;
            SessionListDropdown.interactable = true;
            SendBinaryButton.interactable = true;
            SendTextButton.interactable = true;
            SendFileButton.interactable = true;
            CloseSessionButton.interactable = true;
            AddHistory(string.Format("서버를 시작했습니다. (포트 : {0})", syncTcpSocketServer.PortNumber));
        }
        else
        {
            StartServerButton.interactable = true;
            AddHistory("서버를 시작하지 못했습니다.");
        }
    }

    private void StopServerButton_Click()
    {
        syncTcpSocketServer.Close();
        RebuildSessions();
        StartServerButton.interactable = true;
        StopServerButton.interactable = false;
        SessionListDropdown.interactable = false;
        SendBinaryButton.interactable = false;
        SendTextButton.interactable = false;
        SendFileButton.interactable = false;
        CloseSessionButton.interactable = false;
        AddHistory("서버를 중지했습니다.");
    }

    private void SendBinaryButton_Click()
    {
        string ID = GetSelectedSessionID();
        if (!string.IsNullOrEmpty(ID))
        {
            TcpSocketSession session = syncTcpSocketServer.FindSession(ID);
            if (session != null && session.IsTransferFile)
            {
                AddHistory("파일을 보내거나 받는 중이므로 다른 데이터를 보낼 수 없습니다.");
            }
            else
            {
                byte[] data = Encoding.UTF8.GetBytes("Hello? 안녕");
                syncTcpSocketServer.SendTo(ID, data);
                AddHistory(string.Format("클라이언트({0})에게 바이너리({1} 바이트)를 보냈습니다.", ID, data.Length));
            }
        }
        else if (syncTcpSocketServer.SessionCount > 0)
        {
            byte[] data = Encoding.UTF8.GetBytes("Have a nice day! 즐거운 하루 되세요");
            syncTcpSocketServer.SendToAll(data);
            AddHistory(string.Format("클라이언트 전체에게 바이너리({0} 바이트)를 보냈습니다.", data.Length));
        }
        else
        {
            MessageBox.Show("연결중인 클라이언트가 없습니다.", "확인", 2f);
        }
    }

    private void SendTextButton_Click()
    {
        string ID = GetSelectedSessionID();
        if (!string.IsNullOrEmpty(ID))
        {
            TcpSocketSession session = syncTcpSocketServer.FindSession(ID);
            if (session != null && session.IsTransferFile)
            {
                AddHistory("파일을 보내거나 받는 중이므로 다른 데이터를 보낼 수 없습니다.");
            }
            else
            {
                string text = "Good morning? 좋은 아침";
                syncTcpSocketServer.SendTextTo(ID, text);
                AddHistory(string.Format("클라이언트({0})에게 텍스트(\"{1}\")를 보냈습니다.", ID, text));
            }
        }
        else if (syncTcpSocketServer.SessionCount > 0)
        {
            string text = "Nice to meet you! 만나서 반가워요";
            syncTcpSocketServer.SendTextToAll(text);
            AddHistory(string.Format("클라이언트 전체에게 텍스트(\"{0}\")를 보냈습니다.", text));
        }
        else
        {
            MessageBox.Show("연결중인 클라이언트가 없습니다.", "확인", 2f);
        }
    }

    private void SendFileButton_Click()
    {
        string ID = GetSelectedSessionID();
        if (!string.IsNullOrEmpty(ID))
        {
            TcpSocketSession session = syncTcpSocketServer.FindSession(ID);
            if (session != null && session.IsTransferFile)
            {
                AddHistory("파일을 보내거나 받는 중이므로 다른 데이터를 보낼 수 없습니다.");
            }
            else
            {
                string path = string.Format("{0}/Upload/video.mp4", ApplicationPath);
                if (File.Exists(path))
                {
                    syncTcpSocketServer.SendFileTo(ID, path);
                    AddHistory(string.Format("클라이언트({0})에게 파일(\"{1}\")을 보내기 시작합니다.", ID, path));
                }
                else
                {
                    AddHistory(string.Format("클라이언트({0})에게 보낼 파일(\"{1}\")이 없습니다.", ID, path));
                }
            }
        }
        else if (syncTcpSocketServer.SessionCount > 0)
        {
            string path = string.Format("{0}/Upload/video.mp4", ApplicationPath);
            if (File.Exists(path))
            {
                syncTcpSocketServer.SendFileToAll(path);
                AddHistory(string.Format("클라이언트 전체에게 파일(\"{0}\")을 보내기 시작합니다.", path));
            }
            else
            {
                AddHistory(string.Format("클라이언트 전체에게 보낼 파일(\"{0}\")이 없습니다.", path));
            }
        }
        else
        {
            MessageBox.Show("연결중인 클라이언트가 없습니다.", "확인", 2f);
        }
    }

    private void CloseSessionButton_Click()
    {
        string ID = GetSelectedSessionID();
        if (!string.IsNullOrEmpty(ID))
        {
            syncTcpSocketServer.Disconnect(ID);
            AddHistory(string.Format("클라이언트({0})의 연결을 강제로 끊었습니다.", ID));
        }
        else if (syncTcpSocketServer.SessionCount > 0)
        {
            syncTcpSocketServer.DisconnectAll();
            AddHistory("클라이언트 전체의 연결을 강제로 끊었습니다.");
        }
        else
        {
            MessageBox.Show("연결중인 클라이언트가 없습니다.", "확인", 2f);
        }
    }

    private void ReceiveAsTextToggle_ValueChanged(bool value)
    {
        syncTcpSocketServer.ReceiveAsText = value;
    }

    private void ClearHistoryButton_Click()
    {
        ClearHistory();
    }

    private void SyncTcpSocketServer_OnConnect(TcpSocketSession session)
    {
        AddSession(session.ID);
        AddHistory(string.Format("클라이언트({0})가 연결되었습니다.", session.ID));
    }

    private void SyncTcpSocketServer_OnDisconnect(TcpSocketSession session)
    {
        RemoveSession(session.ID);
        AddHistory(string.Format("클라이언트({0})의 연결이 끊어졌습니다.", session.ID));
    }

    private void SyncTcpSocketServer_OnReceive(TcpSocketSession session, byte[] data)
    {
        AddHistory(string.Format("클라이언트({0})로부터 바이너리({1} 바이트)를 받았습니다.", session.ID, data.Length));
    }

    private void SyncTcpSocketServer_OnReceiveText(TcpSocketSession session, string text)
    {
        AddHistory(string.Format("클라이언트({0})로부터 텍스트(\"{1}\")를 받았습니다.", session.ID, text));
    }

    private void SyncTcpSocketServer_OnReceiveFile(TcpSocketSession session, string path, bool completed)
    {
        if (completed)
        {
            AddHistory(string.Format("클라이언트({0})로부터 파일(\"{1}\")을 성공적으로 받았습니다.", session.ID, path));
        }
        else
        {
            AddHistory(string.Format("클라이언트({0})로부터 파일(\"{1}\")을 받기 시작합니다.", session.ID, path));
        }
    }

    private void SyncTcpSocketServer_OnSendFile(TcpSocketSession session, string path, bool completed)
    {
        if (completed)
        {
            AddHistory(string.Format("클라이언트({0})에게 파일(\"{1}\")을 성공적으로 보냈습니다.", session.ID, path));
        }
        else
        {
            AddHistory(string.Format("클라이언트({0})에게 파일(\"{1}\")을 보내지 못했습니다.", session.ID, path));
        }
    }
}
