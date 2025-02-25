using NUnit.Framework;
using System;
using System.IO;
using System.Xml;
using UnityEngine;

public class ProjectSettings
{
    #region 저장용 데이터
    public static string PathName;


    #endregion
    /// <summary>
    /// [시스템 설정] 제어용 웹소켓 서버 주소
    /// </summary>
    public static string SignalWebSocketUrl = "ws://192.168.71.54:8280";

    public static string WebSocketFileUrl = "ws://192.168.0.99:18182/file";

    public static string AssetBundlePath = "./Bundle";

    /// <summary>
    /// [시스템 설정] 시리얼 포트 이름
    /// </summary>
    public static string SerialPortName = "COM3";

    /// <summary>
    /// [시스템 설정] 시리얼 통신 속도
    /// </summary>
    public static int SerialBoundRate = 9600;

    public static float BackToTitleTime = 300f;
    public static float FinishTime = 10f;
    public static float TargetTime = 3;

    /// <summary>
    /// 프로그램 실행 경로 (/)
    /// </summary>
    public static string ApplicationPath
    {
        get
        {
            // 특정 상황에서 Application.dataPath가 변경되는 증상 때문에 Application.streamingAssetsPath를 사용
            //return Path.GetDirectoryName(Application.dataPath).Replace('\\', '/');
            return Path.GetDirectoryName(Path.GetDirectoryName(Application.streamingAssetsPath)).Replace('\\', '/');
        }
    }

    /// <summary>
    /// 스냅샷 경로 (/Snapshot)
    /// </summary>
    public static string SnapshotPath
    {
        get
        {
            return ApplicationPath + "/Snapshot";
        }
    }

    /// <summary>
    /// 스냅샷 하위 경로 (/Snapshot/{folderName}/{fileName})
    /// </summary>
    public static string GetSnapshotPath(string folderName, string fileName)
    {
        string path = SnapshotPath;
        if (!Directory.Exists(path))
        {
            Debug.Log(string.Format("Create directory : {0}", path));
            try
            {
                Directory.CreateDirectory(path);
            }
            catch (Exception)
            {
                Debug.LogWarning(string.Format("Failed to create directory : {0}", path));
            }
        }
        if (!string.IsNullOrEmpty(folderName))
        {
            path += "/" + folderName;
            if (!Directory.Exists(path))
            {
                Debug.Log(string.Format("Create directory : {0}", path));
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch (Exception)
                {
                    Debug.LogWarning(string.Format("Failed to create directory : {0}", path));
                }
            }
        }
        if (!string.IsNullOrEmpty(fileName))
        {
            path += "/" + fileName;
        }
        return path;
    }

    /// <summary>
    /// 다운로드 경로 (/Download)
    /// </summary>
    public static string DownloadPath
    {
        get
        {
            return ApplicationPath + "/Download";
        }
    }

    /// <summary>
    /// 다운로드 하위 경로 (/Download/{folderName}/{fileName})
    /// </summary>
    public static string GetDownloadPath(string folderName, string fileName)
    {
        string path = DownloadPath;
        if (!Directory.Exists(path))
        {
            Debug.Log(string.Format("Create directory : {0}", path));
            try
            {
                Directory.CreateDirectory(path);
            }
            catch (Exception)
            {
                Debug.LogWarning(string.Format("Failed to create directory : {0}", path));
            }
        }
        if (!string.IsNullOrEmpty(folderName))
        {
            path += "/" + folderName;
            if (!Directory.Exists(path))
            {
                Debug.Log(string.Format("Create directory : {0}", path));
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch (Exception)
                {
                    Debug.LogWarning(string.Format("Failed to create directory : {0}", path));
                }
            }
        }
        if (!string.IsNullOrEmpty(fileName))
        {
            path += "/" + fileName;
        }
        return path;
    }

    /// <summary>
    /// 지정한 경로의 오래된 파일을 모두 삭제
    /// </summary>
    public static void DeleteOldFiles(string parentPath, string fileFilter, int oldDays)
    {
        if (Directory.Exists(parentPath))
        {
            DateTime today = DateTime.Today;
            string[] entries = Directory.GetFiles(parentPath, fileFilter, SearchOption.TopDirectoryOnly);
            foreach (string entry in entries)
            {
                string path = entry.Replace('\\', '/');
                int days = (today - File.GetLastWriteTime(path)).Days;
                if (days > oldDays)
                {
                    Debug.Log(string.Format("Delete file : {0} ({1} days old)", path, days));
                    try
                    {
                        File.Delete(path);
                    }
                    catch (Exception)
                    {
                        Debug.LogWarning(string.Format("Failed to delete file : {0}", path));
                    }
                }
            }
        }
    }

    /// <summary>
    /// PlayerPrefs에서 설정을 로드
    /// </summary>
    public static void Load()
    {
        // 사용자 설정
        //KioskNumber = PlayerPrefs.GetInt("KioskNumber", KioskNumber);
        //TitleDelayTime = PlayerPrefs.GetFloat("TitleDelayTime", TitleDelayTime);
        //HowToPlayTime = PlayerPrefs.GetFloat("HowToPlayTime", HowToPlayTime);
        //SelectLevelTime = PlayerPrefs.GetFloat("SelectLevelTime", SelectLevelTime);
        //FinishWaitTime = PlayerPrefs.GetFloat("FinishWaitTime", FinishWaitTime);
        Debug.Log("Configuration loaded from PlayerPrefs");
    }

    /// <summary>
    /// PlayerPrefs에 설정을 저장
    /// </summary>
    public static void Save()
    {
        // 사용자 설정
        //PlayerPrefs.SetInt("KioskNumber", KioskNumber);
        //PlayerPrefs.SetFloat("TitleDelayTime", TitleDelayTime);
        //PlayerPrefs.SetFloat("HowToPlayTime", HowToPlayTime);
        //PlayerPrefs.SetFloat("SelectLevelTime", SelectLevelTime);
        //PlayerPrefs.SetFloat("FinishWaitTime", FinishWaitTime);
        PlayerPrefs.Save();
        Debug.Log("Configuration saved to PlayerPrefs");
    }

    /// <summary>
    /// 환경설정 XML 파일 이름
    /// </summary>
    private const string ConfigXmlName = "ProjectSettings.xml";

    /// <summary>
    /// XML 파일에서 설정을 로드
    /// </summary>
    public static bool LoadFromXml()
    {
        string path = string.Format("{0}/{1}", ApplicationPath, ConfigXmlName);
        try
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            // 시스템 설정
            XmlNode root = doc.SelectSingleNode("Environment");
            if (root != null)
            {
                XmlNode signalCtrl = root.SelectSingleNode("SignalCtrl");
                if (signalCtrl != null)
                {
                    SignalWebSocketUrl = signalCtrl.ReadString("WebSocket", SignalWebSocketUrl);
                }
                XmlNode doorRemote = root.SelectSingleNode("DoorRemote");
                if (doorRemote != null)
                {
                    SerialPortName = doorRemote.ReadString("SerialPortName", SerialPortName);
                    SerialBoundRate = doorRemote.ReadInt("BoudRate", SerialBoundRate);
                }
                XmlNode sendFile = root.SelectSingleNode("SendFile");
                if(sendFile != null)
                {
                    WebSocketFileUrl = root.ReadString("SendFile", WebSocketFileUrl);
                }
                XmlNode AssetBundle = root.SelectSingleNode("AssetBundle");
                if(AssetBundle != null)
                {
                    AssetBundlePath = root.ReadString("AssetBundlePath", AssetBundlePath);
                }
                XmlNode Project = root.SelectSingleNode("Project");
                if (Project != null)
                {
                    BackToTitleTime = root.ReadFloat("BackToTitleTime", BackToTitleTime);
                    FinishTime = root.ReadFloat("FinishTime", FinishTime);
                    TargetTime = root.ReadFloat("TargetTime", TargetTime);
                }
            }
            Debug.Log(string.Format("Configuration loaded from {0}", ConfigXmlName));
            return true;
        }
        catch (Exception ex)
        {
            Debug.Log(string.Format("Failed to load configuration : {0}", ex.Message));
        }
        return false;
    }

    /// <summary>
    /// 설정을 XML 파일에 저장
    /// </summary>
    /// <returns></returns>
    public static bool SaveToXml()
    {
        string path = string.Format("{0}/{1}", ApplicationPath, ConfigXmlName);
        try
        {
            XmlDocument doc = new XmlDocument();
            doc.AppendChild(doc.CreateXmlDeclaration("1.0", "utf-8", "yes"));
            // 시스템 설정
            XmlNode root = doc.AppendChild(doc.CreateElement("Environment"));
            if (root != null)
            {
                XmlNode signalCtrl = root.AppendChild(doc.CreateElement("SignalCtrl"));
                if (signalCtrl != null)
                {
                    signalCtrl.WriteString("WebSocket", SignalWebSocketUrl);
                }
                XmlNode doorRemote = root.AppendChild(doc.CreateElement("DoorRemote"));
                if (doorRemote != null)
                {
                    doorRemote.WriteString("SerialPortName", SerialPortName);
                    doorRemote.WriteInt("BoudRate", SerialBoundRate);
                }
                XmlNode sendFile = root.AppendChild(doc.CreateElement("SendFile"));
                if (sendFile != null)
                {
                    sendFile.WriteString("SendFile", WebSocketFileUrl);
                }
                XmlNode assetBundle = root.AppendChild(doc.CreateElement("AssetBundle"));
                if (assetBundle != null)
                {
                    assetBundle.WriteString("AssetBundlePath", AssetBundlePath);
                }
                XmlNode Project = root.AppendChild(doc.CreateElement("Project"));
                if (Project != null)
                {
                    Project.WriteFloat("BackToTitleTime", BackToTitleTime);
                    Project.WriteFloat("FinishTime", FinishTime);
                    Project.WriteFloat("TargetTime", TargetTime);
                }
            }
            doc.Save(path);
            Debug.Log(string.Format("Configuration saved to {0}", ConfigXmlName));
            return true;
        }
        catch (Exception ex)
        {
            Debug.Log(string.Format("Failed to save configuration : {0}", ex.Message));
        }
        return false;
    }
}
