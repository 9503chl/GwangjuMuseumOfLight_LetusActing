using System;
using System.IO;
using System.Xml;
using UnityEngine;

public class HokuyoSensorSettings
{
    /// <summary>
    /// Hokuyo 센서 정보
    /// </summary>
    public static HokuyoSensorInfo[] HokuyoSensors = new HokuyoSensorInfo[3]
    {
        new HokuyoSensorInfo("192.168.0.10", 10940, new Vector2Int(7680, 2400), new Vector2Int(3840, 1200)),
        new HokuyoSensorInfo("192.168.0.11", 10940, new Vector2Int(7680, 2400), new Vector2Int(3840, 1200)),
        new HokuyoSensorInfo("192.168.0.12", 10940, new Vector2Int(7680, 2400), new Vector2Int(3840, 1200))
    };

    /// <summary>
    /// 프로그램 실행 경로
    /// </summary>
    private static string ApplicationPath
    {
        get
        {
            // 특정 상황에서 Application.dataPath가 변경되는 증상 때문에 Application.streamingAssetsPath를 사용
            //return Path.GetDirectoryName(Application.dataPath).Replace('\\', '/');
            return Path.GetDirectoryName(Path.GetDirectoryName(Application.streamingAssetsPath)).Replace('\\', '/');
        }
    }

    /// <summary>
    /// 환경설정 XML 파일 이름
    /// </summary>
    private const string ConfigXmlName = "HokuyoSensorSettings.xml";

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
            XmlNode root = doc.SelectSingleNode("Settings");
            if (root != null)
            {
                for (int i = 0; i < HokuyoSensors.Length; i++)
                {
                    XmlNode hokuyo = root.SelectSingleNode(string.Format("HokuyoSensor{0}", i + 1));
                    if (hokuyo != null)
                    {
                        HokuyoSensors[i].IPAddress = hokuyo.ReadString("IPAddress", HokuyoSensors[i].IPAddress);
                        HokuyoSensors[i].PortNumber = hokuyo.ReadInt("PortNumber", HokuyoSensors[i].PortNumber);
                        HokuyoSensors[i].RectSize.x = hokuyo.ReadInt("RectWidth", HokuyoSensors[i].RectSize.x);
                        HokuyoSensors[i].RectSize.y = hokuyo.ReadInt("RectHeight", HokuyoSensors[i].RectSize.y);
                        HokuyoSensors[i].RectOffset.x = hokuyo.ReadInt("RectOffsetX", HokuyoSensors[i].RectOffset.x);
                        HokuyoSensors[i].RectOffset.y = hokuyo.ReadInt("RectOffsetY", HokuyoSensors[i].RectOffset.y);
                        HokuyoSensors[i].ScreenSize.x = hokuyo.ReadInt("ScreenWidth", HokuyoSensors[i].ScreenSize.x);
                        HokuyoSensors[i].ScreenSize.y = hokuyo.ReadInt("ScreenHeight", HokuyoSensors[i].ScreenSize.y);
                        HokuyoSensors[i].ScreenOffset.x = hokuyo.ReadInt("ScreenOffsetX", HokuyoSensors[i].ScreenOffset.x);
                        HokuyoSensors[i].ScreenOffset.y = hokuyo.ReadInt("ScreenOffsetY", HokuyoSensors[i].ScreenOffset.y);
                    }
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
        string backupPath = Path.ChangeExtension(path, "bak");
        if (File.Exists(path))
        {
            try
            {
                File.Copy(path, backupPath, true);
            }
            catch (Exception)
            {
            }
        }
        try
        {
            XmlDocument doc = new XmlDocument();
            doc.AppendXmlDeclaration();
            XmlNode root = doc.AppendElement("Settings");
            if (root != null)
            {
                for (int i = 0; i < HokuyoSensors.Length; i++)
                {
                    XmlNode hokuyo = root.AppendElement(string.Format("HokuyoSensor{0}", i + 1));
                    if (hokuyo != null)
                    {
                        hokuyo.WriteString("IPAddress", HokuyoSensors[i].IPAddress);
                        hokuyo.WriteInt("PortNumber", HokuyoSensors[i].PortNumber);
                        hokuyo.WriteInt("RectWidth", HokuyoSensors[i].RectSize.x);
                        hokuyo.WriteInt("RectHeight", HokuyoSensors[i].RectSize.y);
                        hokuyo.WriteInt("RectOffsetX", HokuyoSensors[i].RectOffset.x);
                        hokuyo.WriteInt("RectOffsetY", HokuyoSensors[i].RectOffset.y);
                        hokuyo.WriteInt("ScreenWidth", HokuyoSensors[i].ScreenSize.x);
                        hokuyo.WriteInt("ScreenHeight", HokuyoSensors[i].ScreenSize.y);
                        hokuyo.WriteInt("ScreenOffsetX", HokuyoSensors[i].ScreenOffset.x);
                        hokuyo.WriteInt("ScreenOffsetY", HokuyoSensors[i].ScreenOffset.y);
                    }
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
