using System;
using UnityEngine;

[Serializable]
public class HokuyoSensorInfo
{
    public string IPAddress = "192.168.0.10";
    public int PortNumber = 10940;
    public Vector2Int RectSize = Vector2Int.zero;
    public Vector2Int RectOffset = Vector2Int.zero;
    public Vector2Int ScreenSize = Vector2Int.zero;
    public Vector2Int ScreenOffset = Vector2Int.zero;

    public bool HasIPAddress
    {
        get { return !string.IsNullOrEmpty(IPAddress); }
    }

    public bool HasRectSize
    {
        get { return RectSize != Vector2Int.zero; }
    }

    public bool HasScreenSize
    {
        get { return ScreenSize != Vector2Int.zero; }
    }

    public HokuyoSensorInfo()
    {
    }

    public HokuyoSensorInfo(string ipAddress, int portNumber, Vector2Int rectSize, Vector2Int rectOffset)
    {
        IPAddress = ipAddress;
        PortNumber = portNumber;
        RectSize = rectSize;
        RectOffset = rectOffset;
    }

    public HokuyoSensorInfo(string ipAddress, int portNumber, Vector2Int rectSize, Vector2Int rectOffset, Vector2Int screenSize, Vector2Int screenOffset)
    {
        IPAddress = ipAddress;
        PortNumber = portNumber;
        RectSize = rectSize;
        RectOffset = rectOffset;
        ScreenSize = screenSize;
        ScreenOffset = screenOffset;
    }
}
