using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebServerData
{
    public static string userId;
    public static string userName;
    public static string studentId; 
    public static string initialTime;
    public static string characterType;
    public static Texture2D facialExpression1;
    public static Texture2D facialExpression2;
    public static Texture2D facialExpression3;
    public static Texture2D facialExpression4;
    public static Texture2D materialTexture1;
    public static Texture2D materialTexture2;
    public static Texture2D screenshotImage;
    public static string friendName;
    public static string villainName;
    public static string bgName;
    public static string scenarioText;
    public static string motionData1;
    public static string motionData2;
    public static string motionData3;
    public static string motionData4;
    public static string motionData5;
    public static byte[] videoFile;

    public static BodyDataList[] dataArray = new BodyDataList[5];

    public static int captureIndex = 0;

    public static string BaseUrl = "http://192.168.2.196:80";

    public static string[] GetUrls = { "/E1Get", "/E2Get", "/E3Get", "/E4Get", "/E5Get" };
    public static string[] PostUrls = { "/E1Post/", "/E2Post/", "/E3Post/", "/E4Post/" };
    public static string[] data_types = { "facial_expression", "material_texture", "screenshot_image", "video_file" };


    public static void Clear()
    {
        userId = "";
        userName = "";
        studentId = "";
        initialTime = "";
        characterType = "";
        facialExpression1 = null;
        facialExpression2 = null;
        facialExpression3 = null;
        facialExpression4 = null;
        materialTexture1 = null;
        materialTexture2 = null;
        screenshotImage = null;
        friendName = "";
        villainName = "";
        bgName = "";
        scenarioText = "";
        motionData1 = "";
        motionData2 = "";
        motionData3 = "";
        motionData4 = "";
        motionData5 = "";
        videoFile = null;

        for(int i = 0; i<5; i++)
        {
            dataArray[i].Clear();
            Debug.Log(i + " Cleared");
        }
    }
}
