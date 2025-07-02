using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebServerData
{
    public string status;
    public string userInfo;
    public string userId;
    public string userName;
    public string studentId;
    public string characterType;
    public string facialExpression1Url;
    public string facialExpression2Url;
    public string facialExpression3Url;
    public string facialExpression4Url;
    public Texture2D facialExpression1;
    public Texture2D facialExpression2;
    public Texture2D facialExpression3;
    public Texture2D facialExpression4;
    public string materialTexture1Url;
    public string materialTexture2Url;
    public Texture2D materialTexture1;
    public Texture2D materialTexture2;
    public string screenshotImageUrl;
    public Texture2D screenshotImage;
    public string friendName;
    public string villainName;
    public string bgName;
    public string scenarioText;
    public string motionData1Url;
    public string motionData2Url;
    public string motionData3Url;
    public string motionData4Url;
    public string motionData5Url;
    public string motionData1;
    public string motionData2;
    public string motionData3;
    public string motionData4;
    public string motionData5;
    public string videoFileUrl;
    public string videoThumbnailUrl;
    public Texture2D videoThumbnail;

    public int card_index_1;
    public int card_index_2;
    public int card_index_3;
    public int card_index_4;

    public string message;
    public string errorCode;

    public bool success
    {
        get
        {
            return !string.IsNullOrEmpty(status) && string.Compare(status, "success") == 0;
        }
    }

    public bool hasFacialExpressions
    {
        get
        {
            return facialExpression1 != null && facialExpression2 != null && facialExpression3 != null && facialExpression4 != null;
        }
    }

    public bool hasMaterialTextures
    {
        get
        {
            return materialTexture1 != null && materialTexture2 != null;
        }
    }

    public bool hasMotionDatas
    {
        get
        {
            return !string.IsNullOrEmpty(motionData1) && !string.IsNullOrEmpty(motionData2) && !string.IsNullOrEmpty(motionData3) && !string.IsNullOrEmpty(motionData4) && !string.IsNullOrEmpty(motionData5);
        }
    }

    public bool hasVideoAndThumbnail
    {
        get
        {
            return !string.IsNullOrEmpty(videoFileUrl) && videoThumbnail != null;
        }
    }

    public void Clear()
    {
        status = null;
        userInfo = null;
        userId = null;
        userName = null;
        studentId = null;
        characterType = null;
        facialExpression1Url = null;
        facialExpression2Url = null;
        facialExpression3Url = null;
        facialExpression4Url = null;
        if (facialExpression1 != null)
        {
            UnityEngine.Object.Destroy(facialExpression1);
            facialExpression1 = null;
        }
        if (facialExpression2 != null)
        {
            UnityEngine.Object.Destroy(facialExpression2);
            facialExpression2 = null;
        }
        if (facialExpression3 != null)
        {
            UnityEngine.Object.Destroy(facialExpression3);
            facialExpression3 = null;
        }
        if (facialExpression4 != null)
        {
            UnityEngine.Object.Destroy(facialExpression4);
            facialExpression4 = null;
        }
        materialTexture1Url = null;
        materialTexture2Url = null;
        if (materialTexture1 != null)
        {
            UnityEngine.Object.Destroy(materialTexture1);
            materialTexture1 = null;
        }
        if (materialTexture2 != null)
        {
            UnityEngine.Object.Destroy(materialTexture2);
            materialTexture2 = null;
        }
        screenshotImageUrl = null;
        if (screenshotImage != null)
        {
            UnityEngine.Object.Destroy(screenshotImage);
            screenshotImage = null;
        }
        friendName = null;
        villainName = null;
        bgName = null;
        scenarioText = null;
        motionData1Url = null;
        motionData2Url = null;
        motionData3Url = null;
        motionData4Url = null;
        motionData5Url = null;
        motionData1 = null;
        motionData2 = null;
        motionData3 = null;
        motionData4 = null;
        motionData5 = null;
        videoFileUrl = null;
        videoThumbnailUrl = null;
        card_index_1 = -1;
        card_index_2 = -1;
        card_index_3 = -1;
        card_index_4 = -1;

        if (videoThumbnail != null)
        {
            UnityEngine.Object.Destroy(videoThumbnail);
            videoThumbnail = null;
        }

        message = null;
        errorCode = null;
    }
}