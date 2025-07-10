using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class SyncSerialPort_Test : MonoBehaviour
{
    public Button OpenPortButton;
    public Button ClosePortButton;
    public InputField DataInputField;
    public Button SendBinaryButton;
    public Button SendTextButton;
    public ScrollRect HistoryScrollView;
    public Text HistoryText;
    public Toggle ReadAsTextToggle;
    public Button ClearHistoryButton;

    [NonSerialized]
    private SyncSerialPort syncSerialPort;

    private const int MAX_STATUS_LINES = 200;

    public static string ApplicationPath
    {
        get { return Path.GetDirectoryName(Path.GetDirectoryName(Application.streamingAssetsPath)).Replace('\\', '/'); }
    }

    private void Awake()
    {
        syncSerialPort = gameObject.EnsureComponent<SyncSerialPort>();

        OpenPortButton.interactable = true;
        OpenPortButton.onClick.AddListener(OpenPortButton_Click);
        ClosePortButton.interactable = false;
        ClosePortButton.onClick.AddListener(ClosePortButton_Click);
        SendBinaryButton.interactable = false;
        SendBinaryButton.onClick.AddListener(SendBinaryButton_Click);
        SendTextButton.interactable = false;
        SendTextButton.onClick.AddListener(SendTextButton_Click);
        ReadAsTextToggle.isOn = syncSerialPort.ReadAsText;
        ReadAsTextToggle.onValueChanged.AddListener(ReceiveAsTextToggle_ValueChanged);
        ClearHistoryButton.interactable = false;
        ClearHistoryButton.onClick.AddListener(ClearHistoryButton_Click);
    }

    private void Start()
    {
        syncSerialPort.OnOpen += syncSerialPort_OnOpen;
        syncSerialPort.OnClose += syncSerialPort_OnClose;
        syncSerialPort.OnRead += syncSerialPort_OnRead;
        syncSerialPort.OnReadText += syncSerialPort_OnReadText;

        ClearHistory();

        if (syncSerialPort.OpenOnEnable)
        {
            if (syncSerialPort.IsOpen)
            {
                OpenPortButton.interactable = false;
                ClosePortButton.interactable = true;
                AddHistory("시리얼 포트를 열었습니다.");
            }
            else
            {
                OpenPortButton.interactable = true;
                ClosePortButton.interactable = false;
                AddHistory("시리얼 포트를 열 수 없습니다.");
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

    private void OpenPortButton_Click()
    {
        OpenPortButton.interactable = false;
        ClosePortButton.interactable = false;
        syncSerialPort.Open();
        if (syncSerialPort.IsOpen)
        {
            OpenPortButton.interactable = false;
            ClosePortButton.interactable = true;
            AddHistory("시리얼 포트를 열었습니다.");
        }
        else
        {
            OpenPortButton.interactable = true;
            ClosePortButton.interactable = false;
            AddHistory("시리얼 포트를 열 수 없습니다.");
        }
    }

    private void ClosePortButton_Click()
    {
        syncSerialPort.Close();
        OpenPortButton.interactable = true;
        ClosePortButton.interactable = false;
        SendBinaryButton.interactable = false;
        SendTextButton.interactable = false;
        AddHistory("시리얼 포트를 닫았습니다.");
    }

    private void SendBinaryButton_Click()
    {
        if (syncSerialPort.IsOpen)
        {
            byte[] data;
            if (HexConverter.TryGetBytes(DataInputField.text, out data))
            {
                syncSerialPort.Write(data);
                AddHistory(string.Format("시리얼 포트로 바이너리({0} 바이트)를 보냈습니다.", data.Length));
                DataInputField.text = HexConverter.ToString(data, 1);
            }
            else
            {
                MessageBox.Show("보낼 바이너리가 없거나 형식에 맞지 않습니다.", "확인", 2f);
            }
        }
        else
        {
            MessageBox.Show("시리얼 포트가 열려 있지 않습니다.", "확인", 2f);
        }
    }

    private void SendTextButton_Click()
    {
        if (syncSerialPort.IsOpen)
        {
            string text = DataInputField.text;
            if (!string.IsNullOrEmpty(text))
            {
                syncSerialPort.WriteText(text);
                AddHistory(string.Format("시리얼 포트로 텍스트(\"{0}\")를 보냈습니다.", text));
            }
            else
            {
                MessageBox.Show("보낼 텍스트를 입력하십시오.", "확인", 2f);
            }
        }
        else
        {
            MessageBox.Show("시리얼 포트가 열려 있지 않습니다.", "확인", 2f);
        }
    }

    private void ReceiveAsTextToggle_ValueChanged(bool value)
    {
        syncSerialPort.ReadAsText = value;
    }

    private void ClearHistoryButton_Click()
    {
        ClearHistory();
    }

    private void syncSerialPort_OnOpen()
    {
        OpenPortButton.interactable = false;
        ClosePortButton.interactable = true;
        SendBinaryButton.interactable = true;
        SendTextButton.interactable = true;
    }

    private void syncSerialPort_OnClose()
    {
        OpenPortButton.interactable = true;
        ClosePortButton.interactable = false;
        SendBinaryButton.interactable = false;
        SendTextButton.interactable = false;
    }

    private void syncSerialPort_OnRead(byte[] data)
    {
        AddHistory(string.Format("시리얼 포트부터 바이너리({0} 바이트)를 받았습니다.", data.Length));
        AddHistory(HexConverter.ToString(data));
    }

    private void syncSerialPort_OnReadText(string text)
    {
        AddHistory(string.Format("시리얼 포트로부터 텍스트(\"{0}\")를 받았습니다.", text));
    }
}
