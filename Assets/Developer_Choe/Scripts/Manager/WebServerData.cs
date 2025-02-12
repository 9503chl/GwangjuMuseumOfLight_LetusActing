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
    public static Texture2D materialTexture1;
    public static Texture2D materialTexture2;
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

    public static List<Texture2D> texture2Ds = new List<Texture2D>() { facialExpression1, materialTexture1, materialTexture2 }; 

    public static int captureIndex = 0;

    public static string GetUrls = "http://101.101.219.13:8080/ArtStationExternalAPI/jux/E1Get.do";
    public static string[] PostUrls = { "http://101.101.219.13:8080/ArtStationExternalAPI/jux/E1Post.do",
                                        "http://101.101.219.13:8080/ArtStationExternalAPI/jux/E2Post.do",
                                        "http://101.101.219.13:8080/ArtStationExternalAPI/jux/E3Post.do",
                                        "http://101.101.219.13:8080/ArtStationExternalAPI/jux/E4Post.do" };

    public static void Clear()
    {
        userId = "";
        userName = "";
        studentId = "";
        initialTime = "";
        characterType = "";
        facialExpression1 = null;
        materialTexture1 = null;
        materialTexture2 = null;
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
