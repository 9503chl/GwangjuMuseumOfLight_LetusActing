using System.IO;
using UnityEngine;

public class DataSettings
{
    public static string PathName;

    public static string PlayerID;
    public static string PlayerName;
    public static string StudentID;

    public static BodyDataList[] dataArray = new BodyDataList[5];

    public static int captureIndex = 0;

    public static float TargetTime = 3;

    public static void Clear()
    {
        PlayerID = string.Empty;
        PlayerName = string.Empty;
        StudentID = string.Empty;
        for (int i = 0; i < dataArray.Length; i++)
        {
            dataArray[i].Clear();
        }
    }
}